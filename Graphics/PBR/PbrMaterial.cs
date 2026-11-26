namespace Colin.Core.Graphics.PBR
{
  public enum PbrAlphaMode
  {
    Opaque,
    Mask,
    Blend
  }

  /// <summary>
  /// 材质工作流. glTF 核心是 MetallicRoughness; KHR_materials_pbrSpecularGlossiness 扩展为 SpecularGlossiness.
  /// <br>两者都属物理渲染: 前者用 baseColor + metallic + roughness 推导 F0, 后者直接给 diffuse + specular(F0) + glossiness.</br>
  /// </summary>
  public enum PbrMaterialWorkflow
  {
    MetallicRoughness,
    SpecularGlossiness
  }

  public sealed class PbrMaterial
  {
    public string Name = string.Empty;

    public PbrMaterialWorkflow Workflow = PbrMaterialWorkflow.MetallicRoughness;

    /// <summary>MetallicRoughness: baseColorFactor; SpecularGlossiness: diffuseFactor.</summary>
    public Vector4 BaseColorFactor = Vector4.One;

    public float MetallicFactor = 1f;

    public float RoughnessFactor = 1f;

    /// <summary>SpecularGlossiness 的 specularFactor (即 F0), 线性空间.</summary>
    public Vector3 SpecularFactor = Vector3.One;

    public float GlossinessFactor = 1f;

    public Vector3 EmissiveFactor = Vector3.Zero;

    public float EmissiveStrength = 1f;

    public float NormalScale = 1f;

    public float OcclusionStrength = 1f;

    public float AlphaCutoff = 0.5f;

    public PbrAlphaMode AlphaMode = PbrAlphaMode.Opaque;

    public bool DoubleSided;

    public Texture2D BaseColorTexture;

    /// <summary>MetallicRoughness 工作流为 ORM 贴图 (G=粗糙度, B=金属度); SpecularGlossiness 工作流为 specular(RGB)+glossiness(A) 贴图.</summary>
    public Texture2D MetallicRoughnessTexture;

    public Texture2D NormalTexture;

    public Texture2D OcclusionTexture;

    public Texture2D EmissiveTexture;

    public bool IsEmissive => EmissiveStrength > 0f && (EmissiveFactor != Vector3.Zero || EmissiveTexture is not null);
  }
}
