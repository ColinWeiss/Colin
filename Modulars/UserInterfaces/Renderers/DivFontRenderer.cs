using FontStashSharp;
using FontStashSharp.Interfaces;
using FontStashSharp.RichText;

namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivFontStashRenderer : IFontStashRenderer
  {
    public GraphicsDevice GraphicsDevice => CoreInfo.Graphics.GraphicsDevice;

    private static DivFontStashRenderer _renderer;
    public static DivFontStashRenderer Instance => _renderer ??= new DivFontStashRenderer();

    public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
    {
      if (texture is null)
        return;
      int r = color.R;
      r = Math.Clamp(r - 200, 0, 255);
      int g = color.G;
      g = Math.Clamp(g - 200, 0, 255);
      int b = color.B;
      b = Math.Clamp(b - 200, 0, 255);
      CoreInfo.Batch.Draw(texture, pos + Vector2.One, src, new Color(r, g, b), rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
      CoreInfo.Batch.Draw(texture, pos, src, color, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
    }
  }

  public class DivFontRenderer : DivRenderer
  {
    public RichTextLayout RichTextLayout = new RichTextLayout()
    {
      IgnoreColorCommand = false
    };
    public DynamicSpriteFont Font;
    private string _text;
    private Vector2 _measureCache;
    private DynamicSpriteFont _measureFont;
    private bool _measureDirty = true;

    public string Text
    {
      get => _text;
      set
      {
        if (_text == value)
          return;
        _text = value;
        _measureDirty = true;
        if (div is null || Font is null)
          return;
        Div.Layout.SetSize(MeasureContent());
        Div.Layout.Anchor = Div.Layout.Half;
      }
    }

    /// <summary>
    /// 测量文本所期望的尺寸; 结果随文本与字体缓存, 避免逐帧重复测量.
    /// </summary>
    public override Vector2 MeasureContent()
    {
      if (Font is null)
        return new Vector2(-1f);
      if (_measureDirty || _measureFont != Font)
      {
        _measureCache = Font.MeasureString(_text ?? string.Empty);
        _measureFont = Font;
        _measureDirty = false;
      }
      return _measureCache;
    }

    public TextStyle TextStyle;

    public FontSystemEffect FontSystemEffect;
    public override void OnDivInitialize()
    {
      base.OnDivInitialize();
    }

    private static DynamicSpriteFont font = Asset.GetFont("Unifont").GetFont(16);
    public override void OnBinded()
    {
      if (Font == null)
        Font = font;//Asset.GetFont("Unifont").GetFont(16);
      base.OnBinded();
    }
    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      RichTextLayout.AutoEllipsisMethod = AutoEllipsisMethod.None;
      RichTextLayout.Font = Font;
      RichTextLayout.Text = Text;
      RichTextLayout.Draw(DivFontStashRenderer.Instance,
        Div.Layout.RenderTargetLocation + div.Layout.Anchor,
        div.Design.Color,
        Div.Layout.Rotation,
        div.Layout.Anchor,
        div.Layout.Scale);
      base.RenderStep(device, batch);
    }
  }
}