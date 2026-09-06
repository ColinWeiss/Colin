namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>
  /// 刀光条带顶点: 位置 (世界/局部 2D) + 纹理坐标 + 顶点色.
  /// <br>顶点由 <see cref="RibbonBuilder"/> 沿轨迹直接摆放 (拉刀光 Mesh).</br>
  /// </summary>
  public struct SlashVertex : IVertexType
  {
    /// <summary>位置 (像素).</summary>
    public Vector2 Position;
    /// <summary>纹理坐标 (u 沿刀光长度: 0=尾, 1=头).</summary>
    public Vector2 TexCoord;
    /// <summary>顶点色 rgba (渐隐/配色在此完成).</summary>
    public Vector4 Color;

    /// <summary>顶点声明.</summary>
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
      new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
      new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
      new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Color, 0));

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
  }
}
