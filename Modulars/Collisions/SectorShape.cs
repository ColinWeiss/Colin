namespace Colin.Core.Modulars.Collisions
{
  public class SectorShape : Shape
  {
    /// <summary>
    /// 指示扇形半径.
    /// </summary>
    public float Radius;

    /// <summary>
    /// 指示扇形起始角度.
    /// </summary>
    public float StartAngle;

    /// <summary>
    /// 指示扇形扫过角度.
    /// </summary>
    public float SweepAngle;

    /// <summary>
    /// 指示扇形分段数.
    /// </summary>
    public const int Segments = 16;

    public SectorShape(Vector2 position, Color color, float radius, float startAngle, float sweepAngle) : base(position, color)
    {
      Radius = radius;
      StartAngle = startAngle;
      SweepAngle = sweepAngle;
    }

    public override void DoInitialize()
    {
      FillVertices = new VertexPositionColor[(Segments + 1) * 3]; // 每个扇形段需要 3 个顶点
      BorderVertices = new VertexPositionColor[(Segments + 3) * 2]; // 每条描边线段需要 2 个顶点
      base.DoInitialize();
    }

    public override void DoUpdate(GameTime gameTime)
    {
      Vector3 center = new Vector3(Position, 0);

      float startAngleOffsetResult = StartAngle + Rotation.RadiansF;

      // 初始化顶点列表和索引列表
      List<VertexPositionColor> vertices = new List<VertexPositionColor>();
      List<short> fillIndices = new List<short>();
      List<short> borderIndices = new List<short>();

      // 添加中心点
      short centerIndex = (short)vertices.Count;
      vertices.Add(new VertexPositionColor(center, new Color(Color, 0.5f)));

      // 添加起始边缘点
      Vector2 previousPoint = new Vector2(
          Position.X + Radius * (float)Math.Cos(startAngleOffsetResult),
          Position.Y + Radius * (float)Math.Sin(startAngleOffsetResult)
      );
      short previousIndex = (short)vertices.Count;
      vertices.Add(new VertexPositionColor(new Vector3(previousPoint, 0), new Color(Color, 0.5f)));

      // 添加第一条直线边（从中心点到起始边缘点）
      borderIndices.Add(centerIndex);
      borderIndices.Add(previousIndex);

      for (int i = 0; i <= Segments; i++)
      {
        float angle = startAngleOffsetResult + (SweepAngle / Segments) * i;
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
          fillIndices.Add(previousIndex);
          fillIndices.Add(currentIndex);
        }

        // 构建描边线段（边缘弧线）
        if (i > 0)
        {
          borderIndices.Add(previousIndex);
          borderIndices.Add(currentIndex);
        }

        previousIndex = currentIndex; // 保存当前点索引作为下一个三角形的上一个点
      }

      // 添加第二条直线边（从中心点到结束边缘点）
      borderIndices.Add(centerIndex);
      borderIndices.Add(previousIndex);

      // 将列表转换为数组
      FillVertices = vertices.ToArray();
      FillIndicesArray = fillIndices.ToArray(); // 填充扇形的索引数组
      BorderIndicesArray = borderIndices.ToArray(); // 描边扇形的索引数组
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
        basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1
        );

        // 绘制填充扇形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.TriangleList, // 使用 TriangleList 绘制填充扇形
              FillVertices,
              0,
              FillVertices.Length,
              FillIndicesArray, // 使用填充扇形的索引数组
              0,
              Segments // 每个扇形段对应一个三角形
          );
        }

        // 绘制描边扇形
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
          pass.Apply();
          device.DrawUserIndexedPrimitives<VertexPositionColor>(
              PrimitiveType.LineList, // 使用 LineList 绘制描边
              FillVertices,
              0,
              FillVertices.Length,
              BorderIndicesArray, // 使用描边扇形的索引数组
              0,
              (Segments + 2) // 线段数量等于扇形段数 + 2（两条直线边）
          );
        }
      }
      base.DoRender(device, batch);
    }

    /// <summary>
    /// 检测给定角度是否在扇形的角度范围内
    /// </summary>
    public bool IsAngleInRange(float angle)
    {
      // 将角度归一化到 [0, 2π) 范围内
      angle = NormalizeAngle(angle);
      float start = NormalizeAngle(StartAngle);
      float end = NormalizeAngle(StartAngle + SweepAngle);

      if (end > start)
      {
        return angle >= start && angle <= end;
      }
      else
      {
        return angle >= start || angle <= end;
      }
    }

    /// <summary>
    /// 将角度归一化到 [0, 2π) 范围内
    /// </summary>
    private float NormalizeAngle(float angle)
    {
      while (angle < 0) angle += MathHelper.TwoPi;
      while (angle >= MathHelper.TwoPi) angle -= MathHelper.TwoPi;
      return angle;
    }

    private RectangleF? _bounds;
    public override RectangleF Bounds
    {
      get
      {
        if (_bounds is null)
        {
          // 计算 AABB 的最小和最大坐标
          float minX = -Radius;  // 从中心点开始计算
          float minY = -Radius;  // 从中心点开始计算
          float maxX = Radius;   // 从中心点开始计算
          float maxY = Radius;   // 从中心点开始计算

          // 根据起始角度和扫过角度计算扇形的边界
          float endAngle = StartAngle + SweepAngle;

          Vector2 point1 = new Vector2(Radius * (float)Math.Cos(StartAngle), Radius * (float)Math.Sin(StartAngle));
          Vector2 point2 = new Vector2(Radius * (float)Math.Cos(endAngle), Radius * (float)Math.Sin(endAngle));

          // 更新 AABB 的最小和最大坐标
          minX = Math.Min(minX, Math.Min(point1.X, point2.X));
          minY = Math.Min(minY, Math.Min(point1.Y, point2.Y));
          maxX = Math.Max(maxX, Math.Max(point1.X, point2.X));
          maxY = Math.Max(maxY, Math.Max(point1.Y, point2.Y));

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