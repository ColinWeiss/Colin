﻿namespace Colin.Core.Modulars.Collisions
{
  public class Shape
  {
    /// <summary>
    /// 指示坐标.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 指示颜色.
    /// </summary>
    public Color Color;

    public Angle Rotation;

    public Vector2 Anchor;

    public virtual RectangleF Bounds { get; }

    public Matrix View;

    public Shape(Vector2 position, Color color)
    {
      Position = position;
      Color = color;
    }

    protected VertexPositionColor[] FillVertices;

    protected VertexPositionColor[] BorderVertices;

    protected short[] FillIndicesArray;

    protected short[] BorderIndicesArray;

    public virtual void DoInitialize() { }

    public virtual void DoUpdate(GameTime gameTime) { }

    public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }

    public bool CheckCollision(Shape other)
    {
      return CollisionHandle.CheckCollision(this, other);
    }
  }
}
