using System.Windows.Forms;

namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivTextureRenderer : DivRenderer
  {
    private Sprite _sprite;
    public Sprite Sprite => _sprite;
    
    public override void OnDivInitialize() { }

    public bool Addtive = false;

    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if(Addtive)
      {
        batch.End();
        batch.Begin(SpriteSortMode.Deferred,BlendState.Additive,null , null ,div.Layout.ScissorEnable ? div.ScissiorRasterizer : null);
      }
      if (_sprite is not null)
      {
        Frame currentFrame = _sprite.Frame;
        batch.Draw(
          _sprite.Source,
          Div.Layout.RenderTargetLocation,
          currentFrame.GetFrame(),
          Div.Design.Color,
          Div.Layout.Rotation,
          div.Layout.Anchor,
          Div.Layout.Scale,
          SpriteEffects.None,
          _sprite.Depth);
      }
      if (Addtive)
      {
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,SamplerState.PointClamp);
      }
    }
    public DivTextureRenderer Bind(Sprite sprite)
    {
      _sprite = sprite;
      return this;
    }
    public DivTextureRenderer Bind(Texture2D texture)
    {
      _sprite = new Sprite(texture);
      return this;
    }
    public DivTextureRenderer Bind(string path)
    {
      _sprite = Sprite.Get(path);
      return this;
    }
    public DivTextureRenderer Bind(params string[] path)
    {
      _sprite = Sprite.Get(path);
      return this;
    }
  }
}