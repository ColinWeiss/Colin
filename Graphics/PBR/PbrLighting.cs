namespace Colin.Core.Graphics.PBR
{
  public struct PbrDirectionalLight
  {
    public Vector3 Direction;
    public Vector3 Color;

    public bool Enabled => Color != Vector3.Zero;
  }

  /// <summary>
  /// 前向光照配置: 三盏方向光 (与 XNA BasicEffect 的三光位一致) + 程序化环境项.
  /// <br>环境项当前是天空渐变近似; 后续 IBL 会替换成预过滤环境图 + BRDF LUT, 接口保持不变.</br>
  /// </summary>
  public sealed class PbrLighting
  {
    public PbrDirectionalLight KeyLight;

    public PbrDirectionalLight FillLight;

    public PbrDirectionalLight RimLight;

    public Vector3 EnvironmentTint = Vector3.One;

    public float EnvironmentIntensity = 1f;

    public float Exposure = 1f;

    /// <summary>环境贴图绕 Y 轴的旋转 (弧度); 让天空盒相对物体旋转.</summary>
    public float EnvironmentRotation;

    /// <summary>
    /// 等距柱状 (equirectangular) 环境贴图; 为 null 时使用程序化天空.
    /// <br>旧 Process3D 示例的 Sky 贴图就是这种 2:1 左右的天空全景.</br>
    /// </summary>
    public Texture2D EnvironmentTexture;

    public static PbrLighting CreateDefault()
    {
      PbrLighting lighting = new PbrLighting();
      lighting.KeyLight = new PbrDirectionalLight
      {
        Direction = Vector3.Normalize(new Vector3(-0.54f, -0.62f, -0.58f)),
        Color = new Vector3(3.4f, 3.2f, 2.9f)
      };
      lighting.FillLight = new PbrDirectionalLight
      {
        Direction = Vector3.Normalize(new Vector3(0.72f, 0.34f, 0.60f)),
        Color = new Vector3(0.9f, 0.82f, 0.7f)
      };
      lighting.RimLight = new PbrDirectionalLight
      {
        Direction = Vector3.Normalize(new Vector3(0.45f, -0.77f, 0.45f)),
        Color = new Vector3(0.5f, 0.58f, 0.68f)
      };
      lighting.EnvironmentTint = new Vector3(0.75f, 0.82f, 0.95f);
      lighting.EnvironmentIntensity = 0.85f;
      lighting.Exposure = 1f;
      return lighting;
    }
  }
}
