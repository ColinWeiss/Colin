using FontStashSharp;
using FontStashSharp.Interfaces;
using FontStashSharp.RichText;

namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivFontStashRenderer : IFontStashRenderer
  {
    public GraphicsDevice GraphicsDevice => CoreInfo.Graphics.GraphicsDevice;

    /// <summary>
    /// 指示是否绘制 1px 偏移的暗色投影.
    /// <br>投影由同一字形双绘制实现; 关闭后文本只绘制一遍.</br>
    /// </summary>
    public bool Shadow = true;

    public static readonly DivFontStashRenderer Shadowed = new DivFontStashRenderer() { Shadow = true };
    public static readonly DivFontStashRenderer Plain = new DivFontStashRenderer() { Shadow = false };

    public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
    {
      if (texture is null)
        return;
      if (Shadow)
      {
        int r = color.R;
        r = Math.Clamp(r - 200, 0, 255);
        int g = color.G;
        g = Math.Clamp(g - 200, 0, 255);
        int b = color.B;
        b = Math.Clamp(b - 200, 0, 255);
        CoreInfo.Batch.Draw(texture, pos + Vector2.One, src, new Color(r, g, b), rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
      }
      CoreInfo.Batch.Draw(texture, pos, src, color, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
    }
  }

  public class DivFontRenderer : DivRenderer
  {
    public RichTextLayout RichTextLayout = new RichTextLayout()
    {
      IgnoreColorCommand = false
    };
    public SpriteFontBase Font;
    private string _text;
    private Vector2 _measureCache;
    private SpriteFontBase _measureFont;
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

    /// <summary>
    /// 指示是否绘制 1px 暗色投影; 默认开启, 关闭后该元素的文本只绘制一遍.
    /// </summary>
    public bool TextShadow = true;

    /// <summary>
    /// 字体来源路径 (Leemo 资产相对路径); 为空表示使用引擎默认字体.
    /// <br>界面文档依据它还原字体绑定.</br>
    /// </summary>
    public string FontPath;

    /// <summary>
    /// 字号; 与 <see cref="FontPath"/> 配对记录.
    /// </summary>
    public int FontSize = 16;

    /// <summary>
    /// 绑定字体并记录来源; 文档加载与编辑器换字体用.
    /// </summary>
    public DivFontRenderer BindFont(string path, int size)
    {
      FontPath = path;
      FontSize = size;
      Font = Assets.Font(path).GetFont(size);
      if (div is not null)
      {
        Div.Layout.SetSize(MeasureContent());
        Div.Layout.Anchor = Div.Layout.Half;
      }
      return this;
    }

    private static SpriteFontBase _defaultFont;
    private static SpriteFontBase DefaultFont => _defaultFont ??= Assets.Font("Fonts/Unifont.ttf").GetFont(16);

    public override void OnBinded()
    {
      if (Font == null)
      {
        Font = DefaultFont;
        FontPath ??= "Fonts/Unifont.ttf";
      }
      RichTextLayout.AutoEllipsisMethod = AutoEllipsisMethod.None;
      base.OnBinded();
    }

    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      //富文本排版缓存被 Font/Text 写入打穿, 逐帧无条件赋值会让每个 Label 每帧全量重排 —— 仅在真变化时写入.
      if (RichTextLayout.Font != Font)
        RichTextLayout.Font = Font;
      if (RichTextLayout.Text != Text)
        RichTextLayout.Text = Text;
      RichTextLayout.Draw(TextShadow ? DivFontStashRenderer.Shadowed : DivFontStashRenderer.Plain,
        Div.Layout.RenderTargetLocation + div.Layout.Anchor,
        div.Design.Color,
        Div.Layout.Rotation,
        div.Layout.Anchor,
        div.Layout.Scale);
      base.RenderStep(device, batch);
    }
  }
}