namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// PBR Effect 的参数绑定层. 着色器参数名与 PbrShader.fx 一一对应, 缺参数直接抛异常 (避免静默错渲染).
  /// </summary>
  public sealed class PbrEffect : IDisposable
  {
    private readonly EffectParameter _world;
    private readonly EffectParameter _viewProjection;
    private readonly EffectParameter _inverseViewProjection;
    private readonly EffectParameter _eyePosition;
    private readonly EffectParameter _environmentRotation;
    private readonly EffectParameter _dirLight0Direction;
    private readonly EffectParameter _dirLight0Color;
    private readonly EffectParameter _dirLight1Direction;
    private readonly EffectParameter _dirLight1Color;
    private readonly EffectParameter _dirLight2Direction;
    private readonly EffectParameter _dirLight2Color;
    private readonly EffectParameter _environmentTint;
    private readonly EffectParameter _environmentIntensity;
    private readonly EffectParameter _environmentTexture;
    private readonly EffectParameter _hasEnvironmentTexture;
    private readonly EffectParameter _exposure;
    private readonly EffectParameter _baseColorFactor;
    private readonly EffectParameter _metallicFactor;
    private readonly EffectParameter _roughnessFactor;
    private readonly EffectParameter _metallicOverride;
    private readonly EffectParameter _roughnessOverride;
    private readonly EffectParameter _specularFactor;
    private readonly EffectParameter _glossinessFactor;
    private readonly EffectParameter _emissiveFactor;
    private readonly EffectParameter _emissiveStrength;
    private readonly EffectParameter _normalScale;
    private readonly EffectParameter _normalMapFlipY;
    private readonly EffectParameter _occlusionStrength;
    private readonly EffectParameter _alphaCutoff;
    private readonly EffectParameter _alphaMode;
    private readonly EffectParameter _workflow;
    private readonly EffectParameter _gameLighting;
    private readonly EffectParameter _boneCount;
    private readonly EffectParameter _baseColorTexture;
    private readonly EffectParameter _metallicRoughnessTexture;
    private readonly EffectParameter _normalTexture;
    private readonly EffectParameter _occlusionTexture;
    private readonly EffectParameter _emissiveTexture;
    private readonly EffectParameter _boneTexture;
    private readonly Texture2D _white;
    private readonly Texture2D _black;
    private readonly Texture2D _flatNormal;
    private readonly Texture2D _identityBone;
    private readonly EffectTechnique _staticTechnique;
    private readonly EffectTechnique _skinnedTechnique;
    private readonly EffectTechnique _skyTechnique;

    public Effect Effect { get; }

    /// <summary>glTF 法线贴图为 OpenGL 约定 (+Y 朝上), D3D 采样方向相反, 默认翻转绿通道.</summary>
    public bool NormalMapFlipY = true;

    public PbrEffect(GraphicsDevice device)
    {
      Effect = new Effect(device, PbrEffectCompiler.Compile());

      _world = Require("World");
      _viewProjection = Require("ViewProjection");
      _inverseViewProjection = Require("InverseViewProjection");
      _eyePosition = Require("EyePosition");
      _environmentRotation = Require("EnvironmentRotation");
      _dirLight0Direction = Require("DirLight0Direction");
      _dirLight0Color = Require("DirLight0Color");
      _dirLight1Direction = Require("DirLight1Direction");
      _dirLight1Color = Require("DirLight1Color");
      _dirLight2Direction = Require("DirLight2Direction");
      _dirLight2Color = Require("DirLight2Color");
      _environmentTint = Require("EnvironmentTint");
      _environmentIntensity = Require("EnvironmentIntensity");
      _environmentTexture = Require("EnvironmentTexture");
      _hasEnvironmentTexture = Require("HasEnvironmentTexture");
      _exposure = Require("Exposure");
      _baseColorFactor = Require("BaseColorFactor");
      _metallicFactor = Require("MetallicFactor");
      _roughnessFactor = Require("RoughnessFactor");
      _metallicOverride = Require("MetallicOverride");
      _roughnessOverride = Require("RoughnessOverride");
      _specularFactor = Require("SpecularFactor");
      _glossinessFactor = Require("GlossinessFactor");
      _emissiveFactor = Require("EmissiveFactor");
      _emissiveStrength = Require("EmissiveStrength");
      _normalScale = Require("NormalScale");
      _normalMapFlipY = Require("NormalMapFlipY");
      _occlusionStrength = Require("OcclusionStrength");
      _alphaCutoff = Require("AlphaCutoff");
      _alphaMode = Require("AlphaMode");
      _workflow = Require("Workflow");
      _gameLighting = Require("GameLighting");
      _boneCount = Require("BoneCount");
      _baseColorTexture = Require("BaseColorTexture");
      _metallicRoughnessTexture = Require("MetallicRoughnessTexture");
      _normalTexture = Require("NormalTexture");
      _occlusionTexture = Require("OcclusionTexture");
      _emissiveTexture = Require("EmissiveTexture");
      _boneTexture = Require("BoneTexture");

      _staticTechnique = RequireTechnique("Static");
      _skinnedTechnique = RequireTechnique("Skinned");
      _skyTechnique = RequireTechnique("Skybox");

      _white = CreateSolid(device, Color.White);
      _black = CreateSolid(device, new Color(0, 0, 0, 255));
      _flatNormal = CreateSolid(device, new Color(128, 128, 255, 255));
      _identityBone = CreateIdentityBone(device);
    }

    private EffectParameter Require(string name)
    {
      EffectParameter parameter = Effect.Parameters[name];
      if (parameter is null)
        throw new InvalidOperationException($"PBR 着色器缺少参数 '{name}'");
      return parameter;
    }

    private EffectTechnique RequireTechnique(string name)
    {
      EffectTechnique technique = Effect.Techniques[name];
      if (technique is null)
        throw new InvalidOperationException($"PBR 着色器缺少技术 '{name}'");
      return technique;
    }

    private static Texture2D CreateSolid(GraphicsDevice device, Color color)
    {
      Texture2D texture = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
      texture.SetData(new[] { color });
      return texture;
    }

    private static Texture2D CreateIdentityBone(GraphicsDevice device)
    {
      Texture2D texture = new Texture2D(device, 1, 4, false, SurfaceFormat.Vector4);
      texture.SetData(new[]
      {
        new Vector4(1f, 0f, 0f, 0f),
        new Vector4(0f, 1f, 0f, 0f),
        new Vector4(0f, 0f, 1f, 0f),
        new Vector4(0f, 0f, 0f, 1f)
      });
      return texture;
    }

    public void SetCamera(Matrix view, Matrix projection, Vector3 eyePosition)
    {
      Matrix viewProjection = Matrix.Multiply(view, projection);
      _viewProjection.SetValue(viewProjection);
      Matrix.Invert(ref viewProjection, out Matrix inverse);
      _inverseViewProjection.SetValue(inverse);
      _eyePosition.SetValue(eyePosition);
    }

    public void SetLighting(PbrLighting lighting)
    {
      _dirLight0Direction.SetValue(lighting.KeyLight.Direction);
      _dirLight0Color.SetValue(lighting.KeyLight.Color);
      _dirLight1Direction.SetValue(lighting.FillLight.Direction);
      _dirLight1Color.SetValue(lighting.FillLight.Color);
      _dirLight2Direction.SetValue(lighting.RimLight.Direction);
      _dirLight2Color.SetValue(lighting.RimLight.Color);
      _environmentTint.SetValue(lighting.EnvironmentTint);
      _environmentIntensity.SetValue(lighting.EnvironmentIntensity);
      _environmentRotation.SetValue(lighting.EnvironmentRotation);
      _environmentTexture.SetValue(lighting.EnvironmentTexture ?? _white);
      _hasEnvironmentTexture.SetValue(lighting.EnvironmentTexture is null ? 0f : 1f);
      _exposure.SetValue(lighting.Exposure);
    }

    public void SetWorld(Matrix world)
    {
      _world.SetValue(world);
    }

    public void SelectTechnique(bool skinned)
    {
      Effect.CurrentTechnique = skinned ? _skinnedTechnique : _staticTechnique;
    }

    /// <summary>切换到全屏天空盒技术. 需先 SetCamera / SetLighting.</summary>
    public void SelectSkybox()
    {
      Effect.CurrentTechnique = _skyTechnique;
    }

    /// <summary>游戏 2D 光照模式: 跳过 3D 直接光/色调映射, 由 Ecs 的 Light 后处理统一上光.</summary>
    public void SetGameLighting(bool gameLighting)
    {
      _gameLighting.SetValue(gameLighting ? 1f : 0f);
    }

    public void BindMaterial(PbrMaterial material)
    {
      _baseColorTexture.SetValue(material.BaseColorTexture ?? _white);
      _metallicRoughnessTexture.SetValue(material.MetallicRoughnessTexture ?? _white);
      _normalTexture.SetValue(material.NormalTexture ?? _flatNormal);
      _occlusionTexture.SetValue(material.OcclusionTexture ?? _white);
      _emissiveTexture.SetValue(material.EmissiveTexture ?? _black);

      _baseColorFactor.SetValue(material.BaseColorFactor);
      _metallicFactor.SetValue(material.MetallicFactor);
      _roughnessFactor.SetValue(material.RoughnessFactor);
      _metallicOverride.SetValue(material.MetallicOverride);
      _roughnessOverride.SetValue(material.RoughnessOverride);
      _specularFactor.SetValue(material.SpecularFactor);
      _glossinessFactor.SetValue(material.GlossinessFactor);
      _emissiveFactor.SetValue(material.EmissiveFactor);
      _emissiveStrength.SetValue(material.EmissiveStrength);
      _normalScale.SetValue(material.NormalScale);
      _normalMapFlipY.SetValue(NormalMapFlipY ? -1f : 1f);
      _occlusionStrength.SetValue(material.OcclusionStrength);
      _alphaCutoff.SetValue(material.AlphaCutoff);
      _alphaMode.SetValue((float)material.AlphaMode);
      _workflow.SetValue((float)material.Workflow);
    }

    public void BindBones(PbrMesh mesh)
    {
      _boneTexture.SetValue(mesh.IsSkinned ? mesh.BonePaletteTexture : _identityBone);
      _boneCount.SetValue((float)Math.Max(1, mesh.BoneCount));
    }

    public void ApplyPass()
    {
      foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
        pass.Apply();
    }

    public void Dispose()
    {
      Effect?.Dispose();
      _white?.Dispose();
      _black?.Dispose();
      _flatNormal?.Dispose();
      _identityBone?.Dispose();
    }
  }
}
