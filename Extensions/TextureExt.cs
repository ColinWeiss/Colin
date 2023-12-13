using Colin.Core.Graphics.Shaders;

namespace Colin.Core.Extensions
{
    public static class TextureExt
    {
        public static Texture2D Create(int width, int height)
        {
            return new Texture2D(EngineInfo.Graphics.GraphicsDevice, width, height);
        }
        public static UnorderedAccessTexture2D CreateUAV(int width, int height)
        {
            return new UnorderedAccessTexture2D(width, height);
        }
        public static Vector2 GetSize(this Texture2D texture)
        {
            return new Vector2(texture.Width, texture.Height);
        }
    }
}
