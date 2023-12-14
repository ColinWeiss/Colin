using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Particles
{
    public struct ParticleInfo
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Rotation;
        public static readonly VertexDeclaration VertexDeclaration;
        static ParticleInfo()
        {
            VertexElement[] instanceStreamElements = new VertexElement[4];
            instanceStreamElements[0] = new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1);
            instanceStreamElements[1] = new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.Position, 2);
            instanceStreamElements[2] = new VertexElement(sizeof(float) * 4, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1);
            instanceStreamElements[3] = new VertexElement(sizeof(float) * 5, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2);
            VertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }
    }
}