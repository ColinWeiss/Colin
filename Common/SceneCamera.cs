namespace Colin.Core.Common
{
  /// <summary>
  /// 场景摄像机.
  /// </summary>
  public class SceneCamera : Camera, ISceneModule
  {
    public void Dispose()
    {
      Scene = null;
    }
    public void DoInitialize()
    {
      DoInitialize(CoreInfo.Graphics.GraphicsDevice.Viewport.Width, CoreInfo.Graphics.GraphicsDevice.Viewport.Height);
      Scene.Events.ClientSizeChanged += (s, e) =>
      {
        Translate = CoreInfo.ViewCenter;
        Projection = Matrix.CreateOrthographicOffCenter(0f, CoreInfo.Graphics.GraphicsDevice.Viewport.Width, CoreInfo.Graphics.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
        View = Matrix.Identity;
        ResetCamera();
      };
    }
    public void Start() { }
  }
}