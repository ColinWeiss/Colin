using System.Text.Json;
using System.Text.Json.Serialization;

namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>循环模式.</summary>
  public enum ClipLoopMode
  {
    /// <summary>播一次, 到尾停住.</summary>
    Once,
    /// <summary>到尾回环.</summary>
    Loop,
    /// <summary>到尾往返.</summary>
    PingPong,
  }

  /// <summary>键插值类型; 段内插值语义由段前键决定.</summary>
  public enum KeyInterpolation
  {
    Linear,
    Step,
    Bezier,
    Preset,
  }

  /// <summary>关键帧: 时间 + 分量值 + 插值描述 (贝塞尔控制点 / 缓动预设 / 步数).</summary>
  public sealed class AnimationKey
  {
    public float Time { get; set; }
    public float[] Value { get; set; } = new float[] { 0f };
    public KeyInterpolation Interpolation { get; set; } = KeyInterpolation.Linear;
    public EasingPreset Preset { get; set; } = EasingPreset.Linear;
    public int Steps { get; set; } = 1;
    public float BezierX1 { get; set; } = 0.25f;
    public float BezierY1 { get; set; } = 0.1f;
    public float BezierX2 { get; set; } = 0.75f;
    public float BezierY2 { get; set; } = 0.9f;

    public AnimationKey Clone()
      => new AnimationKey
      {
        Time = Time,
        Value = (float[])Value.Clone(),
        Interpolation = Interpolation,
        Preset = Preset,
        Steps = Steps,
        BezierX1 = BezierX1,
        BezierY1 = BezierY1,
        BezierX2 = BezierX2,
        BezierY2 = BezierY2,
      };
  }

  /// <summary>动画轨道: 以目标节点稳定 Id (Div.Id) + 属性路径 (如 "Layout.Left") 绑定, 与数组下标解耦.</summary>
  public sealed class AnimationTrack
  {
    public string TargetId { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public TrackValueType ValueType { get; set; } = TrackValueType.Float;
    public List<AnimationKey> Keys { get; set; } = new List<AnimationKey>();

    public void SortKeys()
      => Keys.Sort((left, right) => left.Time.CompareTo(right.Time));

    /// <summary>取播放头处的键 (容差 1e-4 秒); 不存在返回 null.</summary>
    public AnimationKey FindKeyAt(float time)
    {
      for (int index = 0; index < Keys.Count; index++)
      {
        if (MathF.Abs(Keys[index].Time - time) <= 1e-4f)
          return Keys[index];
      }
      return null;
    }
  }

  /// <summary>
  /// 动画片段: 时长 / 帧率 / 循环模式 / 轨道集合, 以 JSON 持久化 (System.Text.Json, 带版本号向后兼容).
  /// </summary>
  public sealed class AnimationClip
  {
    public const int FormatVersion = 1;

    public int Version { get; set; } = FormatVersion;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "NewClip";
    public float Duration { get; set; } = 5f;
    public float FrameRate { get; set; } = 60f;
    public ClipLoopMode Loop { get; set; } = ClipLoopMode.Loop;
    public List<AnimationTrack> Tracks { get; set; } = new List<AnimationTrack>();

    public AnimationTrack GetOrCreateTrack(string targetId, string property, TrackValueType valueType)
    {
      for (int index = 0; index < Tracks.Count; index++)
      {
        AnimationTrack track = Tracks[index];
        if (track.TargetId == targetId && track.Property == property)
          return track;
      }
      AnimationTrack created = new AnimationTrack { TargetId = targetId, Property = property, ValueType = valueType };
      Tracks.Add(created);
      return created;
    }

    public static AnimationClip FromJson(string json)
    {
      AnimationClip clip = JsonSerializer.Deserialize<AnimationClip>(json, AnimationJson.Options);
      if (clip is null)
        return null;
      if (clip.Version > FormatVersion)
        throw new InvalidDataException($"动画数据版本过新: {clip.Version} > {FormatVersion}");
      //旧版本数据兼容修补: 缺分量/键序错乱在此收敛.
      for (int trackIndex = 0; trackIndex < clip.Tracks.Count; trackIndex++)
      {
        AnimationTrack track = clip.Tracks[trackIndex];
        for (int keyIndex = 0; keyIndex < track.Keys.Count; keyIndex++)
        {
          AnimationKey key = track.Keys[keyIndex];
          if (key.Value is null || key.Value.Length == 0)
            key.Value = new float[] { 0f };
        }
        track.SortKeys();
      }
      return clip;
    }

    public string ToJson()
      => JsonSerializer.Serialize(this, AnimationJson.Options);
  }

  /// <summary>动画 JSON 序列化约定 (片段/组合共用): 缩进 + 枚举走字符串, 手写手改可读.</summary>
  internal static class AnimationJson
  {
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Converters = { new JsonStringEnumConverter() },
    };
  }
}
