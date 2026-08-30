namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素的布局信息.
  /// <br>布局数据采用脏标记与版本戳管理: 仅当自身输入或父级变换发生变化时才会重新计算.</br>
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
      set
      {
        if (left == value)
          return;
        left = value;
        dirty = true;
      }
    }

    /// <summary>
    /// 获取划分元素的右侧坐标.
    /// </summary>
    public float Right => Left + Width;

    /// <summary>
    /// 获取划分元素的底部坐标.
    /// </summary>
    public float Bottom => Top + Height;

    private float top;
    /// <summary>
    /// 指示划分元素相对于父元素的顶部坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public float Top
    {
      get => top;
      set
      {
        if (top == value)
          return;
        top = value;
        dirty = true;
      }
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

    private float paddingRight;
    /// <summary>
    /// 指示划分元素的右侧填充值.
    /// </summary>
    public float PaddingRight
    {
      get => paddingRight;
      set => paddingRight = value;
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

    /// <summary>
    /// 指示划分元素的底部填充值.
    /// </summary>
    private float paddingBottom;
    public float PaddingBottom
    {
      get => paddingBottom;
      set => paddingBottom = value;
    }

    /// <summary>
    /// 为划分元素设置四边等值的填充.
    /// </summary>
    /// <param name="padding">四边填充值.</param>
    public void SetPadding(float padding)
      => paddingLeft = paddingRight = paddingTop = paddingBottom = padding;

    /// <summary>
    /// 为划分元素设置填充.
    /// </summary>
    /// <param name="horizontal">左右填充值.</param>
    /// <param name="vertical">上下填充值.</param>
    public void SetPadding(float horizontal, float vertical)
    {
      paddingLeft = paddingRight = horizontal;
      paddingTop = paddingBottom = vertical;
    }

    /// <summary>
    /// 获取划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public Vector2 Location
    {
      get => new Vector2(left, top);
      set => SetLocation(value);
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    public void SetLocation(float leftAndTop)
    {
      if (left == leftAndTop && top == leftAndTop)
        return;
      left = top = leftAndTop;
      dirty = true;
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    /// <param name="left">左侧坐标.</param>
    /// <param name="top">顶部坐标.</param>
    public void SetLocation(float left, float top)
    {
      if (this.left == left && this.top == top)
        return;
      this.left = left;
      this.top = top;
      dirty = true;
    }
    /// <summary>
    /// 设置划分元素相对于父元素的坐标.
    /// <br>若划分元素没有父元素 (即其属于DivView), 则指示其为相对于屏幕起点的偏移.</br>
    /// </summary>
    /// <param name="location">相对坐标.</param>
    public void SetLocation(Vector2 location) => SetLocation(location.X, location.Y);

    public void SetLocation(Point point) => SetLocation(point.X, point.Y);

    /// <summary>
    /// 将划分元素居中于指定布局范围.
    /// </summary>
    /// <param name="container">作为居中参照的布局信息.</param>
    public void CenterIn(DivLayout container) => SetLocation(container.Half - Half);

    /// <summary>
    /// 将划分元素于指定布局范围内水平居中.
    /// </summary>
    /// <param name="container">作为居中参照的布局信息.</param>
    public void CenterInX(DivLayout container) => Left = container.HalfWidth - HalfWidth;

    /// <summary>
    /// 将划分元素于指定布局范围内垂直居中.
    /// </summary>
    /// <param name="container">作为居中参照的布局信息.</param>
    public void CenterInY(DivLayout container) => Top = container.HalfHeight - HalfHeight;

    private float width;
    /// <summary>
    /// 指示划分元素的宽度.
    /// </summary>
    public float Width
    {
      get => width;
      set
      {
        value = Math.Clamp(value, 0, int.MaxValue);
        if (width == value)
          return;
        width = value;
        dirty = true;
      }
    }

    private float height;
    /// <summary>
    /// 指示划分元素的高度.
    /// </summary>
    public float Height
    {
      get => height;
      set
      {
        value = Math.Clamp(value, 0, int.MaxValue);
        if (height == value)
          return;
        height = value;
        dirty = true;
      }
    }

    public Vector2 Half => new Vector2(width / 2, height / 2);

    public float HalfWidth => width / 2;

    public float HalfHeight => height / 2;

    /// <summary>
    /// 获取划分元素的大小.
    /// </summary>
    public Vector2 Size
    {
      get => new Vector2(width, height);
      set => SetSize(value);
    }

    public Point SizeP => new Point((int)width, (int)height);

    /// <summary>
    /// 设置划分元素的大小.
    /// </summary>
    /// <param name="width">宽度.</param>
    /// <param name="height">高度.</param>
    public void SetSize(float width, float height)
    {
      Width = width;
      Height = height;
    }
    /// <summary>
    /// 设置划分元素的大小.
    /// </summary>
    /// <param name="size">宽高.</param>
    public void SetSize(float size)
    {
      Width = size;
      Height = size;
    }
    /// <summary>
    /// 设置划分元素的大小.
    /// </summary>
    /// <param name="size">大小.</param>
    public void SetSize(Vector2 size) => SetSize(size.X, size.Y);

    private float rotation;
    /// <summary>
    /// 指示划分元素的旋转.
    /// </summary>
    public float Rotation
    {
      get => rotation;
      set
      {
        if (rotation == value)
          return;
        rotation = value;
        dirty = true;
      }
    }
    /// <summary>
    /// 顺时针旋转指定的弧度.
    /// </summary>
    /// <param name="radian">弧度</param>
    public void ClockwiseRad(float radian)
    {
      rotation += radian;
      dirty = true;
    }
    /// <summary>
    /// 逆时针旋转指定的弧度.
    /// </summary>
    /// <param name="radian">弧度</param>
    public void AntiClockwiseRad(float radian)
    {
      rotation -= radian;
      dirty = true;
    }

    private float anchorX;
    public float AnchorX
    {
      get => anchorX;
      set
      {
        if (anchorX == value)
          return;
        anchorX = value;
        dirty = true;
      }
    }
    private float anchorY;
    public float AnchorY
    {
      get => anchorY;
      set
      {
        if (anchorY == value)
          return;
        anchorY = value;
        dirty = true;
      }
    }

    public Vector2 Anchor
    {
      get => new Vector2(anchorX, anchorY);
      set
      {
        if (anchorX == value.X && anchorY == value.Y)
          return;
        anchorX = value.X;
        anchorY = value.Y;
        dirty = true;
      }
    }

    private float scaleX;
    /// <summary>
    /// 指示划分元素的横向缩放.
    /// </summary>
    public float ScaleX
    {
      get => scaleX;
      set
      {
        if (scaleX == value)
          return;
        scaleX = value;
        dirty = true;
      }
    }

    private float scaleY;
    /// <summary>
    /// 指示划分元素的纵向缩放.
    /// </summary>
    public float ScaleY
    {
      get => scaleY;
      set
      {
        if (scaleY == value)
          return;
        scaleY = value;
        dirty = true;
      }
    }

    /// <summary>
    /// 指示划分元素的缩放.
    /// </summary>
    public Vector2 Scale
    {
      get => new Vector2(scaleX, scaleY);
      set
      {
        if (scaleX == value.X && scaleY == value.Y)
          return;
        scaleX = value.X;
        scaleY = value.Y;
        dirty = true;
      }
    }
    /// <summary>
    /// 设置划分元素的缩放.
    /// </summary>
    /// <param name="scaleX">横向缩放值.</param>
    /// <param name="scaleY">纵向缩放值.</param>
    public void SetScale(int scaleX, int scaleY)
    {
      ScaleX = scaleX;
      ScaleY = scaleY;
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

    private bool scissorEnable;
    public bool ScissorEnable
    {
      get => scissorEnable;
      set
      {
        if (scissorEnable == value)
          return;
        scissorEnable = value;
        dirty = true;
      }
    }

    private int scissorLeft;
    public int ScissorLeft
    {
      get => scissorLeft;
      set
      {
        if (scissorLeft == value)
          return;
        scissorLeft = value;
        dirty = true;
      }
    }

    private int scissorTop;
    public int ScissorTop
    {
      get => scissorTop;
      set
      {
        if (scissorTop == value)
          return;
        scissorTop = value;
        dirty = true;
      }
    }

    private int scissorWidth;
    public int ScissorWidth
    {
      get => scissorWidth;
      set
      {
        if (scissorWidth == value)
          return;
        scissorWidth = value;
        dirty = true;
      }
    }

    private int scissorHeight;
    public int ScissorHeight
    {
      get => scissorHeight;
      set
      {
        if (scissorHeight == value)
          return;
        scissorHeight = value;
        dirty = true;
      }
    }

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

    /// <summary>
    /// 指示布局输入是否发生变更, 需要于下一次 <see cref="Calculate(Div)"/> 时重新计算.
    /// </summary>
    public bool IsDirty => dirty;

    /// <summary>
    /// 标记布局为脏, 使其于下一次 <see cref="Calculate(Div)"/> 时强制重新计算.
    /// </summary>
    public void Invalidate() => dirty = true;

    /// <summary>
    /// 布局版本号; 每当有划分元素的布局计算结果发生实际变化时递增.
    /// </summary>
    private static uint layoutVersion = 1;

    /// <summary>
    /// 获取划分元素布局变换的当前版本.
    /// </summary>
    internal uint TransformStamp => transformStamp;

    /// <summary>
    /// 获取上次布局计算所依据的父级变换版本.
    /// </summary>
    internal uint ParentStamp => parentStamp;

    private bool dirty;
    private bool calculated;
    private uint transformStamp;
    private uint parentStamp;

    /// <summary>
    /// 更新计算指定划分元素的布局信息.
    /// <br>若划分元素的布局输入与父级变换均未发生变化, 则跳过计算.</br>
    /// </summary>
    /// <param name="div">要进行计算布局信息的划分元素.</param>
    public static void Calculate(Div div)
    {
      Div parent = div.Parent;
      uint currentParentStamp = parent is null ? 0u : parent.Layout.transformStamp;
      if (div.Layout.calculated && div.Layout.dirty is false && div.Layout.parentStamp == currentParentStamp)
        return;

      Matrix oldRenderTargetTransform = div.Layout.renderTargetTransform;
      Matrix oldScreenTransform = div.Layout.screenTransform;
      Rectangle oldRenderTargetBounds = div.Layout.renderTargetBounds;
      Rectangle oldBounds = div.Layout.bounds;
      Rectangle oldScissorRectangle = div.Layout.scissorRectangle;

      Matrix local = CalculateTransform(div);

      div.Layout.renderTargetTransform = local;
      if (parent is not null)
        div.Layout.renderTargetTransform *= parent.Layout.renderTargetTransform;

      if (parent is not null && parent.IsCanvas)
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

      div.Layout.screenTransform = local;
      if (parent is not null)
        div.Layout.screenTransform *= parent.Layout.screenTransform;

      div.Layout.screenLeft = (int)div.Layout.screenTransform.Translation.X;
      div.Layout.screenTop = (int)div.Layout.screenTransform.Translation.Y;

      //以变换后四角的包围盒作为屏幕包围盒, 使缩放与旋转参与命中判定.
      Vector2 corner0 = Vector2.Transform(Vector2.Zero, div.Layout.screenTransform);
      Vector2 corner1 = Vector2.Transform(new Vector2(div.Layout.width, 0), div.Layout.screenTransform);
      Vector2 corner2 = Vector2.Transform(new Vector2(0, div.Layout.height), div.Layout.screenTransform);
      Vector2 corner3 = Vector2.Transform(new Vector2(div.Layout.width, div.Layout.height), div.Layout.screenTransform);
      Vector2 min = Vector2.Min(Vector2.Min(corner0, corner1), Vector2.Min(corner2, corner3));
      Vector2 max = Vector2.Max(Vector2.Max(corner0, corner1), Vector2.Max(corner2, corner3));

      div.Layout.bounds.X = (int)min.X;
      div.Layout.bounds.Y = (int)min.Y;
      div.Layout.bounds.Width = (int)(max.X - min.X);
      div.Layout.bounds.Height = (int)(max.Y - min.Y);

      bool changed = div.Layout.calculated is false
          || oldRenderTargetTransform != div.Layout.renderTargetTransform
          || oldScreenTransform != div.Layout.screenTransform
          || oldRenderTargetBounds != div.Layout.renderTargetBounds
          || oldBounds != div.Layout.bounds
          || oldScissorRectangle != div.Layout.scissorRectangle;

      div.Layout.calculated = true;
      div.Layout.dirty = false;
      div.Layout.parentStamp = currentParentStamp;

      if (changed)
      {
        div.Layout.transformStamp = ++layoutVersion;
        //计算结果发生实际变化, 下级元素的布局随之失效.
        List<Div> children = div.Children;
        for (int count = 0; count < children.Count; count++)
          children[count].Layout.dirty = true;
      }
    }

    private static Matrix CalculateTransform(Div div)
    {
      Matrix result;
      if (div.IsCanvas is false)
      {
        result =
          Matrix.CreateScale(div.Layout.ScaleX, div.Layout.ScaleY, 0) *
          Matrix.CreateTranslation(-div.Layout.AnchorX, -div.Layout.AnchorY, 0) *
          Matrix.CreateRotationZ(div.Layout.rotation) *
          Matrix.CreateTranslation(div.Layout.AnchorX, div.Layout.AnchorY, 0) *
          Matrix.CreateTranslation(div.Layout.left, div.Layout.top, 0);
      }
      else
      {
        result =
          Matrix.CreateTranslation(-div.Layout.AnchorX, -div.Layout.AnchorY, 0) *
          Matrix.CreateRotationZ(div.Layout.rotation) *
          Matrix.CreateTranslation(div.Layout.AnchorX, div.Layout.AnchorY, 0) *
          Matrix.CreateTranslation(div.Layout.left, div.Layout.top, 0);
      }
      return result;
    }
  }
}
