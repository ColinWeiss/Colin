namespace Colin.Core.Assets
{
  /// <summary>
  /// 标识游戏资产.
  /// </summary>
  public interface IGameAsset
  {
    /// <summary>
    /// 指示该游戏资产对象类型的名称.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 指示当前加载进度.
    /// </summary>
    float Progress { get; set; }

    /// <summary>
    /// 加载资源.
    /// </summary>
    void LoadResource();
  }
}