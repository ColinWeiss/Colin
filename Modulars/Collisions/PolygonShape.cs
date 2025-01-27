using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin.Core.Modulars.Collisions
{
  public class PolygonShape : Shape
  {
    public Matrix View;

    /// <summary>
    /// 指示多边形的顶点列表.
    /// </summary>
    public List<Vector2> Vertices { get; private set; }

    public PolygonShape(Vector2 position, Color color, List<Vector2> vertices, float rotation = 0, Vector2 anchor = default(Vector2)) : base(position, color)
    {
      Vertices = vertices;
      Rotation = rotation; // 初始化 Rotation 字段
      Anchor = anchor; // 初始化 Anchor 字段
    }

    public override void DoInitialize()
    {
      // 初始化顶点数组和索引数组
      FillVertices = new VertexPositionColor[Vertices.Count + 1]; // 中心点 + 顶点数
      BorderVertices = new VertexPositionColor[Vertices.Count + 1]; // 描边多边形也有顶点数 + 1 个顶点
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

      for (int i = 0; i < Vertices.Count; i++)
      {
        Vector2 point = Vertices[i] + Position;

        // 旋转点围绕 Anchor
        point = RotatePointAroundAnchor(point, Rotation.RadiansF, Anchor);

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

      // 闭合多边形
      if (Vertices.Count > 1)
      {
        fillIndices.Add(centerIndex);
        fillIndices.Add((short)(vertices.Count - 1));
        fillIndices.Add(1);

        borderIndices.Add((short)(vertices.Count - 1));
        borderIndices.Add(1);
      }

      // 将列表转换为数组
      FillVertices = vertices.ToArray();
      FillIndicesArray = fillIndices.ToArray(); // 填充多边形的索引数组
      BorderIndicesArray = borderIndices.ToArray(); // 描边多边形的索引数组
      base.DoUpdate(gameTime);
    }

    // 辅助方法：旋转点围绕 Anchor
    private Vector2 RotatePointAroundAnchor(Vector2 point, float rotation, Vector2 anchor)
    {
      float cos = (float)Math.Cos(rotation);
      float sin = (float)Math.Sin(rotation);
      Vector2 rotatedPoint = new Vector2(
          cos * (point.X - anchor.X) - sin * (point.Y - anchor.Y) + anchor.X,
          sin * (point.X - anchor.X) + cos * (point.Y - anchor.Y) + anchor.Y
      );
      return rotatedPoint;
    }

    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      // 设置图形设备状态
      device.RasterizerState = RasterizerState.CullNone;
      device.BlendState = BlendState.Additive;

      // 使用 BasicEffect 绘制
      using (BasicEffect basicEffect = new BasicEffect(device))
      {
        basicEffect.VertexColorEnabled = true;
        basicEffect.World = Matrix.CreateTranslation(-Anchor.X, -Anchor.Y, 0) * // 移动到 Anchor
                            Matrix.CreateRotationZ(Rotation.RadiansF) *                   // 旋转
                            Matrix.CreateTranslation(Anchor.X, Anchor.Y, 0);       // 移动回原点
        basicEffect.View = View;
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1
        );

        // 绘制填充多边形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.TriangleList, // 使用 TriangleList 绘制填充多边形
              FillVertices,
              0,
              FillVertices.Length,
              FillIndicesArray, // 使用填充多边形的索引数组
              0,
              Vertices.Count // 每个顶点对应一个三角形
          );
        }

        // 绘制描边多边形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.LineList, // 使用 LineList 绘制描边
              FillVertices,
              0,
              FillVertices.Length,
              BorderIndicesArray, // 使用描边多边形的索引数组
              0,
              Vertices.Count // 线段数量等于顶点数
          );
        }
      }
      base.DoRender(device, batch);
    }

    /// <summary>
    /// 检测当前多边形与另一个多边形是否发生碰撞
    /// </summary>
    /// <param name="other">另一个多边形</param>
    /// <returns>如果发生碰撞返回 true, 否则返回 false</returns>
    public bool CollidesWith(PolygonShape other)
    {
      // 获取当前多边形和另一个多边形的边法线
      List<Vector2> axes1 = GetAxes();
      List<Vector2> axes2 = other.GetAxes();

      // 合并所有可能的分离轴
      List<Vector2> allAxes = new List<Vector2>(axes1);
      allAxes.AddRange(axes2);

      // 检查所有分离轴
      foreach (Vector2 axis in allAxes)
      {
        if (!OverlapOnAxis(axis, other))
        {
          // 如果存在一个分离轴, 说明两个多边形不相交
          return false;
        }
      }

      // 如果没有找到分离轴, 说明两个多边形相交
      return true;
    }

    /// <summary>
    /// 获取多边形的所有边法线（分离轴）
    /// </summary>
    /// <returns>边法线列表</returns>
    public List<Vector2> GetAxes()
    {
      List<Vector2> axes = new List<Vector2>();

      for (int i = 0; i < Vertices.Count; i++)
      {
        Vector2 p1 = Vertices[i];
        Vector2 p2 = Vertices[(i + 1) % Vertices.Count];

        // 计算边的方向向量
        Vector2 edge = p2 - p1;

        // 计算边的法线（垂直于边的向量）
        Vector2 normal = new Vector2(-edge.Y, edge.X);
        normal.Normalize();

        axes.Add(normal);
      }

      return axes;
    }

    /// <summary>
    /// 检查两个多边形在给定轴上的投影是否重叠
    /// </summary>
    /// <param name="axis">分离轴</param>
    /// <param name="other">另一个多边形</param>
    /// <returns>如果重叠返回 true, 否则返回 false</returns>
    private bool OverlapOnAxis(Vector2 axis, PolygonShape other)
    {
      // 获取当前多边形在轴上的投影
      float min1, max1;
      ProjectOntoAxis(axis, out min1, out max1);

      // 获取另一个多边形在轴上的投影
      float min2, max2;
      other.ProjectOntoAxis(axis, out min2, out max2);

      // 检查投影是否重叠
      return !(max1 < min2 || max2 < min1);
    }

    /// <summary>
    /// 将多边形投影到给定轴上, 并计算投影的最小值和最大值
    /// </summary>
    /// <param name="axis">投影轴</param>
    /// <param name="min">投影的最小值</param>
    /// <param name="max">投影的最大值</param>
    public void ProjectOntoAxis(Vector2 axis, out float min, out float max)
    {
      min = float.MaxValue;
      max = float.MinValue;

      foreach (Vector2 vertex in Vertices)
      {
        Vector2 transformedVertex = vertex + Position;

        // 计算顶点在轴上的投影
        float projection = Vector2.Dot(transformedVertex, axis);

        // 更新最小值和最大值
        if (projection < min) min = projection;
        if (projection > max) max = projection;
      }
    }

    private RectangleF? _bounds;
    public override RectangleF Bounds
    {
      get
      {
        if (_bounds is null)
        {
          if (Vertices.Count == 0)
          {
            throw new InvalidOperationException("多边形没有顶点, 无法计算 AABB。");
          }

          // 初始化最小值和最大值
          float minX = float.MaxValue;
          float minY = float.MaxValue;
          float maxX = float.MinValue;
          float maxY = float.MinValue;

          // 遍历所有顶点以计算 AABB
          foreach (Vector2 vertex in Vertices)
          {
            Vector2 transformedVertex = vertex + Position;
            transformedVertex = RotatePointAroundAnchor(transformedVertex, Rotation.RadiansF, Anchor);

            if (transformedVertex.X < minX) minX = transformedVertex.X;
            if (transformedVertex.Y < minY) minY = transformedVertex.Y;
            if (transformedVertex.X > maxX) maxX = transformedVertex.X;
            if (transformedVertex.Y > maxY) maxY = transformedVertex.Y;
          }
          _bounds = new RectangleF(minX, minY, (maxX - minX), (maxY - minY));
        }
        RectangleF result = _bounds.Value;
        result.Offset(Position);
        return result;
      }
    }
  }
}
