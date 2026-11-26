namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 一个 glTF/GLB 模型: 节点层次 + GPU 网格 + 材质 + 动画.
  /// <br>绑定姿势在导入时写入节点, 播放动画时先整体回绑再叠加通道, 因此切换/停止动画不会残留上一段姿态.</br>
  /// </summary>
  public sealed class PbrModel : IDisposable
  {
    public string Name = string.Empty;

    public string SourcePath = string.Empty;

    public Matrix Transform = Matrix.Identity;

    public readonly List<PbrNode> Nodes = new List<PbrNode>();

    public readonly List<PbrMesh> Meshes = new List<PbrMesh>();

    public readonly List<PbrMaterial> Materials = new List<PbrMaterial>();

    public readonly Dictionary<string, PbrAnimationClip> Animations =
        new Dictionary<string, PbrAnimationClip>(StringComparer.Ordinal);

    /// <summary>模型空间 (绑定姿势) 包围盒; 世界包围盒见 <see cref="GetWorldBounds"/>.</summary>
    public BoundingBox Bounds = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f));

    public PbrAnimationPlayer Player { get; private set; }

    public bool IsAnimated => Animations.Count > 0;

    public IReadOnlyCollection<string> AnimationNames => Animations.Keys;

    internal PbrTextureCache TextureCache;

    internal int[] MeshRemap;

    internal PbrNode[] NodeArray;

    public static PbrModel Load(GraphicsDevice device, string path) => PbrModelImporter.Import(device, path);

    public void BuildIndexes()
    {
      NodeArray = Nodes.ToArray();
    }

    public void ResetToBindPose()
    {
      for (int i = 0; i < Nodes.Count; i++)
        Nodes[i].ResetToBindPose();
    }

    public void UpdateNodeWorlds()
    {
      for (int i = 0; i < Nodes.Count; i++)
      {
        PbrNode node = Nodes[i];
        node.World = node.ParentIndex >= 0
            ? Matrix.Multiply(node.Local, Nodes[node.ParentIndex].World)
            : node.Local;
      }
    }

    public void UpdateSkins()
    {
      if (NodeArray is null)
        BuildIndexes();
      for (int i = 0; i < Meshes.Count; i++)
      {
        if (Meshes[i].IsSkinned)
          Meshes[i].UpdateBonePalette(NodeArray);
      }
    }

    /// <summary>推进动画. 无动画/未播放时为无操作.</summary>
    public void Update(float deltaSeconds)
    {
      if (Player is null || deltaSeconds <= 0f)
        return;
      if (Player.Advance(deltaSeconds) is false)
        return;

      ResetToBindPose();
      Player.Apply(this);
      UpdateNodeWorlds();
      UpdateSkins();
    }

    public bool Play(string animationName, bool loop = true)
    {
      if (Animations.TryGetValue(animationName, out PbrAnimationClip clip) is false)
      {
        Console.Log(ConsoleTextType.Warning, "PBR", $"模型 '{Name}' 没有动画 '{animationName}'");
        return false;
      }

      Player = new PbrAnimationPlayer(clip) { Loop = loop };
      ResetToBindPose();
      Player.Apply(this);
      UpdateNodeWorlds();
      UpdateSkins();
      return true;
    }

    public bool PlayFirstAnimation(bool loop = true)
    {
      if (Animations.Count == 0)
        return false;
      foreach (string name in Animations.Keys)
        return Play(name, loop);
      return false;
    }

    public void StopAnimation()
    {
      Player = null;
      ResetToBindPose();
      UpdateNodeWorlds();
      UpdateSkins();
    }

    public BoundingBox GetWorldBounds()
    {
      Vector3[] corners = Bounds.GetCorners();
      Vector3 min = new Vector3(float.MaxValue);
      Vector3 max = new Vector3(float.MinValue);
      for (int i = 0; i < corners.Length; i++)
      {
        Vector3 corner = Vector3.Transform(corners[i], Transform);
        min = Vector3.Min(min, corner);
        max = Vector3.Max(max, corner);
      }
      return new BoundingBox(min, max);
    }

    public BoundingSphere GetWorldSphere()
    {
      BoundingBox bounds = GetWorldBounds();
      Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
      float radius = (bounds.Max - bounds.Min).Length() * 0.5f;
      return new BoundingSphere(center, radius);
    }

    public void Draw(PbrRenderer renderer)
    {
      Draw(renderer, Transform);
    }

    /// <summary>用一个临时世界变换绘制 (多个实体共享同一个模型资产时用).</summary>
    public void Draw(PbrRenderer renderer, Matrix transform)
    {
      for (int i = 0; i < Nodes.Count; i++)
      {
        PbrNode node = Nodes[i];
        if (node.MeshIndices.Count == 0)
          continue;

        for (int m = 0; m < node.MeshIndices.Count; m++)
        {
          int sourceIndex = node.MeshIndices[m];
          int meshIndex = MeshRemap is not null && sourceIndex >= 0 && sourceIndex < MeshRemap.Length
              ? MeshRemap[sourceIndex]
              : -1;
          if (meshIndex < 0 || meshIndex >= Meshes.Count)
            continue;

          PbrMesh mesh = Meshes[meshIndex];
          Matrix world = mesh.IsSkinned ? transform : node.World * transform;
          renderer.SubmitMesh(mesh, world);
        }
      }
    }

    public void Dispose()
    {
      Player = null;
      for (int i = 0; i < Meshes.Count; i++)
        Meshes[i].Dispose();
      Meshes.Clear();
      TextureCache?.Dispose();
      TextureCache = null;
      Materials.Clear();
      Animations.Clear();
      Nodes.Clear();
      NodeArray = null;
    }
  }
}
