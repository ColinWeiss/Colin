namespace Colin.Core.Graphics.PBR
{
  /// <summary>可被 3D 层锚定的世界对象 (玩家/实体/物块均可实现).</summary>
  public interface IPbrWorldAnchor
  {
    Vector2 WorldPosition { get; }
  }

  /// <summary>
  /// 挂在世界锚点上的 3D 模型. Offset 为 2D 世界像素偏移, Depth 为 3D 深度 (正数更靠近相机).
  /// </summary>
  public sealed class PbrAttachment
  {
    public PbrModel Model;

    /// <summary>为空时跟随宿主模块的锚点 (通常是玩家).</summary>
    public IPbrWorldAnchor Anchor;

    public Vector2 Offset;

    public float Depth;

    public float Scale;

    public float Yaw;

    /// <summary>把模型底部中心移到原点的预变换.</summary>
    public Matrix Pivot;
  }
}
