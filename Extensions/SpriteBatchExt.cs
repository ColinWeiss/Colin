namespace Colin.Core.Extensions
{
  /// <summary>
  /// 与 <see cref="SpriteBatch"/> 相关的扩展方法.
  /// </summary>
  public static class SpriteBatchExt
  {
    /// <summary>
    /// 绘制一条线.
    /// </summary>
    public static void DrawLine(this SpriteBatch batch, Line line, Color color)
    {
      float radian = (line.End - line.Start).GetRadian();
      Sprite pixel = Sprite.Get("Pixel");
      float depth = pixel.Depth;
      if (pixel is not null)
        batch.Draw(pixel.Source , line.Start, null, color, radian, Vector2.Zero, new Vector2(Vector2.Distance(line.Start, line.End), 1f), SpriteEffects.None, depth);
    }

    /// <summary>
    /// 绘制一条线.
    /// </summary>
    public static void DrawLine(this SpriteBatch batch, Vector2 start, Vector2 end, Color color)
    {
      batch.DrawLine(new Line(start, end), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    public static void DrawRectangle(this SpriteBatch batch, Vector2 start, Vector2 size, Color color)
    {
      batch.DrawRectangle(new Rectangle(start.ToPoint(), size.ToPoint()), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    public static void DrawRectangle(this SpriteBatch batch, Point start, Point size, Color color)
    {
      batch.DrawRectangle(new Rectangle(start, size), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    public static void DrawRectangle(this SpriteBatch batch, float x, float y, float width, float height, Color color)
    {
      batch.DrawRectangle(new Rectangle((int)x, (int)y, (int)width, (int)height), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    public static void DrawRectangle(this SpriteBatch batch, int x, int y, int width, int height, Color color)
    {
      batch.DrawRectangle(new Rectangle(x, y, width, height), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    public static void DrawRectangle(this SpriteBatch batch, RectangleF rect, Color color)
    {
      batch.DrawRectangle(new Rectangle((int)Math.Round(rect.X), (int)Math.Round(rect.Y), (int)rect.Width, (int)rect.Height), color);
    }

    /// <summary>
    /// 矩形绘制.
    /// </summary>
    public static void DrawRectangle(this SpriteBatch batch, Rectangle rect, Color color)
    {
      batch.Draw(Asset.GetTexture("Pixel"), rect, color);
    }

    /// <summary>
    /// 使用线绘制矩形.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="rect"></param>
    /// <param name="color"></param>
    public static void DrawRectangleLine(this SpriteBatch batch, Rectangle rect, Color color)
    {
      batch.DrawLine(new Line(new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y)), color);
      batch.DrawLine(new Line(new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height)), color);
      batch.DrawLine(new Line(new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height)), color);
      batch.DrawLine(new Line(new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height)), color);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, float x, float y, int Width, int Height, Point cut, float depth)
    {
      Point borderSize = cut;
      Rectangle leftTop = new Rectangle((int)x, (int)y, cut.X, cut.Y);
      Rectangle rightTop = new Rectangle((int)x + Width - cut.X, (int)y, cut.X, cut.Y);
      Rectangle leftBottom = new Rectangle((int)x, (int)y + Height - cut.Y, cut.X, cut.Y);
      Rectangle rightBottom = new Rectangle((int)x + Width - cut.X, (int)y + Height - cut.Y, cut.X, cut.Y);
      Rectangle top = new Rectangle((int)x + cut.X, (int)y, Width - cut.X * 2, cut.Y);
      Rectangle left = new Rectangle((int)x, (int)y + cut.Y, cut.X, Height - cut.Y * 2);
      Rectangle right = new Rectangle((int)x + Width - cut.X, (int)y + cut.Y, cut.X, Height - cut.Y * 2);
      Rectangle bottom = new Rectangle((int)x + cut.X, (int)y + Height - cut.Y, Width - cut.X * 2, cut.Y);
      Rectangle center = new Rectangle((int)x + cut.X, (int)y + cut.Y, Width - cut.X * 2, Height - cut.Y * 2);
      batch.Draw(texture, leftTop, new Rectangle(Point.Zero, borderSize), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, rightTop, new Rectangle(new Point(texture.Width - cut.X, 0), borderSize), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, leftBottom, new Rectangle(new Point(0, texture.Height - cut.Y), borderSize), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, rightBottom, new Rectangle(new Point(texture.Width - cut.X, texture.Height - cut.Y), borderSize), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, top, new Rectangle(cut.X, 0, texture.Width - cut.X * 2, cut.Y), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, left, new Rectangle(0, cut.Y, cut.X, texture.Height - cut.Y * 2), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, right, new Rectangle(texture.Width - cut.X, cut.Y, cut.X, texture.Height - cut.Y * 2), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, bottom, new Rectangle(cut.X, texture.Height - cut.Y, texture.Width - cut.X * 2, cut.Y), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
      batch.Draw(texture, center, new Rectangle(cut.X, cut.Y, texture.Width - cut.X * 2, texture.Height - cut.Y * 2), color, 0f, Vector2.Zero, SpriteEffects.None, depth);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="pos"></param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, Vector2 pos, int Width, int Height, Point cut, float depth)
    {
      batch.DrawNineCut(texture, color, (int)pos.X, (int)pos.Y, Width, Height, cut, depth);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, Vector2 pos, Point size, Point cut, float depth)
    {
      batch.DrawNineCut(texture, color, (int)pos.X, (int)pos.Y, size.X, size.Y, cut, depth);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, Vector2 pos, Vector2 size, Point cut, float depth)
    {
      batch.DrawNineCut(texture, color, (int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y, cut, depth);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, Point pos, Point size, Point cut, float depth)
    {
      batch.DrawNineCut(texture, color, pos.X, pos.Y, size.X, size.Y, cut, depth);
    }

    /// <summary>
    /// 九宫绘制.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="rec"></param>
    /// <param name="cut"></param>
    /// <param name="depth"></param>
    public static void DrawNineCut(this SpriteBatch batch, Texture2D texture, Color color, Rectangle rec, Point cut, float depth)
    {
      batch.DrawNineCut(texture, color, rec.X, rec.Y, rec.Width, rec.Height, cut, depth);
    }
  }
}