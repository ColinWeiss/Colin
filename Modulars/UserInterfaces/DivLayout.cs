using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素的布局信息.
  /// </summary>
  public struct DivLayout
  {
    private float left;
    /// <summary>
    /// 指示划分元素相对于父元素的左侧坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public float Left
    {
      get => left;
      set => location.X = left = value;
    }

    private float top;
    /// <summary>
    /// 指示划分元素相对于父元素的顶部坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public float Top
    {
      get => top;
      set => location.Y = top = value;
    }

    private float paddingLeft;
    /// <summary>
    /// 指示划分元素的左侧填充值.
    /// </summary>
    public float PaddingLeft
    {
      get => paddingLeft;
      set => paddingLeft = value;
    }

    private float paddingTop;
    /// <summary>
    /// 指示划分元素的顶部填充值.
    /// </summary>
    public float PaddingTop
    {
      get => paddingTop;
      set => paddingTop = value;
    }

    private Vector2 location;
    /// <summary>
    /// 指示划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public Vector2 Location
    {
      get => location;
      set => SetLocation(value);
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public void SetLocation(float leftAndTop)
    {
      location.X = left = leftAndTop;
      location.Y = top = leftAndTop;
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    /// <param name="left">左侧坐标.</param>
    /// <param name="top">顶部坐标.</param>
    public void SetLocation(float left, float top)
    {
      location.X = this.left = left;
      location.Y = this.top = top;
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    /// <param name="location">相对坐标.</param>
    public void SetLocation(Vector2 location) => SetLocation(location.X, location.Y);

    private float width;
    /// <summary>
    /// 指示划分元素的宽度.
    /// </summary>
    public float Width
    {
      get => width;
      set => size.X = width = Math.Clamp(value, 0, int.MaxValue);
    }

    private float height;
    /// <summary>
    /// 指示划分元素的高度.
    /// </summary>
    public float Height
    {
      get => height;
      set => size.Y = height = Math.Clamp(value, 0, int.MaxValue);
    }

    public Vector2 Half => new Vector2(width / 2, height / 2);

    public float HalfWidth => width / 2;

    public float HalfHeight => height / 2;

    private Vector2 size;
    /// <summary>
    /// 获取划分元素的大小.
    /// </summary>
    public Vector2 Size
    {
      get => size;
      set => SetSize(value);
    }

    public Point SizeP => size.ToPoint();

    /// <summary>
    /// 设置划分元素的大小.
    /// <br>[!] 使用损失精度的参数.</br>
    /// </summary>
    /// <param name="width">宽度.</param>
    /// <param name="height">高度.</param>
    public void SetSize(float width, float height)
    {
      size.X = this.width = (int)width;
      size.Y = this.height = (int)height;
    }
    /// <summary>
    /// 设置划分元素的大小.
    /// <br>[!] 使用损失精度的参数.</br>
    /// </summary>
    /// <param name="size">宽高.</param>
    public void SetSize(float size)
    {
      this.size.X = width = (int)size;
      this.size.Y = height = (int)size;
    }
    /// <summary>
    /// 设置划分元素的大小.
    /// <br>[!] 使用损失精度的参数.</br>
    /// </summary>
    /// <param name="size">大小.</param>
    public void SetSize(Vector2 size) => SetSize((int)size.X, (int)size.Y);

    private float rotation;
    /// <summary>
    /// 指示划分元素的旋转.
    /// </summary>
    public float Rotation
    {
      get => rotation;
      set => rotation = value;
    }
    /// <summary>
    /// 顺时针旋转指定的弧度.
    /// </summary>
    /// <param name="radian">弧度</param>
    public void ClockwiseRad(float radian) => rotation += radian;
    /// <summary>
    /// 逆时针旋转指定的弧度.
    /// </summary>
    /// <param name="radian">弧度</param>
    public void AntiClockwiseRad(float radian) => rotation -= radian;

    private float anchorX;
    public float AnchorX
    {
      get => anchorX;
      set => anchor.X = anchorX = value;
    }
    private float anchorY;
    public float AnchorY
    {
      get => anchorY;
      set => anchor.Y = anchorY = value;
    }

    private Vector2 anchor;
    public Vector2 Anchor
    {
      get => anchor;
      set
      {
        if (anchor != value)
        {
          anchor.X = anchorX = value.X;
          anchor.Y = anchorY = value.Y;
        }
      }
    }

    private float scaleX;
    /// <summary>
    /// 指示划分元素的横向缩放.
    /// </summary>
    public float ScaleX
    {
      get => scaleX;
      set => scale.X = scaleX = value;
    }

    private float scaleY;
    /// <summary>
    /// 指示划分元素的纵向缩放.
    /// </summary>
    public float ScaleY
    {
      get => scaleY;
      set => scale.Y = scaleY = value;
    }

    private Vector2 scale;
    /// <summary>
    /// 指示划分元素的缩放.
    /// </summary>
    public Vector2 Scale
    {
      get => scale;
      set
      {
        if (scale != value)
        {
          scale.X = scaleX = value.X;
          scale.Y = scaleY = value.Y;
        }
      }
    }
    /// <summary>
    /// 设置划分元素的缩放.
    /// </summary>
    /// <param name="scaleX">横向缩放值.</param>
    /// <param name="scaleY">纵向缩放值.</param>
    public void SetScale(int scaleX, int scaleY)
    {
      scale.X = this.scaleX = scaleX;
      scale.Y = this.scaleY = scaleY;
    }
    /// <summary>
    /// 设置划分元素的缩放.
    /// </summary>
    /// <param name="scale">缩放值.</param>
    public void SetScale(Point scale) => SetScale(scale.X, scale.Y);

    public Matrix renderTargetTransform;
    public Matrix RenderTargetTransform => renderTargetTransform;

    private int renderTargetLeft;
    /// <summary>
    /// 获取划分元素于当前渲染目标上的左侧坐标.
    /// </summary>
    public int RenderTargetLeft => renderTargetLeft;

    private int renderTargetTop;
    /// <summary>
    /// 获取划分元素于当前渲染目标上的顶部坐标.
    /// </summary>
    public int RenderTargetTop => renderTargetTop;

    /// <summary>
    /// 获取划分元素于当前渲染目标上的坐标.
    /// </summary>
    public Vector2 RenderTargetLocation => new Vector2(renderTargetLeft, renderTargetTop);

    private Rectangle renderTargetBounds;
    /// <summary>
    /// 获取划分元素于当前渲染目标上的包围盒.
    /// </summary>
    public Rectangle RenderTargetBounds => renderTargetBounds;

    private int screenLeft;
    /// <summary>
    /// 获取划分元素相对于屏幕起点的左侧坐标.
    /// </summary>
    public int ScreenLeft => screenLeft;

    private int screenTop;
    /// <summary>
    /// 获取划分元素相对于屏幕起点的顶部坐标.
    /// </summary>
    public int ScreenTop => screenTop;

    /// <summary>
    /// 获取划分元素相对于屏幕起点的坐标.
    /// </summary>
    public Vector2 ScreenLocation => new Vector2(screenLeft, screenTop);

    public Matrix screenTransform;
    public Matrix ScreenTransform => screenTransform;

    private Rectangle bounds;
    /// <summary>
    /// 获取划分元素于屏幕上的包围盒.
    /// </summary>
    public Rectangle Bounds => bounds;

    public bool ScissorEnable;

    public int ScissorLeft;
    public int ScissorTop;
    public int ScissorWidth;
    public int ScissorHeight;

    public static Stack<Rectangle> scissors = new Stack<Rectangle>();

    /// <summary>
    /// 用于暂存划分元素本次剪裁矩形的字段.
    /// </summary>
    public Rectangle ScissorRectangleCache;

    private Rectangle scissorRectangle;
    /// <summary>
    /// 指示该划分元素的剪裁矩形.
    /// <br>剪裁矩形的坐标相对于划分元素进行计算.</br>
    /// </summary>
    public Rectangle ScissorRectangle => scissorRectangle;

    private Rectangle interactiveRectangle;

    /// <summary>
    /// 更新计算指定划分元素的布局信息.
    /// </summary>
    /// <param name="div">要进行计算布局信息的划分元素.</param>
    public static void Calculate(Div div)
    {
      div.Layout.renderTargetTransform =
          Matrix.CreateTranslation(div.Layout.anchor.X, div.Layout.anchor.Y, 0) *
          Matrix.CreateScale(1, 1, 0) *
          Matrix.CreateRotationZ(div.Layout.rotation) *
          Matrix.CreateTranslation(div.Layout.left, div.Layout.top, 0);
      if (div.Parent is not null)
        div.Layout.renderTargetTransform *= div.Parent.Layout.renderTargetTransform;

      if (div.Parent is not null && div.Parent.IsCanvas)
        div.Layout.renderTargetTransform.Translation = new Vector3(div.Layout.left, div.Layout.top, 0);

      div.Layout.renderTargetBounds.X = div.Layout.renderTargetLeft = (int)div.Layout.renderTargetTransform.Translation.X;
      div.Layout.renderTargetBounds.Y = div.Layout.renderTargetTop = (int)div.Layout.renderTargetTransform.Translation.Y;
      div.Layout.renderTargetBounds.Width = (int)div.Layout.width;
      div.Layout.renderTargetBounds.Height = (int)div.Layout.height;

      if (div.Layout.ScissorEnable)
      {
        div.Layout.scissorRectangle.X = div.Layout.renderTargetBounds.X + div.Layout.ScissorLeft;
        div.Layout.scissorRectangle.Y = div.Layout.renderTargetBounds.Y + div.Layout.ScissorTop;
        div.Layout.scissorRectangle.Width = div.Layout.ScissorWidth;
        div.Layout.scissorRectangle.Height = div.Layout.ScissorHeight;
      }

      div.Layout.screenTransform =
          Matrix.CreateScale(1, 1, 0) *
          Matrix.CreateRotationZ(div.Layout.rotation) *
          Matrix.CreateTranslation(div.Layout.left, div.Layout.top, 0);
      if (div.Parent is not null)
        div.Layout.screenTransform *= div.Parent.Layout.screenTransform;
      div.Layout.bounds.X = div.Layout.screenLeft = (int)div.Layout.screenTransform.Translation.X;
      div.Layout.bounds.Y = div.Layout.screenTop = (int)div.Layout.screenTransform.Translation.Y;
      div.Layout.bounds.Width = (int)div.Layout.width;
      div.Layout.bounds.Height = (int)div.Layout.height;
    }
  }
}