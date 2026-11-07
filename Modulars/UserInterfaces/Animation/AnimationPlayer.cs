namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>
  /// 动画播放器: 编辑器预览与运行时播放共用.
  /// <br>预览隔离契约 —— <see cref="Play"/> 时对将要被写的属性做编辑值快照, <see cref="Stop"/> 时原样写回,
  /// 播放期间的逐帧采样不会污染编辑态属性值.</br>
  /// </summary>
  public sealed class AnimationPlayer
  {
    public AnimationClip Clip;
    public float Time;
    public float Speed = 1f;
    /// <summary>起始延迟 (秒): 组合播放里做错峰/编排; 延迟期间不推进不采样, 目标保持编辑姿态.</summary>
    public float Delay;
    public bool IsPlaying;

    private bool _pingPongForward = true;
    private readonly Dictionary<(string TargetId, string Property), float[]> _snapshot = new Dictionary<(string, string), float[]>();
    private float _lastAppliedTime = float.MinValue;

    public void Play(UserInterface ui)
    {
      if (Clip is null)
        return;
      if (IsPlaying is false)
        Snapshot(ui);
      IsPlaying = true;
      //强制下一个 Tick 立即采样一次 (预览起播)
      _lastAppliedTime = float.MinValue;
    }

    public void Pause()
      => IsPlaying = false;

    public void Stop(UserInterface ui)
    {
      IsPlaying = false;
      Time = 0f;
      _pingPongForward = true;
      //回写闸门对齐归零时间: 下一 Tick 不再把恢复出的编辑值立刻又盖成 t=0 采样值
      _lastAppliedTime = 0f;
      Restore(ui);
    }

    /// <summary>
    /// 由 <see cref="UserInterface.DoUpdate"/> 末尾驱动 (挂载见 <see cref="UserInterface.AnimationPlayers"/>).
    /// <br>播放中推进时间; 回写仅在播放中或播放头挪动时执行 —— 暂停/停驻态逐帧回写会把用户在播放头处的
    /// 编辑 (gizmo 摆姿势) 当帧盖掉, 第二帧永远 K 不上; 播放头动一次采一次即 scrub 预览的本意.</br>
    /// </summary>
    public void Tick(UserInterface ui, float deltaTime)
    {
      if (Clip is null)
        return;
      if (Delay > 0f)
      {
        //延迟期: 只扣延迟不走时间不采样 —— 目标元素保持编辑姿态, 到点即起播
        if (IsPlaying)
          Delay = MathF.Max(0f, Delay - deltaTime);
        return;
      }
      if (IsPlaying)
      {
        //[!] 播放头按"方向"推进: 往返模式必须是 +step/-step, 而不是"只增然后再镜像一次" ——
        //    旧实现里 Time 只增, PingPong 会把 1.0 映成 0.0 再映成 0.9, 表现为两帧一跳的闪烁, 且永远回不到正向.
        float duration = MathF.Max(0f, Clip.Duration);
        float step = deltaTime * Speed;
        switch (Clip.Loop)
        {
          case ClipLoopMode.Once:
            Time += step;
            if (Time >= duration)
            {
              Time = duration;
              IsPlaying = false;
            }
            else if (Time < 0f)
              Time = 0f;
            break;
          case ClipLoopMode.Loop:
            Time += step;
            if (duration <= 0f)
              Time = 0f;
            else
            {
              Time %= duration;
              if (Time < 0f)
                Time += duration;
            }
            break;
          case ClipLoopMode.PingPong:
            Time += _pingPongForward ? step : -step;
            if (_pingPongForward)
            {
              if (Time >= duration)
              {
                Time = duration - (Time - duration);
                _pingPongForward = false;
              }
            }
            else if (Time <= 0f)
            {
              Time = -Time;
              _pingPongForward = true;
            }
            Time = Math.Clamp(Time, 0f, duration);
            break;
        }
      }
      if (IsPlaying || Time != _lastAppliedTime)
      {
        Apply(ui, Time);
        _lastAppliedTime = Time;
      }
    }

    /// <summary>逐帧步进 (帧率取自片段); 暂停态下同样生效, 供 scrub.</summary>
    public void StepFrames(UserInterface ui, int frames)
    {
      if (Clip is null)
        return;
      Time = Math.Clamp(Time + frames / MathF.Max(Clip.FrameRate, 1f), 0f, Clip.Duration);
      Apply(ui, Time);
      _lastAppliedTime = Time;
    }

    /// <summary>把片段在指定时间的采样值写到全部轨道的目标节点上.</summary>
    public void Apply(UserInterface ui, float time)
    {
      if (Clip is null || ui is null)
        return;
      for (int index = 0; index < Clip.Tracks.Count; index++)
      {
        AnimationTrack track = Clip.Tracks[index];
        Div div = ui.FindById(track.TargetId);
        if (div is null || div.Disposed)
          continue;
        float[] sampled = AnimationMath.SampleTrack(track, time);
        if (sampled is null)
          continue;
        DivAnimatable.Write(div, track.Property, sampled);
      }
    }

    private void Snapshot(UserInterface ui)
    {
      _snapshot.Clear();
      if (Clip is null)
        return;
      for (int index = 0; index < Clip.Tracks.Count; index++)
      {
        AnimationTrack track = Clip.Tracks[index];
        Div div = ui.FindById(track.TargetId);
        if (div is null || div.Disposed)
          continue;
        float[] current = DivAnimatable.Read(div, track.Property);
        if (current is not null)
          _snapshot[(track.TargetId, track.Property)] = current;
      }
    }

    private void Restore(UserInterface ui)
    {
      //ui 为空 (编辑器切换片段等无界面上下文的场景) 时跳过回写, 不能炸
      if (Clip is null || ui is null)
        return;
      foreach (KeyValuePair<(string TargetId, string Property), float[]> pair in _snapshot)
      {
        Div div = ui.FindById(pair.Key.TargetId);
        if (div is null || div.Disposed)
          continue;
        DivAnimatable.Write(div, pair.Key.Property, pair.Value);
      }
      _snapshot.Clear();
    }
  }
}
