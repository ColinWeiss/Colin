using Assimp;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using PrimitiveType = Assimp.PrimitiveType;

namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// glTF 2.0 / GLB 导入: Assimp 解析 -> 引擎运行时的 PbrModel.
  /// <br>[!] 矩阵约定: Assimp 为列向量 (平移在第四列), XNA 为行向量 (平移在第四行), 两边互转需要转置;
  /// 转置后世界变换为 local * parent, 蒙皮骨骼矩阵为 绑定逆矩阵 * 骨骼世界.</br>
  /// <br>[!] UV 约定: glTF 纹理坐标原点在图像左上角, 但 Assimp 统一成左下角原点, 因此需要 FlipUVs;
  /// 翻转后排 UV 与 Assimp 的切线空间不再匹配, 所以切线由本导入器按最终 UV 重新计算.</br>
  /// </summary>
  internal static class PbrModelImporter
  {
    private static readonly PostProcessSteps ImportSteps =
        PostProcessSteps.Triangulate |
        PostProcessSteps.GenerateNormals |
        PostProcessSteps.FlipUVs |
        PostProcessSteps.LimitBoneWeights;

    public static PbrModel Import(GraphicsDevice device, string path)
    {
      if (File.Exists(path) is false)
        throw new FileNotFoundException($"PBR: 模型文件不存在 '{path}'", path);

      using AssimpContext context = new AssimpContext();
      Assimp.Scene scene = context.ImportFile(path, ImportSteps);
      if (scene is null || scene.RootNode is null)
        throw new InvalidDataException($"PBR: Assimp 无法读取模型 '{path}'");

      PbrModel model = new PbrModel
      {
        Name = Path.GetFileNameWithoutExtension(path),
        SourcePath = Path.GetFullPath(path)
      };
      PbrTextureCache textures = new PbrTextureCache(device, scene, path);
      model.TextureCache = textures;

      Dictionary<string, int> nodeIndices = new Dictionary<string, int>(StringComparer.Ordinal);
      AddNode(model, scene.RootNode, -1, nodeIndices);
      model.BuildIndexes();
      model.UpdateNodeWorlds();

      List<PbrMaterial> materials = new List<PbrMaterial>(scene.MaterialCount);
      for (int i = 0; i < scene.MaterialCount; i++)
        materials.Add(BuildMaterial(scene.Materials[i], textures));
      PbrMaterial defaultMaterial = new PbrMaterial { Name = "Default" };
      model.Materials.AddRange(materials);

      Dictionary<int, PbrVertex[]> cpuVertices = new Dictionary<int, PbrVertex[]>();
      for (int i = 0; i < scene.MeshCount; i++)
      {
        Assimp.Mesh mesh = scene.Meshes[i];
        if (mesh.PrimitiveType != PrimitiveType.Triangle)
        {
          Console.Log(ConsoleTextType.Warning, "PBR", $"跳过非三角形网格 '{mesh.Name}' ({mesh.PrimitiveType})");
          continue;
        }

        PbrMaterial material = mesh.MaterialIndex >= 0 && mesh.MaterialIndex < materials.Count
            ? materials[mesh.MaterialIndex]
            : defaultMaterial;
        PbrMesh pbrMesh = BuildMesh(device, mesh, material, nodeIndices, out PbrVertex[] vertices);
        if (pbrMesh is null)
          continue;
        model.Meshes.Add(pbrMesh);
        cpuVertices[model.Meshes.Count - 1] = vertices;
      }
      model.MeshRemap = BuildMeshRemap(scene);

      foreach (Animation animation in scene.Animations)
      {
        PbrAnimationClip clip = BuildAnimation(animation, nodeIndices);
        if (clip is null || clip.ChannelCount == 0)
          continue;
        string name = string.IsNullOrWhiteSpace(clip.Name) ? $"Animation{model.Animations.Count}" : clip.Name;
        while (model.Animations.ContainsKey(name))
          name += "_";
        model.Animations[name] = clip;
      }

      model.ResetToBindPose();
      model.UpdateNodeWorlds();
      model.UpdateSkins();
      model.Bounds = ComputeBounds(model, scene, cpuVertices);
      return model;
    }

    private static int[] BuildMeshRemap(Assimp.Scene scene)
    {
      int[] remap = new int[scene.MeshCount];
      int next = 0;
      for (int i = 0; i < scene.MeshCount; i++)
      {
        Assimp.Mesh mesh = scene.Meshes[i];
        if (mesh.PrimitiveType != PrimitiveType.Triangle)
          remap[i] = -1;
        else
          remap[i] = next++;
      }
      return remap;
    }

    private static void AddNode(PbrModel model, Assimp.Node node, int parentIndex, Dictionary<string, int> nodeIndices)
    {
      PbrNode pbrNode = new PbrNode
      {
        Name = node.Name ?? string.Empty,
        Index = model.Nodes.Count,
        ParentIndex = parentIndex
      };

      Matrix local = ToXna(node.Transform);
      if (local.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation))
      {
        pbrNode.BindScale = scale;
        pbrNode.BindRotation = Quaternion.Normalize(rotation);
        pbrNode.BindTranslation = translation;
      }
      else
      {
        pbrNode.BindTranslation = local.Translation;
        Console.Log(ConsoleTextType.Warning, "PBR", $"节点 '{pbrNode.Name}' 的变换含切变/负缩放, 已退化为平移.");
      }
      pbrNode.ResetToBindPose();
      pbrNode.MeshIndices.AddRange(node.MeshIndices);

      model.Nodes.Add(pbrNode);
      if (parentIndex >= 0)
        model.Nodes[parentIndex].Children.Add(pbrNode.Index);

      if (nodeIndices.ContainsKey(pbrNode.Name) is false)
        nodeIndices[pbrNode.Name] = pbrNode.Index;

      foreach (Assimp.Node child in node.Children)
        AddNode(model, child, pbrNode.Index, nodeIndices);
    }

    private static PbrMesh BuildMesh(
        GraphicsDevice device,
        Assimp.Mesh mesh,
        PbrMaterial material,
        Dictionary<string, int> nodeIndices,
        out PbrVertex[] vertices)
    {
      int count = mesh.VertexCount;
      vertices = new PbrVertex[count];

      int[] boneNodeIndices = new int[mesh.BoneCount];
      Matrix[] boneOffsets = new Matrix[mesh.BoneCount];
      for (int i = 0; i < mesh.BoneCount; i++)
      {
        Bone bone = mesh.Bones[i];
        boneOffsets[i] = ToXna(bone.OffsetMatrix);
        boneNodeIndices[i] = nodeIndices.TryGetValue(bone.Name ?? string.Empty, out int nodeIndex) ? nodeIndex : -1;
        if (boneNodeIndices[i] < 0)
          Console.Log(ConsoleTextType.Warning, "PBR", $"网格 '{mesh.Name}' 的骨骼 '{bone.Name}' 在节点树中不存在.");
      }

      int[] influenceCount = new int[count];
      int[,] influenceSlots = new int[count, 4];
      float[,] influenceWeights = new float[count, 4];
      for (int boneSlot = 0; boneSlot < mesh.BoneCount; boneSlot++)
      {
        if (boneNodeIndices[boneSlot] < 0)
          continue;
        Bone bone = mesh.Bones[boneSlot];
        foreach (VertexWeight weight in bone.VertexWeights)
        {
          int vertex = weight.VertexID;
          float value = weight.Weight;
          if (vertex < 0 || vertex >= count || value <= 0f)
            continue;

          int influence = influenceCount[vertex];
          if (influence < 4)
          {
            influenceSlots[vertex, influence] = boneSlot;
            influenceWeights[vertex, influence] = value;
            influenceCount[vertex] = influence + 1;
            continue;
          }

          int smallest = 0;
          for (int i = 1; i < 4; i++)
          {
            if (influenceWeights[vertex, i] < influenceWeights[vertex, smallest])
              smallest = i;
          }
          if (value > influenceWeights[vertex, smallest])
          {
            influenceSlots[vertex, smallest] = boneSlot;
            influenceWeights[vertex, smallest] = value;
          }
        }
      }

      int[] indicesArray = mesh.GetIndices().ToArray();
      if (indicesArray.Length < 3)
        return null;

      bool hasUv = mesh.HasTextureCoords(0);
      bool hasNormals = mesh.HasNormals;
      Vector3[] vertexPositions = new Vector3[count];
      Vector2[] vertexUvs = new Vector2[count];
      Vector3[] normals = new Vector3[count];
      for (int i = 0; i < count; i++)
      {
        vertexPositions[i] = ToXna(mesh.Vertices[i]);
        vertexUvs[i] = hasUv
            ? new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y)
            : Vector2.Zero;
        normals[i] = hasNormals ? ToXna(mesh.Normals[i]) : Vector3.Up;
        if (normals[i].LengthSquared() < 1e-8f)
          normals[i] = Vector3.Up;
        normals[i].Normalize();
      }

      (Vector3[] tangentSums, Vector3[] bitangentSums) = ComputeTangentSpace(vertexPositions, vertexUvs, indicesArray);

      for (int i = 0; i < count; i++)
      {
        Vector3 position = vertexPositions[i];
        Vector3 normal = normals[i];
        Vector2 texCoord = vertexUvs[i];

        Vector3 tangent = tangentSums[i];
        Vector3 bitangent = bitangentSums[i];
        float tangentSign = 1f;
        if (tangent.LengthSquared() < 1e-8f)
        {
          tangent = Vector3.Cross(normal, MathF.Abs(normal.Y) < 0.9f ? Vector3.Up : Vector3.Right);
          bitangent = Vector3.Cross(normal, tangent);
        }
        if (tangent.LengthSquared() < 1e-8f)
          tangent = Vector3.Cross(normal, Vector3.Right);
        tangent.Normalize();
        if (Vector3.Dot(Vector3.Cross(normal, tangent), bitangent) < 0f)
          tangentSign = -1f;

        PbrBoneIndices indices = default;
        Vector4 weights = Vector4.Zero;
        if (mesh.HasBones && influenceCount[i] > 0)
        {
          indices = new PbrBoneIndices(
              (short)influenceSlots[i, 0],
              (short)influenceSlots[i, 1],
              (short)influenceSlots[i, 2],
              (short)influenceSlots[i, 3]);
          weights = new Vector4(
              influenceWeights[i, 0],
              influenceWeights[i, 1],
              influenceWeights[i, 2],
              influenceWeights[i, 3]);
        }

        vertices[i] = new PbrVertex(
            position,
            normal,
            new Vector4(tangent.X, tangent.Y, tangent.Z, tangentSign),
            texCoord,
            indices,
            weights);
      }

      return new PbrMesh(device, mesh.Name, vertices, indicesArray, material, boneOffsets, boneNodeIndices);
    }

    /// <summary>按最终 UV 逐三角形累积切线/副切线 (UV 已 FlipUVs, 不能用 Assimp 的切线).</summary>
    private static (Vector3[] tangents, Vector3[] bitangents) ComputeTangentSpace(
        Vector3[] positions, Vector2[] uvs, int[] indices)
    {
      Vector3[] tangentSums = new Vector3[positions.Length];
      Vector3[] bitangentSums = new Vector3[positions.Length];
      for (int i = 0; i + 2 < indices.Length; i += 3)
      {
        int i0 = indices[i];
        int i1 = indices[i + 1];
        int i2 = indices[i + 2];
        Vector3 edge1 = positions[i1] - positions[i0];
        Vector3 edge2 = positions[i2] - positions[i0];
        Vector2 uv1 = uvs[i1] - uvs[i0];
        Vector2 uv2 = uvs[i2] - uvs[i0];
        float determinant = uv1.X * uv2.Y - uv2.X * uv1.Y;
        if (MathF.Abs(determinant) < 1e-12f)
          continue;
        float r = 1f / determinant;
        Vector3 tangent = (edge1 * uv2.Y - edge2 * uv1.Y) * r;
        Vector3 bitangent = (edge2 * uv1.X - edge1 * uv2.X) * r;
        tangentSums[i0] += tangent;
        tangentSums[i1] += tangent;
        tangentSums[i2] += tangent;
        bitangentSums[i0] += bitangent;
        bitangentSums[i1] += bitangent;
        bitangentSums[i2] += bitangent;
      }
      return (tangentSums, bitangentSums);
    }

    private static PbrMaterial BuildMaterial(Assimp.Material material, PbrTextureCache textures)
    {
      PbrMaterial result = new PbrMaterial { Name = material.Name ?? string.Empty };

      // 只有 glTF specular-glossiness 扩展会写 glossinessFactor; 老 Blinn/Phong 材质也有 specular 颜色,
      // 不能只因存在 $clr.specular 就当成 SG 材质, 否则会把普通高光材质渲染成金属.
      bool specularGlossiness = material.IsPBRMaterial is false
          && material.HasNonTextureProperty("$mat.glossinessFactor");
      result.Workflow = specularGlossiness
          ? PbrMaterialWorkflow.SpecularGlossiness
          : PbrMaterialWorkflow.MetallicRoughness;

      string baseColorPath = null;
      if (material.IsPBRMaterial && material.PBR.HasTextureBaseColor)
        baseColorPath = material.PBR.TextureBaseColor.FilePath;
      else if (material.HasTextureDiffuse)
        baseColorPath = material.TextureDiffuse.FilePath;
      result.BaseColorTexture = textures.Load(baseColorPath);

      if (specularGlossiness)
      {
        result.BaseColorFactor = GetColor(material, "$clr.diffuse", Vector4.One);
        Vector4 specular = GetColor(material, "$clr.specular", new Vector4(0.04f, 0.04f, 0.04f, 1f));
        result.SpecularFactor = new Vector3(specular.X, specular.Y, specular.Z);
        result.GlossinessFactor = GetFloat(material, "$mat.glossinessFactor", 1f);
        result.MetallicRoughnessTexture = material.HasTextureSpecular
            ? textures.Load(material.TextureSpecular.FilePath)
            : null;
      }
      else
      {
        result.BaseColorFactor = GetColor(material, "$clr.base", GetColor(material, "$clr.diffuse", Vector4.One));
        result.MetallicFactor = GetFloat(material, "$mat.metallicFactor", 1f);
        result.RoughnessFactor = GetFloat(material, "$mat.roughnessFactor", 1f);

        string workflowPath = null;
        if (material.IsPBRMaterial && material.PBR.HasTextureMetalness)
          workflowPath = material.PBR.TextureMetalness.FilePath;
        else if (material.IsPBRMaterial && material.PBR.HasTextureRoughness)
          workflowPath = material.PBR.TextureRoughness.FilePath;
        result.MetallicRoughnessTexture = textures.Load(workflowPath);

        if (material.IsPBRMaterial is false)
        {
          // Blinn/Phong 老材质: 金属度归零, 高光指数换算成粗糙度.
          result.MetallicFactor = 0f;
          float shininess = GetFloat(material, "$mat.shininess", 0f);
          result.RoughnessFactor = shininess > 0f
              ? MathHelper.Clamp(MathF.Sqrt(2f / (shininess + 2f)), 0.04f, 1f)
              : 0.8f;
          result.MetallicRoughnessTexture = null;
        }
      }

      string normalPath = null;
      if (material.HasTextureNormal)
        normalPath = material.TextureNormal.FilePath;
      else if (material.IsPBRMaterial && material.PBR.HasTextureNormalCamera)
        normalPath = material.PBR.TextureNormalCamera.FilePath;
      result.NormalTexture = textures.Load(normalPath);
      result.NormalScale = GetTextureFloatAny(material, "$tex.scale", 1f, TextureType.NormalCamera, TextureType.Normals);

      string occlusionPath = null;
      if (material.HasTextureAmbientOcclusion)
        occlusionPath = material.TextureAmbientOcclusion.FilePath;
      else if (material.HasTextureLightMap)
        occlusionPath = material.TextureLightMap.FilePath;
      result.OcclusionTexture = textures.Load(occlusionPath);
      result.OcclusionStrength = GetTextureFloatAny(material, "$tex.file.strength", 1f, TextureType.AmbientOcclusion, TextureType.Lightmap);

      string emissivePath = null;
      if (material.HasTextureEmissive)
        emissivePath = material.TextureEmissive.FilePath;
      else if (material.IsPBRMaterial && material.PBR.HasTextureEmissionColor)
        emissivePath = material.PBR.TextureEmissionColor.FilePath;
      result.EmissiveTexture = textures.Load(emissivePath);
      Vector4 emissive = GetColor(material, "$clr.emissive", Vector4.Zero);
      result.EmissiveFactor = new Vector3(emissive.X, emissive.Y, emissive.Z);
      result.EmissiveStrength = GetEmissiveStrength(material);

      result.AlphaCutoff = GetFloat(material, "$mat.gltf.alphaCutoff", 0.5f);
      result.AlphaMode = GetAlphaMode(material);
      result.DoubleSided = ReadTwoSided(material);
      return result;
    }

    private static bool ReadTwoSided(Assimp.Material material)
    {
      // AssimpNetter 6.0.5 的 Material.IsTwoSided 读 1 字节 Buffer 时会取成 0,
      // 所以直接读原始字节, 再退回属性.
      if (material.HasNonTextureProperty("$mat.twosided"))
      {
        MaterialProperty property = material.GetNonTextureProperty("$mat.twosided");
        if (property?.RawData is { Length: > 0 })
          return property.RawData[0] != 0;
      }
      if (material.HasNonTextureProperty("$mat.gltf.doubleSided"))
      {
        MaterialProperty property = material.GetNonTextureProperty("$mat.gltf.doubleSided");
        if (property?.RawData is { Length: > 0 })
          return property.RawData[0] != 0;
      }
      return material.IsTwoSided;
    }

    private static PbrAlphaMode GetAlphaMode(Assimp.Material material)
    {
      string mode = GetString(material, "$mat.gltf.alphaMode");
      if (string.IsNullOrEmpty(mode) is false)
      {
        if (mode.Equals("MASK", StringComparison.OrdinalIgnoreCase))
          return PbrAlphaMode.Mask;
        if (mode.Equals("BLEND", StringComparison.OrdinalIgnoreCase))
          return PbrAlphaMode.Blend;
        if (mode.Equals("OPAQUE", StringComparison.OrdinalIgnoreCase))
          return PbrAlphaMode.Opaque;
      }
      if (material.BlendMode != BlendMode.Default || material.Opacity < 1f)
        return PbrAlphaMode.Blend;
      return PbrAlphaMode.Opaque;
    }

    private static float GetEmissiveStrength(Assimp.Material material)
    {
      foreach (MaterialProperty property in material.GetAllProperties())
      {
        string name = property.Name ?? string.Empty;
        if (name.Contains("emissive", StringComparison.OrdinalIgnoreCase) is false)
          continue;
        if (name.Contains("strength", StringComparison.OrdinalIgnoreCase) is false
            && name.Contains("intensity", StringComparison.OrdinalIgnoreCase) is false)
          continue;
        try
        {
          if (property.ByteCount >= 4)
            return property.GetFloatValue();
        }
        catch
        {
          return 1f;
        }
      }
      return 1f;
    }

    private static Vector4 GetColor(Assimp.Material material, string name, Vector4 fallback)
    {
      if (material.HasNonTextureProperty(name) is false)
        return fallback;
      MaterialProperty property = material.GetNonTextureProperty(name);
      if (property is null)
        return fallback;
      if (property.ByteCount >= 16)
      {
        System.Numerics.Vector4 value = property.GetVector4Value();
        return new Vector4(value.X, value.Y, value.Z, value.W);
      }
      if (property.ByteCount >= 12)
      {
        System.Numerics.Vector3 value = property.GetVector3Value();
        return new Vector4(value.X, value.Y, value.Z, 1f);
      }
      return fallback;
    }

    private static float GetFloat(Assimp.Material material, string name, float fallback)
    {
      if (material.HasNonTextureProperty(name) is false)
        return fallback;
      try
      {
        MaterialProperty property = material.GetNonTextureProperty(name);
        return property is null || property.ByteCount < 4 ? fallback : property.GetFloatValue();
      }
      catch
      {
        return fallback;
      }
    }

    private static string GetString(Assimp.Material material, string name)
    {
      if (material.HasNonTextureProperty(name) is false)
        return null;
      try
      {
        return material.GetNonTextureProperty(name)?.GetStringValue();
      }
      catch
      {
        return null;
      }
    }

    private static float GetTextureFloat(Assimp.Material material, string name, TextureType type, float fallback)
    {
      try
      {
        MaterialProperty property = material.GetProperty(name, type, 0);
        if (property is not null && property.ByteCount >= 4)
          return property.GetFloatValue();
      }
      catch
      {
      }
      return fallback;
    }

    private static float GetTextureFloatAny(Assimp.Material material, string name, float fallback, params TextureType[] types)
    {
      for (int i = 0; i < types.Length; i++)
      {
        float value = GetTextureFloat(material, name, types[i], float.NaN);
        if (float.IsNaN(value) is false)
          return value;
      }
      return fallback;
    }

    private static PbrAnimationClip BuildAnimation(Animation animation, Dictionary<string, int> nodeIndices)
    {
      float ticksPerSecond = animation.TicksPerSecond > 0d ? (float)animation.TicksPerSecond : 1f;
      PbrAnimationClip clip = new PbrAnimationClip
      {
        Name = animation.Name ?? string.Empty,
        Duration = (float)(animation.DurationInTicks / ticksPerSecond)
      };

      List<PbrAnimationChannel> channels = new List<PbrAnimationChannel>();
      float maxKeyTime = 0f;
      foreach (NodeAnimationChannel source in animation.NodeAnimationChannels)
      {
        if (nodeIndices.TryGetValue(source.NodeName ?? string.Empty, out int nodeIndex) is false)
          continue;

        PbrAnimationChannel channel = new PbrAnimationChannel { NodeIndex = nodeIndex };
        if (source.PositionKeyCount > 0)
        {
          channel.PositionTimes = new float[source.PositionKeyCount];
          channel.Positions = new Vector3[source.PositionKeyCount];
          for (int i = 0; i < source.PositionKeyCount; i++)
          {
            VectorKey key = source.PositionKeys[i];
            channel.PositionTimes[i] = (float)(key.Time / ticksPerSecond);
            channel.Positions[i] = ToXna(key.Value);
          }
          maxKeyTime = MathF.Max(maxKeyTime, channel.PositionTimes[^1]);
        }
        if (source.RotationKeyCount > 0)
        {
          channel.RotationTimes = new float[source.RotationKeyCount];
          channel.Rotations = new Quaternion[source.RotationKeyCount];
          for (int i = 0; i < source.RotationKeyCount; i++)
          {
            QuaternionKey key = source.RotationKeys[i];
            channel.RotationTimes[i] = (float)(key.Time / ticksPerSecond);
            channel.Rotations[i] = Quaternion.Normalize(ToXna(key.Value));
          }
          maxKeyTime = MathF.Max(maxKeyTime, channel.RotationTimes[^1]);
        }
        if (source.ScalingKeyCount > 0)
        {
          channel.ScaleTimes = new float[source.ScalingKeyCount];
          channel.Scales = new Vector3[source.ScalingKeyCount];
          for (int i = 0; i < source.ScalingKeyCount; i++)
          {
            VectorKey key = source.ScalingKeys[i];
            channel.ScaleTimes[i] = (float)(key.Time / ticksPerSecond);
            channel.Scales[i] = ToXna(key.Value);
          }
          maxKeyTime = MathF.Max(maxKeyTime, channel.ScaleTimes[^1]);
        }
        channels.Add(channel);
      }

      if (clip.Duration <= 0f)
        clip.Duration = maxKeyTime;
      clip._channels = channels.ToArray();
      return clip;
    }

    private static BoundingBox ComputeBounds(
        PbrModel model,
        Assimp.Scene scene,
        Dictionary<int, PbrVertex[]> cpuVertices)
    {
      Vector3 min = new Vector3(float.MaxValue);
      Vector3 max = new Vector3(float.MinValue);
      bool any = false;
      HashSet<int> skinnedComputed = new HashSet<int>();

      foreach (PbrNode node in model.Nodes)
      {
        foreach (int sourceMeshIndex in node.MeshIndices)
        {
          int meshIndex = model.MeshRemap is not null && sourceMeshIndex >= 0 && sourceMeshIndex < model.MeshRemap.Length
              ? model.MeshRemap[sourceMeshIndex]
              : -1;
          if (meshIndex < 0 || meshIndex >= model.Meshes.Count)
            continue;
          PbrMesh mesh = model.Meshes[meshIndex];
          if (cpuVertices.TryGetValue(meshIndex, out PbrVertex[] vertices) is false)
            continue;

          BoundingBox meshBounds;
          if (mesh.IsSkinned)
          {
            if (skinnedComputed.Add(meshIndex) is false)
              continue;
            meshBounds = ComputeSkinnedBounds(mesh, vertices);
          }
          else
          {
            meshBounds = BoundingBox.CreateFromPoints(vertices.Select(v => Vector3.Transform(v.Position, node.World)));
          }

          min = Vector3.Min(min, meshBounds.Min);
          max = Vector3.Max(max, meshBounds.Max);
          any = true;
          mesh.BindBounds = meshBounds;
        }
      }

      if (any is false)
        return new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f));
      return new BoundingBox(min, max);
    }

    private static BoundingBox ComputeSkinnedBounds(PbrMesh mesh, PbrVertex[] vertices)
    {
      Vector3[] positions = new Vector3[vertices.Length];
      for (int i = 0; i < vertices.Length; i++)
      {
        PbrVertex vertex = vertices[i];
        Matrix blended = new Matrix();
        float weightSum = 0f;
        for (int influence = 0; influence < 4; influence++)
        {
          float weight = influence switch
          {
            0 => vertex.BlendWeights.X,
            1 => vertex.BlendWeights.Y,
            2 => vertex.BlendWeights.Z,
            _ => vertex.BlendWeights.W
          };
          if (weight <= 0f)
            continue;

          int slot = influence switch
          {
            0 => vertex.BlendIndices.X,
            1 => vertex.BlendIndices.Y,
            2 => vertex.BlendIndices.Z,
            _ => vertex.BlendIndices.W
          };
          if (slot < 0 || slot >= mesh.BoneMatrices.Length)
            continue;

          blended += mesh.BoneMatrices[slot] * weight;
          weightSum += weight;
        }

        positions[i] = weightSum > 0f
            ? Vector3.Transform(vertex.Position, blended * (1f / weightSum))
            : vertex.Position;
      }
      return BoundingBox.CreateFromPoints(positions);
    }

    internal static Matrix ToXna(System.Numerics.Matrix4x4 matrix)
    {
      return new Matrix(
          matrix.M11, matrix.M21, matrix.M31, matrix.M41,
          matrix.M12, matrix.M22, matrix.M32, matrix.M42,
          matrix.M13, matrix.M23, matrix.M33, matrix.M43,
          matrix.M14, matrix.M24, matrix.M34, matrix.M44);
    }

    internal static Vector3 ToXna(System.Numerics.Vector3 vector)
    {
      return new Vector3(vector.X, vector.Y, vector.Z);
    }

    internal static Quaternion ToXna(System.Numerics.Quaternion quaternion)
    {
      return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }
  }
}
