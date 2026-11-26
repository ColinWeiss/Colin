namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 4 个骨骼槽下标. 用自定义 blittable 结构而非 MonoGame 的 Short4 (后者不暴露分量),
  /// 顶点声明仍按 <see cref="VertexElementFormat.Short4"/> 解析.
  /// </summary>
  public struct PbrBoneIndices
  {
    public short X;
    public short Y;
    public short Z;
    public short W;

    public PbrBoneIndices(short x, short y, short z, short w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public short this[int index]
    {
      get
      {
        return index switch
        {
          0 => X,
          1 => Y,
          2 => Z,
          _ => W
        };
      }
    }
  }

  /// <summary>
  /// PBR 网格顶点. 骨骼索引/权重两路数据对静态网格恒为零, 同一套顶点声明配 Static / Skinned 两种技术.
  /// <br>Tangent.w 存副切线手性 (cross(N, T) * sign), 用于法线贴图.</br>
  /// </summary>
  public struct PbrVertex : IVertexType
  {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector4 Tangent;
    public Vector2 TexCoord;
    public PbrBoneIndices BlendIndices;
    public Vector4 BlendWeights;

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0),
        new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(48, VertexElementFormat.Short4, VertexElementUsage.BlendIndices, 0),
        new VertexElement(56, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));

    public PbrVertex(Vector3 position, Vector3 normal, Vector4 tangent, Vector2 texCoord)
    {
      Position = position;
      Normal = normal;
      Tangent = tangent;
      TexCoord = texCoord;
      BlendIndices = default;
      BlendWeights = Vector4.Zero;
    }

    public PbrVertex(
        Vector3 position,
        Vector3 normal,
        Vector4 tangent,
        Vector2 texCoord,
        PbrBoneIndices blendIndices,
        Vector4 blendWeights)
    {
      Position = position;
      Normal = normal;
      Tangent = tangent;
      TexCoord = texCoord;
      BlendIndices = blendIndices;
      BlendWeights = blendWeights;
    }

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
  }
}
