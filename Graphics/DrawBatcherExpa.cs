namespace DeltaMachine.Core.Common.Graphics
{
    public static class DrawBatcherExpa
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
            float c = 1f;
            float s = 0f;
            if (rotation != 0f)
            {
                c = MathF.Cos(rotation);
                s = MathF.Sin(rotation);
            }
            origin *= scale;
            var sR = sourceRectangle.HasValue ? sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            var dX_Y = s * sR.Width * scale;
            var dX = c * sR.Width * scale;
            var dY_X = -s * sR.Height * scale;
            var dY = c * sR.Height * scale;
            position.X -= origin.X;
            position.Y -= origin.Y;
            var sourceOX = (float)sR.X / texture.Width;
            var sourceOY = (float)sR.Y / texture.Height;
            var sourceDX = (float)sR.Width / texture.Width;
            var sourceDY = (float)sR.Height / texture.Height;
            if (effects == SpriteEffects.FlipHorizontally)
            {
                sourceDX = -sourceDX;
                sourceOX -= sourceDX;
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