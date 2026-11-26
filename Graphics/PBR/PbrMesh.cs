namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 一个可绘制的 GPU 网格 (顶点/索引缓冲 + 材质 + 蒙皮调色板).
  /// <br>蒙皮调色板 = 骨骼偏移矩阵 * 骨骼节点世界矩阵 (行向量约定, 与 Assimp 的列向量式 世界 * 偏移 互为转置);
  /// 打包进一张 float4 纹理, 每根骨骼占一列 4 个 texel (4 行 = 行向量矩阵的 4 行), 顶点着色器按骨骼下标取样,
  /// 不占常量寄存器上限, 骨骼数不受 128 之类的限制.</br>
  /// </summary>
  public sealed class PbrMesh : IDisposable
  {
    private readonly Vector4[] _paletteTexels;

    public string Name = string.Empty;

    public PbrMaterial Material;

    public VertexBuffer VertexBuffer { get; private set; }

    public IndexBuffer IndexBuffer { get; private set; }

    public int VertexCount { get; private set; }

    public int PrimitiveCount { get; private set; }

    /// <summary>每个骨骼槽的绑定姿势逆矩阵 (网格空间 -> 骨骼空间), XNA 行向量约定.</summary>
    public Matrix[] BoneOffsets { get; private set; } = Array.Empty<Matrix>();

    /// <summary>每个骨骼槽对应的场景节点下标; -1 表示找不到节点.</summary>
    public int[] BoneNodeIndices { get; private set; } = Array.Empty<int>();

    public Matrix[] BoneMatrices { get; private set; } = Array.Empty<Matrix>();

    public Texture2D BonePaletteTexture { get; private set; }

    public bool IsSkinned => BoneOffsets.Length > 0;

    public int BoneCount => BoneOffsets.Length;

    public BoundingBox BindBounds = new BoundingBox(Vector3.Zero, Vector3.Zero);

    public PbrMesh(
        GraphicsDevice device,
        string name,
        PbrVertex[] vertices,
        int[] indices,
        PbrMaterial material,
        Matrix[] boneOffsets,
        int[] boneNodeIndices)
    {
      Name = name;
      Material = material;
      VertexCount = vertices.Length;
      PrimitiveCount = indices.Length / 3;

      VertexBuffer = new VertexBuffer(device, PbrVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
      VertexBuffer.SetData(vertices);
      IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
      IndexBuffer.SetData(indices);

      if (boneOffsets is not null && boneOffsets.Length > 0)
      {
        BoneOffsets = boneOffsets;
        BoneNodeIndices = boneNodeIndices ?? new int[boneOffsets.Length];
        BoneMatrices = new Matrix[boneOffsets.Length];
        for (int i = 0; i < BoneMatrices.Length; i++)
          BoneMatrices[i] = Matrix.Identity;
        _paletteTexels = new Vector4[BoneOffsets.Length * 4];
        BonePaletteTexture = new Texture2D(
            device,
            BoneOffsets.Length,
            4,
            false,
            SurfaceFormat.Vector4);
        UpdateBonePaletteInternal();
      }
    }

    /// <summary>按骨骼节点当前世界矩阵刷新调色板. 未蒙皮网格为无操作.</summary>
    public void UpdateBonePalette(PbrNode[] nodes)
    {
      if (IsSkinned is false)
        return;

      for (int i = 0; i < BoneOffsets.Length; i++)
      {
        int nodeIndex = BoneNodeIndices[i];
        BoneMatrices[i] = nodeIndex >= 0 && nodeIndex < nodes.Length
            ? Matrix.Multiply(BoneOffsets[i], nodes[nodeIndex].World)
            : Matrix.Identity;
      }
      UpdateBonePaletteInternal();
    }

    private void UpdateBonePaletteInternal()
    {
      int width = BoneOffsets.Length;
      for (int slot = 0; slot < width; slot++)
      {
        Matrix matrix = BoneMatrices[slot];
        _paletteTexels[(0 * width) + slot] = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
        _paletteTexels[(1 * width) + slot] = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
        _paletteTexels[(2 * width) + slot] = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
        _paletteTexels[(3 * width) + slot] = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
      }
      BonePaletteTexture?.SetData(_paletteTexels);
    }

    public void Dispose()
    {
      VertexBuffer?.Dispose();
      IndexBuffer?.Dispose();
      BonePaletteTexture?.Dispose();
      VertexBuffer = null;
      IndexBuffer = null;
      BonePaletteTexture = null;
    }
  }
}
