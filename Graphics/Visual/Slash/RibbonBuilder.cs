namespace Colin.Core.Graphics.Visual.Slash
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
    /// <param name="halfWidths">每点的半宽 (像素).</param>
    /// <param name="colors">每点顶点色 rgba.</param>
    /// <param name="us">每点 UV.u (0=尾, 1=头; 与纹理渐变方向一致).</param>
    /// <param name="count">实际参与构网的点数 —— 调用方的缓存数组可能比它大,
    /// 超出部分是上一帧的陈旧位置, 绝不允许进入索引 (否则出现拉向旧顶点的撕拉鬼影).</param>
    /// <param name="vertices">输出顶点 (2 × count).</param>
    /// <param name="indices">输出索引 (三角带, 3 × (count-1) × 2).</param>
    /// <param name="vCenter">纹理 V 定侧参考中心 (弧心, 模型空间). 提供时<b>远离中心的一侧恒为 V=1</b> ——
    /// 定侧只看几何位置、不看扫动方向, 反向挥扫/起止角对调/镜像都不再把纹理翻面;
    /// 缺省退回 ±法线定侧 (V: +法线侧 0, −法线侧 1, 随扫向翻转).</param>
    /// <returns>实际写入的三角带段数 (count-1; 不足两点时为 0).</returns>
    public static int Build(
        IReadOnlyList<Vector2> points,
        IReadOnlyList<float> halfWidths,
        IReadOnlyList<Vector4> colors,
        IReadOnlyList<float> us,
        int count,
        SlashVertex[] vertices,
        short[] indices,
        Vector2? vCenter = null)
    {
      if (count < 2 || count > points.Count)
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

        // —— V 定侧: 参考中心给定时, 远离中心 (朝外) 的一侧恒为 V=1 ——
        // 弧形刀光的法线恒为径向, dot(法线, 点位-弧心) 的符号即朝向内外,
        // 全弧连续无跳变 (逐点比 Y 的做法会在弧线左右极点处翻面).
        float vPlus = 0f, vMinus = 1f;
        if (vCenter.HasValue && Vector2.Dot(normal, points[i] - vCenter.Value) > 0f)
        {
          vPlus = 1f;
          vMinus = 0f;
        }

        vertices[i * 2].Position = points[i] + normal * halfWidth;
        vertices[i * 2].TexCoord = new Vector2(u, vPlus);
        vertices[i * 2].Color = color;

        vertices[i * 2 + 1].Position = points[i] - normal * halfWidth;
        vertices[i * 2 + 1].TexCoord = new Vector2(u, vMinus);
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
