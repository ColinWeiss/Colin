namespace Colin.Core.Modulars.Skys
{
  public class Sky : ISceneModule, IRenderableISceneModule
  {
    public Scene Scene { get; set; }

    public bool Enable { get; set; }

    public RenderTarget2D RawRt { get; set; }

    public bool RawRtVisible { get; set; }
    public bool Presentation { get; set; }

    public SpriteSortMode SpriteSortMode { get; }

    public Matrix? TransformMatrix { get; }

    public SkyStyle CurrentSkyStyle { get; private set; }

    public SkyStyle NextStyle { get; private set; }

    public void DoInitialize()
    {

    }
    public void Start()
    {

    }
    public void DoUpdate(GameTime time)
    {
      CurrentSkyStyle?.DoUpdate(time);
    }

    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      CurrentSkyStyle?.DoRender();
    }
    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) { }

    public void ChangeSkyStyle(SkyStyle skyStyle)
    {
      NextStyle = skyStyle;
    }

    public void Dispose()
    {
      CurrentSkyStyle = null;
      NextStyle = null;
    }
  }
}