using DeltaMachine.Windows;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Collisions
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

    public bool CollidesWith(Shape other)
    {
      return CollisionHandle.CheckCollision(this, other);
    }
  }
}
