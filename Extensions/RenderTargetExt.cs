﻿namespace Colin.Core.Extensions
{
  public static class RenderTargetExt
  {
    public static RenderTarget2D CreateDefault()
    {
      RenderTarget2D renderTarget = new RenderTarget2D(
      CoreInfo.Graphics.GraphicsDevice,
      CoreInfo.ViewWidth,
      CoreInfo.ViewHeight,
      false,
      SurfaceFormat.Color,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents);
      return renderTarget;
    }
    public static RenderTarget2D CreateDefault(int width, int height)
    {
      RenderTarget2D renderTarget = new RenderTarget2D(
      CoreInfo.Graphics.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Color,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents);
      return renderTarget;
    }
    public static RenderTarget2D CreateHDR(int width, int height)
    {
      RenderTarget2D renderTarget = new RenderTarget2D(
      CoreInfo.Graphics.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents);
      return renderTarget;
    }
    public static RenderTarget2D CreateWithDepth(int width, int height)
    {
      RenderTarget2D renderTarget = new RenderTarget2D(
      CoreInfo.Graphics.GraphicsDevice,
      width,
      height,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.Depth24,
      0,
      RenderTargetUsage.PreserveContents);
      return renderTarget;
    }
  }
}