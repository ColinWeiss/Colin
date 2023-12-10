namespace Colin.Core.Graphics
{
    public struct Vert2 : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector2 TexCoord;
        public static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        public Vert2(Vector2 position, Color color, Vector2 textureCoord)
        {
            Position = position;
            Color = color;
            TexCoord = textureCoord;
        }
    }
}