namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 实体 3D 渲染组件: 把一个 PbrModel 挂在实体 Transform 上.
  /// <br>模型底部中心对齐实体位置; Depth 是 3D 的 Z: &gt;0 画在 2D 之上, &lt;0 被 2D 遮挡.</br>
  /// </summary>
  public sealed class EcsComPbrRender : IEcsCom
  {
    public PbrModel Model;

    public Vector2 Offset;

    public float Depth;

    public float Scale = 1f;

    public float Yaw;

    public bool Visible = true;

    public void DoInitialize() { }
  }
}
