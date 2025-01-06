using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Collisions
{
  public class CircleShape : Shape
  {
    /// <summary>
    /// 指示圆形的半径.
    /// </summary>
    public float Radius;

    /// <summary>
    /// 指示圆形的分段数.
    /// </summary>
    public const int Segments = 32;

    public Matrix View;

    public CircleShape(Vector2 position, Color color, float radius) : base(position, color)
    {
      Radius = radius;
    }

    public override void DoInitialize()
    {
      // 初始化顶点数组和索引数组
      FillVertices = new VertexPositionColor[Segments + 1]; // 中心点 + 分段数
      BorderVertices = new VertexPositionColor[Segments + 1]; // 描边圆形也有分段数 + 1 个顶点
      base.DoInitialize();
    }

    public override void DoUpdate(GameTime gameTime)
    {
      Vector3 center = new Vector3(Position, 0);

      // 初始化顶点列表和索引列表
      List<VertexPositionColor> vertices = new List<VertexPositionColor>();
      List<short> fillIndices = new List<short>();
      List<short> borderIndices = new List<short>();

      // 添加中心点
      short centerIndex = (short)vertices.Count;
      vertices.Add(new VertexPositionColor(center, new Color(Color, 0.5f)));

      for (int i = 0; i <= Segments; i++)
      {
        float angle = MathHelper.TwoPi / Segments * i;
        Vector2 point = new Vector2(
            Position.X + Radius * (float)Math.Cos(angle),
            Position.Y + Radius * (float)Math.Sin(angle)
        );

        // 添加当前点
        short currentIndex = (short)vertices.Count;
        vertices.Add(new VertexPositionColor(new Vector3(point, 0), new Color(Color, 0.5f)));

        // 构建填充三角形
        if (i > 0)
        {
          fillIndices.Add(centerIndex);
          fillIndices.Add((short)(currentIndex - 1));
          fillIndices.Add(currentIndex);
        }

        // 构建描边线段
        if (i > 0)
        {
          borderIndices.Add((short)(currentIndex - 1));
          borderIndices.Add(currentIndex);
        }
      }

      // 将列表转换为数组
      FillVertices = vertices.ToArray();
      FillIndicesArray = fillIndices.ToArray(); // 填充圆形的索引数组
      BorderIndicesArray = borderIndices.ToArray(); // 描边圆形的索引数组
      base.DoUpdate(gameTime);
    }

    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      // 设置图形设备状态
      device.RasterizerState = RasterizerState.CullNone;
      device.BlendState = BlendState.Additive;

      // 使用BasicEffect绘制
      using (BasicEffect basicEffect = new BasicEffect(device))
      {
        basicEffect.VertexColorEnabled = true;
        basicEffect.World = Matrix.Identity;
        basicEffect.View = View;
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1
        );

        // 绘制填充圆形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.TriangleList, // 使用 TriangleList 绘制填充圆形
              FillVertices,
              0,
              FillVertices.Length,
              FillIndicesArray, // 使用填充圆形的索引数组
              0,
              Segments // 每个分段对应一个三角形
          );
        }

        // 绘制描边圆形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.LineList, // 使用 LineList 绘制描边
              FillVertices,
              0,
              FillVertices.Length,
              BorderIndicesArray, // 使用描边圆形的索引数组
              0,
              Segments // 线段数量等于分段数
          );
        }
      }
      base.DoRender(device, batch);
    }

    private RectangleF? _bounds;
    public override RectangleF Bounds
    {
      get
      {
        if (_bounds is null)
        {
          // 计算 AABB 的最小和最大坐标
          float minX = -Radius;
          float minY = -Radius;
          float maxX = Radius;
          float maxY = Radius;

          // 创建并返回 AABB Rectangle
          _bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
        RectangleF result = _bounds.Value;
        result.Offset(Position);
        return result;
      }
    }
  }
}