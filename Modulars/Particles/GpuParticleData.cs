﻿namespace Colin.Core.Modulars.Particles
{
  public struct GpuParticleData
  {
    public Color Color;
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Scale;
    public float Rotation;
    public float ActiveTime;
    internal float ID;

    public static readonly VertexDeclaration VertexDeclaration;
    static GpuParticleData()
    {
      VertexElement[] instanceStreamElements = new VertexElement[7]
      {
                new VertexElement(0, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 1, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 7, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(sizeof(float) * 9, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5),
      };
      VertexDeclaration = new VertexDeclaration(instanceStreamElements);
    }
  }
}