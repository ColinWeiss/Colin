namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 一条节点动画通道. glTF 的平移/旋转/缩放是三条独立时间线, 这里分开存并各自插值.
  /// </summary>
  internal sealed class PbrAnimationChannel
  {
    public int NodeIndex;

    public float[] PositionTimes = Array.Empty<float>();
    public Vector3[] Positions = Array.Empty<Vector3>();

    public float[] RotationTimes = Array.Empty<float>();
    public Quaternion[] Rotations = Array.Empty<Quaternion>();

    public float[] ScaleTimes = Array.Empty<float>();
    public Vector3[] Scales = Array.Empty<Vector3>();

    public void Apply(PbrNode node, float time)
    {
      if (Positions.Length > 0)
        node.Translation = SampleVector3(PositionTimes, Positions, time);
      if (Rotations.Length > 0)
        node.Rotation = SampleQuaternion(RotationTimes, Rotations, time);
      if (Scales.Length > 0)
        node.Scale = SampleVector3(ScaleTimes, Scales, time);
      node.RebuildLocal();
    }

    private static int FindKey(float[] times, float time)
    {
      if (times.Length <= 1 || time <= times[0])
        return 0;
      if (time >= times[^1])
        return times.Length - 1;

      int low = 0;
      int high = times.Length - 1;
      while (low + 1 < high)
      {
        int middle = (low + high) >> 1;
        if (times[middle] <= time)
          low = middle;
        else
          high = middle;
      }
      return low;
    }

    private static Vector3 SampleVector3(float[] times, Vector3[] values, float time)
    {
      int index = FindKey(times, time);
      if (index >= times.Length - 1)
        return values[^1];
      float span = times[index + 1] - times[index];
      float t = span > 0f ? (time - times[index]) / span : 0f;
      return Vector3.Lerp(values[index], values[index + 1], t);
    }

    private static Quaternion SampleQuaternion(float[] times, Quaternion[] values, float time)
    {
      int index = FindKey(times, time);
      if (index >= times.Length - 1)
        return values[^1];
      float span = times[index + 1] - times[index];
      float t = span > 0f ? (time - times[index]) / span : 0f;
      return Quaternion.Slerp(values[index], values[index + 1], t);
    }
  }

  public sealed class PbrAnimationClip
  {
    public string Name = string.Empty;

    public float Duration;

    public int ChannelCount => _channels.Length;

    internal PbrAnimationChannel[] _channels = Array.Empty<PbrAnimationChannel>();

    public void Apply(PbrModel model, float time)
    {
      PbrNode[] nodes = model.NodeArray;
      if (nodes is null)
        return;
      for (int i = 0; i < _channels.Length; i++)
      {
        PbrAnimationChannel channel = _channels[i];
        if (channel.NodeIndex >= 0 && channel.NodeIndex < nodes.Length)
          channel.Apply(nodes[channel.NodeIndex], time);
      }
    }
  }

  public sealed class PbrAnimationPlayer
  {
    public PbrAnimationClip Clip { get; private set; }

    public float Time { get; private set; }

    public float Speed = 1f;

    public bool Loop = true;

    public bool IsPlaying = true;

    public int LoopCount { get; private set; }

    public PbrAnimationPlayer(PbrAnimationClip clip)
    {
      Clip = clip;
    }

    public void Reset()
    {
      Time = 0f;
      LoopCount = 0;
    }

    /// <summary>推进时间并返回是否有变化. 循环播放时按 Duration 取模.</summary>
    public bool Advance(float deltaSeconds)
    {
      if (IsPlaying is false || Clip is null || deltaSeconds <= 0f)
        return false;

      Time += deltaSeconds * Speed;
      float duration = Clip.Duration;
      if (duration <= 0f)
        return true;

      if (Time >= duration)
      {
        if (Loop)
        {
          int loops = (int)(Time / duration);
          Time -= duration * loops;
          LoopCount += loops;
        }
        else
        {
          Time = duration;
          IsPlaying = false;
        }
      }
      else if (Time < 0f)
      {
        Time = Loop ? Time % duration + duration : 0f;
      }
      return true;
    }

    public void Apply(PbrModel model)
    {
      Clip?.Apply(model, Time);
    }
  }
}
