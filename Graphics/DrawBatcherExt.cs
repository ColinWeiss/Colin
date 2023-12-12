namespace Colin.Core.Graphics
{
    public static class DrawBatcherExt
    {
        public static void DrawRect(this IDrawBatcher<Vert2> batcher, Texture2D texture, float oX, float oY, float dX, float dY, Color color)
        {
            batcher.DrawQuad(texture
                , new Vert2(new Vector2(oX, oY), color, new Vector2(0, 0))
                , new Vert2(new Vector2(oX + dX, oY), color, new Vector2(1, 0))
                , new Vert2(new Vector2(oX + dX, oY + dY), color, new Vector2(1, 1))
                , new Vert2(new Vector2(oX, oY + dY), color, new Vector2(0, 1))
                );
        }
        public static void DrawRect(this IDrawBatcher<Vert2> batcher, Texture2D texture, Rectangle target, Color color)
        {
            DrawRect(batcher, texture, target.X, target.Y, target.Width, target.Height, color);
        }
        public static void DrawRect(this IDrawBatcher<Vert2> batcher, Texture2D texture, float oX, float oY, float scale, Color color)
        {
            DrawRect(batcher, texture, oX, oY, texture.Width * scale, texture.Height * scale, color);
        }
        public static void DrawRect(this IDrawBatcher<Vert2> batcher, Texture2D texture, Vector2 pos, float scale, Color color)
        {
            DrawRect(batcher, texture, pos.X, pos.Y, texture.Width * scale, texture.Height * scale, color);
        }
        public unsafe static void Draw(this IDrawBatcher<Vert2> batcher, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, int sortingKey)
        {
            float dX_Y;
            float dX;
            float dY_X;
            float dY;
            float w;
            float h;
            float sourceOX;
            float sourceOY;
            float sourceDX;
            float sourceDY;
            if (sourceRectangle is Rectangle r)
            {
                w = r.Width * scale;
                h = r.Height * scale;
                sourceOX = (float)r.X / texture.Width;
                sourceOY = (float)r.Y / texture.Height;
                sourceDX = (float)r.Width / texture.Width;
                sourceDY = (float)r.Height / texture.Height;
            }
            else
            {
                w = texture.Width * scale;
                h = texture.Height * scale;
                sourceOX = 0;
                sourceOY = 0;
                sourceDX = 1;
                sourceDY = 1;
            }
            if (rotation != 0f)
            {
                var c = MathF.Cos(rotation);
                var s = MathF.Sin(rotation);
                dX_Y = s * w;
                dX = c * w;
                dY_X = -s * h;
                dY = c * h;
            }
            else
            {
                dX_Y = 0;
                dX = w;
                dY_X = 0;
                dY = h;
            }
            origin.X *= scale;
            origin.Y *= scale;
            position.X -= origin.X;
            position.Y -= origin.Y;
            if (effects == SpriteEffects.None) ;
            else if (effects == SpriteEffects.FlipHorizontally)
            {
                sourceDX = -sourceDX;
                sourceOX -= sourceDX;
            }
            else if (effects == SpriteEffects.FlipVertically)
            {
                sourceDY = -sourceDY;
                sourceOY -= sourceDY;
            }
            batcher.DrawQuad(texture
                , new Vert2(new Vector2(position.X, position.Y), color, new Vector2(sourceOX, sourceOY))
                , new Vert2(new Vector2(position.X + dX, position.Y + dX_Y), color, new Vector2(sourceOX + sourceDX, sourceOY))
                , new Vert2(new Vector2(position.X + dX + dY_X, position.Y + dY + dX_Y), color, new Vector2(sourceOX + sourceDX, sourceOY + sourceDY))
                , new Vert2(new Vector2(position.X + dY_X, position.Y + dY), color, new Vector2(sourceOX, sourceOY + sourceDY))
                , sortingKey);
        }
    }
}