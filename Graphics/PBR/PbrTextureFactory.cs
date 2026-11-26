namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 贴图辅助: 把无 mip 的运行时贴图 (Texture2D.FromStream) 复制成带完整 mip 链的纹理.
  /// <br>环境天空盒需要 mip 才能按粗糙度做模糊采样; 模型贴图也可选用来减少远处闪烁.</br>
  /// </summary>
  public static class PbrTextureFactory
  {
    /// <summary>生成带 mip 链的副本; 返回的纹理由调用方释放, 源纹理不受影响.</summary>
    public static Texture2D CreateMipMapped(GraphicsDevice device, Texture2D source)
    {
      if (source is null)
        return null;

      int width = source.Width;
      int height = source.Height;
      Texture2D result = new Texture2D(device, width, height, true, SurfaceFormat.Color);
      Color[] level = new Color[width * height];
      source.GetData(level);
      result.SetData(0, null, level, 0, level.Length);

      int currentWidth = width;
      int currentHeight = height;
      for (int mip = 1; currentWidth > 1 || currentHeight > 1; mip++)
      {
        int nextWidth = Math.Max(1, currentWidth / 2);
        int nextHeight = Math.Max(1, currentHeight / 2);
        Color[] next = Downsample(level, currentWidth, currentHeight, nextWidth, nextHeight);
        result.SetData(mip, null, next, 0, next.Length);
        level = next;
        currentWidth = nextWidth;
        currentHeight = nextHeight;
      }

      result.Name = source.Name + " (mips)";
      return result;
    }

    private static Color[] Downsample(Color[] source, int sourceWidth, int sourceHeight, int width, int height)
    {
      Color[] destination = new Color[width * height];
      for (int y = 0; y < height; y++)
      {
        int y0 = y * sourceHeight / height;
        int y1 = Math.Max(y0 + 1, (y + 1) * sourceHeight / height);
        for (int x = 0; x < width; x++)
        {
          int x0 = x * sourceWidth / width;
          int x1 = Math.Max(x0 + 1, (x + 1) * sourceWidth / width);

          float red = 0f;
          float green = 0f;
          float blue = 0f;
          float alpha = 0f;
          int count = 0;
          for (int sy = y0; sy < y1; sy++)
          {
            int row = sy * sourceWidth;
            for (int sx = x0; sx < x1; sx++)
            {
              Color color = source[row + sx];
              float a = color.A / 255f;
              red += color.R * a;
              green += color.G * a;
              blue += color.B * a;
              alpha += a;
              count++;
            }
          }

          if (alpha > 0f)
          {
            destination[(y * width) + x] = new Color(
                (int)MathF.Round(red / alpha),
                (int)MathF.Round(green / alpha),
                (int)MathF.Round(blue / alpha),
                (int)MathF.Round(alpha / count * 255f));
          }
        }
      }
      return destination;
    }
  }
}
