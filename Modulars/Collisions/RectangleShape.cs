using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Collisions
{
  public class RectangleShape : PolygonShape
  {
    /// <summary>
    /// 指示矩形的宽度.
    /// </summary>
    public float Width { get; private set; }

    /// <summary>
    /// 指示矩形的高度.
    /// </summary>
    public float Height { get; private set; }

    public RectangleShape(Vector2 position, Color color, float width, float height)
        : base(position, color, GenerateVertices(width, height))
    {
      Width = width;
      Height = height;
    }

    /// <summary>
    /// 生成矩形的顶点列表
    /// </summary>
    /// <param name="width">矩形的宽度</param>
    /// <param name="height">矩形的高度</param>
    /// <returns>矩形的顶点列表</returns>
    private static List<Vector2> GenerateVertices(float width, float height)
    {
      return new List<Vector2>
        {
            new Vector2(0, 0),          // 左上角
            new Vector2(width * 1.00001f, 0),      // 右上角
            new Vector2(width* 1.00001f, height* 1.00001f), // 右下角
            new Vector2(0, height* 1.00001f)      // 左下角
        };
    }
  }
}