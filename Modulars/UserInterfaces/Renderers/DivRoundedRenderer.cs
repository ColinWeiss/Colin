namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  /// <summary>
  /// 圆角矩形渲染器; 以程序化抗锯齿圆角绘制划分元素的矩形区域.
  /// <br>填充颜色取自 <see cref="DivDesign.Color"/>; 不支持旋转.</br>
  /// </summary>
  public class DivRoundedRenderer : DivRenderer
  {
    private Sprite _pixel;

    /// <summary>
    /// 圆角半径.
    /// </summary>
    public float Radius;

    /// <summary>
    /// 描边宽度; 零表示不绘制描边.
    /// </summary>
    public float BorderWidth;

    /// <summary>
    /// 描边颜色.
    /// </summary>
    public Color BorderColor = Color.Black;

    public override void OnDivInitialize()
    {
      _pixel = Sprite.Get("Pixel");
    }

    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      Rectangle rectangle = new Rectangle(
        (int)Div.Layout.RenderTargetLocation.X,
        (int)Div.Layout.RenderTargetLocation.Y,
        (int)Div.Layout.Width,
        (int)Div.Layout.Height);
      if (BorderWidth > 0f)
        RoundedRect.Draw(batch, rectangle, Div.Design.Color, Radius, BorderWidth, BorderColor, _pixel.Depth);
      else
        RoundedRect.Draw(batch, rectangle, Div.Design.Color, Radius, _pixel.Depth);
    }

    public DivRoundedRenderer SetRadius(float radius)
    {
      Radius = radius;
      return this;
    }

    public DivRoundedRenderer SetBorder(float width, Color color)
    {
      BorderWidth = width;
      BorderColor = color;
      return this;
    }

    public DivRoundedRenderer SetDesignColor(Color color)
    {
      Div.Design.Color = color;
      return this;
    }

    public DivRoundedRenderer SetDesignColor(Color color, int a)
    {
      Div.Design.Color = new Color(color, a);
      return this;
    }
  }
}
