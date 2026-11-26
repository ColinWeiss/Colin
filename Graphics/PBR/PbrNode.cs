namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 场景节点: 只保留骨骼/网格动画需要的局部 TRS + 父子下标.
  /// <br>[!] 矩阵约定: Assimp 是列向量约定, XNA 是行向量约定, 导入时须转置; 世界变换为 local * parent.</br>
  /// </summary>
  public sealed class PbrNode
  {
    public string Name = string.Empty;

    public int Index;

    public int ParentIndex = -1;

    public readonly List<int> Children = new List<int>();

    public readonly List<int> MeshIndices = new List<int>();

    public Vector3 BindTranslation;

    public Quaternion BindRotation = Quaternion.Identity;

    public Vector3 BindScale = Vector3.One;

    public Vector3 Translation;

    public Quaternion Rotation = Quaternion.Identity;

    public Vector3 Scale = Vector3.One;

    public Matrix Local = Matrix.Identity;

    public Matrix World = Matrix.Identity;

    public void ResetToBindPose()
    {
      Translation = BindTranslation;
      Rotation = BindRotation;
      Scale = BindScale;
      RebuildLocal();
    }

    public void RebuildLocal()
    {
      Local = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation);
    }
  }
}
