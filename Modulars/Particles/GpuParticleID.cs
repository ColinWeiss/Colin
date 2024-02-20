namespace Colin.Core.Modulars.Particles
{
    public struct GpuParticleID
    {
        public float ID;
        public static readonly VertexDeclaration VertexDeclaration;
        static GpuParticleID()
        {
            VertexElement[] instanceStreamElements = new VertexElement[1]
            {
                new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),
            };
            VertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }
    }
}