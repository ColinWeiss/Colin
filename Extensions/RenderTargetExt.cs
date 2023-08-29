namespace Colin.Core.Extensions
{
    public static class RenderTargetExt
    {
        public static RenderTarget2D CreateDefault( )
        {
            RenderTarget2D renderTarget = new RenderTarget2D(
            EngineInfo.Graphics.GraphicsDevice,
            EngineInfo.ViewWidth,
            EngineInfo.ViewHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents );
            return renderTarget;
        }

        public static RenderTarget2D CreateDefault( int width, int height )
        {
            RenderTarget2D renderTarget = new RenderTarget2D(
            EngineInfo.Graphics.GraphicsDevice,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents );
            return renderTarget;
        }
    }
}