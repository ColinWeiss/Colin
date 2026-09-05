namespace Particle.Slash
{
  /// <summary>
  /// 条带网格构建器 (拉刀光核心): 把轨迹点列直接展开成三角带 Mesh ——
  /// 每个轨迹点沿切线法线两侧各摆一个顶点, 相邻点对组成三角形,
  /// 宽度/颜色/UV 均为逐点输入 (月牙形宽度、头亮尾隐都由调用方给出).
  /// </summary>
  public static class RibbonBuilder
  {
    /// <summary>
    /// 构建条带顶点与索引.
    /// </summary>
    /// <param name="points">轨迹点列 (顺序: 头部在前 —— 即扫动前缘/最新位置在前).</param>
    /// <param name="halfWidths">每点的半宽 (像素), 与点列等长.</param>
    /// <param name="colors">每点顶点色 rgba.</param>
    /// <param name="us">每点 UV.u (0=尾, 1=头; 与纹理渐变方向一致).</param>
    /// <param name="vertices">输出顶点 (2 × 点数).</param>
    /// <param name="indices">输出索引 (三角带, 3 × (点数-1) × 2).</param>
    /// <returns>实际写入的三角带段数 (点数-1; 不足两点时为 0).</returns>
    public static int Build(
        IReadOnlyList<Vector2> points,
        IReadOnlyList<float> halfWidths,
        IReadOnlyList<Vector4> colors,
        IReadOnlyList<float> us,
        SlashVertex[] vertices,
        short[] indices)
    {
      int count = points.Count;
      if (count < 2)
        return 0;

      // —— 顶点: 每点沿法线两侧展开 ——
      for (int i = 0; i < count; i++)
      {
        // 中心差分求切线 (端点用单侧), 法线 = 切线逆时针旋转 90°.
        Vector2 previous = points[Math.Max(0, i - 1)];
        Vector2 next = points[Math.Min(count - 1, i + 1)];
        Vector2 tangent = next - previous;
        float length = tangent.Length();
        Vector2 normal = length > 1e-4f ? new Vector2(-tangent.Y, tangent.X) / length : Vector2.UnitY;

        float halfWidth = halfWidths[i];
        Vector4 color = colors[i];
        float u = us[i];

        vertices[i * 2].Position = points[i] + normal * halfWidth;
        vertices[i * 2].TexCoord = new Vector2(u, 0f);
        vertices[i * 2].Color = color;

        vertices[i * 2 + 1].Position = points[i] - normal * halfWidth;
        vertices[i * 2 + 1].TexCoord = new Vector2(u, 1f);
        vertices[i * 2 + 1].Color = color;
      }

      // —— 索引: 三角带 ——
      int segments = count - 1;
      for (int i = 0; i < segments; i++)
      {
        int baseIndex = (ushort)(i * 2);
        indices[i * 6 + 0] = (short)baseIndex;
        indices[i * 6 + 1] = (short)(baseIndex + 1);
        indices[i * 6 + 2] = (short)(baseIndex + 2);
        indices[i * 6 + 3] = (short)(baseIndex + 1);
        indices[i * 6 + 4] = (short)(baseIndex + 3);
        indices[i * 6 + 5] = (short)(baseIndex + 2);
      }

      return segments;
    }
  }
}
