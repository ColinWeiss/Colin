namespace Colin.Core.Graphics
{
  /// <summary>
  /// 圆角矩形程序化绘制.
  /// <br>按目标半径逐一生成四分之一圆角纹理并缓存, 覆盖率于目标分辨率下按像素中心到圆弧的距离解析求得,</br>
  /// <br>纹理与绘制 1:1 对应, 抗锯齿不受点采样与缩放影响.</br>
  /// <br>圆角矩形由四个圆角、四条边与中心区域共九次批次绘制组成; 全程经由 <c>SpriteBatch</c> 提交, 与界面批次状态机 (见
  /// <see cref="Colin.Core.Modulars.UserInterfaces.UIBatch"/>) 完全兼容, 不会打断批次.</br>
  /// </summary>
  public static class RoundedRect
  {
    private static readonly Dictionary<int, Texture2D> _corners = new Dictionary<int, Texture2D>();

    private static Sprite _pixel;

    private static Sprite Pixel => _pixel ??= Sprite.Get("Pixel");

    /// <summary>
    /// 获取指定半径的四分之一圆角纹理; 不透明区域朝向纹理右下角, 边缘为解析式抗锯齿.
    /// </summary>
    public static Texture2D GetCorner(int radius)
    {
      radius = Math.Max(1, radius);
      if (_corners.TryGetValue(radius, out Texture2D texture))
        return texture;
      texture = CreateCorner(radius);
      _corners.Add(radius, texture);
      return texture;
    }

    private static Texture2D CreateCorner(int size)
    {
      Color[] data = new Color[size * size];
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          //以纹理右下角为圆心建立距离场, 覆盖率按像素中心到圆弧的距离解析求得;
          //纹理尺寸与绘制尺寸一致, 逐像素 1:1 采样, 点采样下也不会丢失抗锯齿.
          float dx = x + 0.5f - size;
          float dy = y + 0.5f - size;
          float distance = MathF.Sqrt(dx * dx + dy * dy);
          float coverage = Math.Clamp(size + 0.5f - distance, 0f, 1f);
          data[y * size + x] = new Color(255, 255, 255, (int)(coverage * 255f));
        }
      }
      Texture2D texture = new Texture2D(CoreInfo.Graphics.GraphicsDevice, size, size);
      texture.SetData(data);
      return texture;
    }

    /// <summary>
    /// 绘制填充圆角矩形.
    /// </summary>
    /// <param name="batch">批次.</param>
    /// <param name="rectangle">目标矩形.</param>
    /// <param name="color">填充颜色.</param>
    /// <param name="radius">圆角半径; 上限为矩形短边之半.</param>
    /// <param name="depth">层级深度.</param>
    public static void Draw(SpriteBatch batch, Rectangle rectangle, Color color, float radius, float depth = 0f)
    {
      if (rectangle.Width <= 0 || rectangle.Height <= 0)
        return;
      float clampedRadius = Math.Clamp(radius, 0f, Math.Min(rectangle.Width, rectangle.Height) / 2f);
      Texture2D pixel = Pixel.Source;
      if (clampedRadius <= 0.5f)
      {
        batch.Draw(pixel, rectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
        return;
      }
      int left = rectangle.X;
      int top = rectangle.Y;
      int right = rectangle.Right;
      int bottom = rectangle.Bottom;
      int ir = Math.Clamp((int)MathF.Round(clampedRadius), 1, Math.Min(rectangle.Width / 2, rectangle.Height / 2));
      Texture2D corner = GetCorner(ir);
      int centerX = left + ir;
      int centerY = top + ir;
      int innerWidth = rectangle.Width - ir * 2;
      int innerHeight = rectangle.Height - ir * 2;
      //四个圆角.
      batch.Draw(corner, new Rectangle(left, top, ir, ir), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(corner, new Rectangle(right - ir, top, ir, ir), null, color, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, depth);
      batch.Draw(corner, new Rectangle(left, bottom - ir, ir, ir), null, color, 0f, Vector2.Zero, SpriteEffects.FlipVertically, depth);
      batch.Draw(corner, new Rectangle(right - ir, bottom - ir, ir, ir), null, color, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, depth);
      //四条边与中心区域.
      if (innerWidth > 0)
      {
        batch.Draw(pixel, new Rectangle(centerX, top, innerWidth, ir), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
        batch.Draw(pixel, new Rectangle(centerX, bottom - ir, innerWidth, ir), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      }
      if (innerHeight > 0)
      {
        batch.Draw(pixel, new Rectangle(left, centerY, ir, innerHeight), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
        batch.Draw(pixel, new Rectangle(right - ir, centerY, ir, innerHeight), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      }
      if (innerWidth > 0 && innerHeight > 0)
        batch.Draw(pixel, new Rectangle(centerX, centerY, innerWidth, innerHeight), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
    }

    /// <summary>
    /// 绘制带描边的填充圆角矩形.
    /// </summary>
    /// <param name="batch">批次.</param>
    /// <param name="rectangle">目标矩形.</param>
    /// <param name="color">填充颜色.</param>
    /// <param name="radius">圆角半径.</param>
    /// <param name="borderWidth">描边宽度; 零表示不绘制描边.</param>
    /// <param name="borderColor">描边颜色.</param>
    /// <param name="depth">层级深度.</param>
    public static void Draw(SpriteBatch batch, Rectangle rectangle, Color color, float radius, float borderWidth, Color borderColor, float depth = 0f)
    {
      if (borderWidth <= 0f)
      {
        Draw(batch, rectangle, color, radius, depth);
        return;
      }
      Draw(batch, rectangle, borderColor, radius, depth);
      Rectangle inner = rectangle;
      inner.Inflate(-(int)MathF.Round(borderWidth), -(int)MathF.Round(borderWidth));
      if (inner.Width > 0 && inner.Height > 0)
        Draw(batch, inner, color, Math.Max(0f, radius - borderWidth), depth);
    }
  }
}
