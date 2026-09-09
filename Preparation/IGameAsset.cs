namespace Colin.Core.Preparation
{
  /// <summary>
  /// 标识游戏资产: 由 <see cref="Preparator"/> 在启动阶段反射实例化并调用 <see cref="LoadResource"/>,
  /// 供游戏项目声明自己的启动期加载逻辑 (具体文件读取经 Leemo.Assets 管线完成).
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
