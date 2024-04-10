namespace Colin.Core
{
  /// <summary>
  /// 提供帧时信息.
  /// </summary>
  public static class Time
  {
    /// <summary>
    ///游戏运行的总时间.
    /// </summary>
    public static float TotalTime;

    /// <summary>
    ///从上一帧到当前帧的时间增量, 按时间缩放.
    /// </summary>
    public static float DeltaTime;

    /// <summary>
    /// <see cref="DeltaTime"/> 的未缩放版本, 不受时间刻度的影响.
    /// </summary>
    public static float UnscaledDeltaTime;

    /// <summary>
    /// <see cref="DeltaTime"/> 的时间尺度.
    /// </summary>
    public static float TimeScale = 1f;

    /// <summary>
    ///已经过的帧数.
    /// </summary>
    public static uint FrameCount;

    internal static void Update(float dt)
    {
      TotalTime += dt;
      DeltaTime = dt * TimeScale;
      UnscaledDeltaTime = dt;
      FrameCount++;
    }
  }
}