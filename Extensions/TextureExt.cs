namespace Colin.Core.Extensions
{
    public static class TextureExt
    {
        public static Vector2 GetSize(this Texture2D texture)
        {
            return new Vector2(texture.Width, texture.Height);
        }
    }
}
