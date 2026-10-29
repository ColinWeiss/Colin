namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>缓动预设库 (CSS 兼容命名的标准缓动族).</summary>
  public enum EasingPreset
  {
    Linear,
    InQuad, OutQuad, InOutQuad,
    InCubic, OutCubic, InOutCubic,
    InQuart, OutQuart, InOutQuart,
    InExpo, OutExpo, InOutExpo,
    InBack, OutBack, InOutBack,
    InElastic, OutElastic, InOutElastic,
  }

  /// <summary>
  /// 动画插值数学 (纯函数, 无状态): 缓动求值 / 三次贝塞尔求解 / 关键帧采样 / 时间吸附.
  /// <br>全部以 float 与索引工作, 供编辑器曲线视图与运行时播放共用.</br>
  /// </summary>
  public static class AnimationMath
  {
    /// <summary>按缓动预设求值: 输入线性进度 t (0~1), 输出缓动进度 (Elastic 可越出 0~1 产生回弹).</summary>
    public static float Ease(EasingPreset preset, float t)
    {
      t = Math.Clamp(t, 0f, 1f);
      return preset switch
      {
        EasingPreset.InQuad => t * t,
        EasingPreset.OutQuad => 1f - (1f - t) * (1f - t),
        EasingPreset.InOutQuad => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
        EasingPreset.InCubic => t * t * t,
        EasingPreset.OutCubic => 1f - MathF.Pow(1f - t, 3f),
        EasingPreset.InOutCubic => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
        EasingPreset.InQuart => t * t * t * t,
        EasingPreset.OutQuart => 1f - MathF.Pow(1f - t, 4f),
        EasingPreset.InOutQuart => t < 0.5f ? 8f * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f,
        EasingPreset.InExpo => t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f),
        EasingPreset.OutExpo => t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t),
        EasingPreset.InOutExpo => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ? MathF.Pow(2f, 20f * t - 10f) / 2f : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f,
        EasingPreset.InBack => 2.70158f * t * t * t - 1.70158f * t * t,
        EasingPreset.OutBack => 1f + 2.70158f * MathF.Pow(t - 1f, 3f) + 1.70158f * MathF.Pow(t - 1f, 2f),
        EasingPreset.InOutBack => t < 0.5f
          ? MathF.Pow(2f * t, 2f) * ((1.70158f * 1.525f + 1f) * 2f * t - 1.70158f * 1.525f) / 2f
          : (MathF.Pow(2f * t - 2f, 2f) * ((1.70158f * 1.525f + 1f) * (t * 2f - 2f) + 1.70158f * 1.525f) + 2f) / 2f,
        EasingPreset.InElastic => t == 0f ? 0f : t == 1f ? 1f : -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * (2f * MathF.PI / 3f)),
        EasingPreset.OutElastic => t == 0f ? 0f : t == 1f ? 1f : MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (2f * MathF.PI / 3f)) + 1f,
        EasingPreset.InOutElastic => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f
          ? -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI / 4.5f))) / 2f
          : MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI / 4.5f)) / 2f + 1f,
        _ => t,
      };
    }

    /// <summary>
    /// 三次贝塞尔缓动求解 (CSS timing-function 约定): 缓动曲线是 (0,0)-(x1,y1)-(x2,y2)-(1,1) 的三次贝塞尔,
    /// <br>但曲线按参数 u 求值 —— 时间进度 x 与缓动输出 y 都是 u 的函数, 给定 x 必须先反解 u.</br>
    /// <br>x(u) = 3(1-u)²u·x1 + 3(1-u)u²·x2 + u³, y(u) 同理. x 单调递增 (x1/x2 限定 0~1), 故 u 可反解:</br>
    /// <br>牛顿迭代 (导数解析式) 收敛极快, 8 次内精度足够; 导数过小 (拐点附近) 时退化为二分保证收敛.</br>
    /// </summary>
    public static float SolveBezier(float x1, float y1, float x2, float y2, float x)
    {
      if (x <= 0f)
        return 0f;
      if (x >= 1f)
        return 1f;
      float cx = 3f * x1;
      float bx = 3f * (x2 - x1) - cx;
      float ax = 1f - cx - bx;
      float cy = 3f * y1;
      float by = 3f * (y2 - y1) - cy;
      float ay = 1f - cy - by;
      float SampleX(float u) => ((ax * u + bx) * u + cx) * u;
      float SampleY(float u) => ((ay * u + by) * u + cy) * u;
      float SampleDX(float u) => (3f * ax * u + 2f * bx) * u + cx;
      float u = x;
      for (int iteration = 0; iteration < 8; iteration++)
      {
        float error = SampleX(u) - x;
        if (MathF.Abs(error) < 1e-6f)
          return SampleY(u);
        float derivative = SampleDX(u);
        if (MathF.Abs(derivative) < 1e-6f)
          break;
        u -= error / derivative;
      }
      //牛顿失败 (导数过小): 二分兜底, x 单调保证有根.
      float low = 0f, high = 1f;
      u = x;
      for (int iteration = 0; iteration < 24; iteration++)
      {
        float error = SampleX(u) - x;
        if (MathF.Abs(error) < 1e-5f)
          break;
        if (error > 0f)
          high = u;
        else
          low = u;
        u = (low + high) / 2f;
      }
      return SampleY(u);
    }

    /// <summary>
    /// 定位时间所在的键段: 返回 i 使 keys[i].Time &lt;= time &lt; keys[i+1].Time.
    /// <br>-1 表示落在首键之前 (采样应夹到首键), -2 表示末键之后 (夹到末键).</br>
    /// </summary>
    public static int FindSegment(List<AnimationKey> keys, float time)
    {
      if (keys.Count == 0)
        return -1;
      if (time <= keys[0].Time)
        return -1;
      if (time >= keys[keys.Count - 1].Time)
        return -2;
      int low = 0, high = keys.Count - 1;
      while (low + 1 < high)
      {
        int mid = (low + high) / 2;
        if (keys[mid].Time <= time)
          low = mid;
        else
          high = mid;
      }
      return low;
    }

    /// <summary>
    /// 采样一条轨道: 返回轨道值类型的分量数组 (Float=1, Vector2=2, Color=4, Bool=1).
    /// <br>段内插值由前一键的插值类型决定 —— Linear 线性 / Step 保持前值 / Preset 走缓动预设 / Bezier 走归一化贝塞尔;</br>
    /// <br>Bool (离散值) 轨道无论声明为何一律按 Step 语义. 首前/末后夹取端点值.</br>
    /// </summary>
    public static float[] SampleTrack(AnimationTrack track, float time)
    {
      List<AnimationKey> keys = track.Keys;
      if (keys.Count == 0)
        return null;
      int segment = FindSegment(keys, time);
      if (segment == -1)
        return (float[])keys[0].Value.Clone();
      if (segment == -2)
        return (float[])keys[keys.Count - 1].Value.Clone();
      AnimationKey from = keys[segment];
      AnimationKey to = keys[segment + 1];
      float span = to.Time - from.Time;
      float t = span > 1e-6f ? (time - from.Time) / span : 1f;
      bool discrete = track.ValueType == TrackValueType.Bool;
      float eased;
      if (discrete || from.Interpolation == KeyInterpolation.Step)
        eased = 0f;
      else if (from.Interpolation == KeyInterpolation.Bezier)
        eased = SolveBezier(from.BezierX1, from.BezierY1, from.BezierX2, from.BezierY2, t);
      else if (from.Interpolation == KeyInterpolation.Preset)
        eased = Ease(from.Preset, t);
      else
        eased = t;
      float[] result = new float[from.Value.Length];
      for (int component = 0; component < result.Length; component++)
        result[component] = from.Value[component] + (to.Value[component] - from.Value[component]) * eased;
      return result;
    }

    /// <summary>吸附模式位: 整帧 / 整秒 / 邻近关键帧.</summary>
    [Flags]
    public enum SnapMode
    {
      None = 0,
      Frame = 1,
      Second = 2,
      Keys = 4,
    }

    /// <summary>
    /// 时间吸附: 在候补目标 (整帧网格 / 整秒网格 / 其他关键帧) 中找与 <paramref name="time"/> 距离小于
    /// <paramref name="threshold"/> (秒) 的最近者, 一律不超过该阈值 —— 不吸附时原样返回.
    /// </summary>
    public static float SnapTime(float time, float frameRate, SnapMode mode, List<float> candidates, float threshold)
    {
      float best = time;
      float bestDistance = threshold;
      if (mode.HasFlag(SnapMode.Frame) && frameRate > 0f)
      {
        float snapped = MathF.Round(time * frameRate) / frameRate;
        float distance = MathF.Abs(snapped - time);
        if (distance < bestDistance)
        {
          bestDistance = distance;
          best = snapped;
        }
      }
      if (mode.HasFlag(SnapMode.Second))
      {
        float snapped = MathF.Round(time);
        float distance = MathF.Abs(snapped - time);
        if (distance < bestDistance)
        {
          bestDistance = distance;
          best = snapped;
        }
      }
      if (mode.HasFlag(SnapMode.Keys) && candidates is not null)
      {
        for (int index = 0; index < candidates.Count; index++)
        {
          float distance = MathF.Abs(candidates[index] - time);
          if (distance < bestDistance)
          {
            bestDistance = distance;
            best = candidates[index];
          }
        }
      }
      return MathF.Max(0f, best);
    }
  }
}
