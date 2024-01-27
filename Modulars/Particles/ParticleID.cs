namespace Colin.Core.Modulars.Particles
{
    public struct ParticleID
    {
        public float ID;
        public static readonly VertexDeclaration VertexDeclaration;
        static ParticleID()
        {
            VertexElement[] instanceStreamElements = new VertexElement[1]
            {
                new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),
            };
            VertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }
    }
}