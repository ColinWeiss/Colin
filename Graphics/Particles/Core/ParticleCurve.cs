using System.Text.Json.Serialization;

namespace Particle.Core
{
  /// <summary>标量曲线关键帧.</summary>
  [Serializable]
  public struct CurveKey
  {
    /// <summary>关键帧时间点 (0~1, 相对生命周期或效果时长).</summary>
    public float Time;
    /// <summary>关键帧数值.</summary>
    public float Value;

    public CurveKey(float time, float value)
    {
      Time = time;
      Value = value;
    }
  }

  /// <summary>颜色曲线关键帧 (rgb, a 由透明度曲线独立控制).</summary>
  [Serializable]
  public struct ColorKey
  {
    /// <summary>关键帧时间点 (0~1).</summary>
    public float Time;
    /// <summary>关键帧颜色 (rgb, 分量 0~1).</summary>
    public Vector4 Color;

    public ColorKey(float time, Vector4 color)
    {
      Time = time;
      Color = color;
    }
  }

  /// <summary>
  /// 可序列化的标量关键帧曲线 (线性插值, 可选平滑过渡).
  /// <br>深拷贝与序列化友好; 每次编辑后版本号自增, GPU 缓冲区据此增量重建.</br>
  /// </summary>
  [Serializable]
  public class FloatCurve
  {
    /// <summary>关键帧列表 (Time 需递增).</summary>
    public List<CurveKey> Keys = new List<CurveKey>();

    /// <summary>是否对插值结果应用 SmoothStep 缓动.</summary>
    public bool Smooth;

    /// <summary>曲线编辑版本号 (编辑时自增).</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>默认构造: 恒定值 1 的曲线.</summary>
    public static FloatCurve Constant(float value = 1f) => new FloatCurve { Keys = { new CurveKey(0f, value), new CurveKey(1f, value) } };

    /// <summary>在时间点 t (0~1) 求值, 溢出区间时取端点值.</summary>
    public float Evaluate(float t)
    {
      List<CurveKey> keys = Keys;
      if (keys is null || keys.Count == 0)
        return 1f;
      if (keys.Count == 1 || t <= keys[0].Time)
        return keys[0].Value;
      for (int i = 0; i < keys.Count - 1; i++)
      {
        if (t <= keys[i + 1].Time)
        {
          CurveKey a = keys[i], b = keys[i + 1];
          float span = b.Time - a.Time;
          float f = span > 1e-6f ? (t - a.Time) / span : 1f;
          if (Smooth)
            f = f * f * (3f - 2f * f);
          return a.Value + (b.Value - a.Value) * f;
        }
      }
      return keys[keys.Count - 1].Value;
    }

    /// <summary>追加关键帧并保持 Time 递增.</summary>
    public void AddKey(float time, float value)
    {
      Keys.Add(new CurveKey(time, value));
      Keys.Sort((x, y) => x.Time.CompareTo(y.Time));
      Version++;
    }

    /// <summary>移除指定序号的关键帧.</summary>
    public void RemoveKeyAt(int index)
    {
      if (index >= 0 && index < Keys.Count)
      {
        Keys.RemoveAt(index);
        Version++;
      }
    }

    /// <summary>深拷贝.</summary>
    public FloatCurve Clone() => new FloatCurve
    {
      Keys = Keys?.Select(k => k).ToList() ?? new List<CurveKey>(),
      Smooth = Smooth
    };

    /// <summary>从另一条曲线恢复内容 (撤销/重做用, 保留自身引用与订阅).</summary>
    public void RestoreFrom(FloatCurve source)
    {
      Keys = source.Keys?.Select(k => k).ToList() ?? new List<CurveKey>();
      Smooth = source.Smooth;
      Version++;
    }
  }

  /// <summary>
  /// 可序列化的颜色关键帧曲线 (rgb, 线性插值).
  /// </summary>
  [Serializable]
  public class ColorCurve
  {
    /// <summary>关键帧列表 (Time 需递增).</summary>
    public List<ColorKey> Keys = new List<ColorKey>();

    /// <summary>是否对插值结果应用 SmoothStep 缓动.</summary>
    public bool Smooth;

    /// <summary>曲线编辑版本号 (编辑时自增).</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>在时间点 t (0~1) 求值, 返回 rgb (alpha 恒 1, 由透明度曲线控制).</summary>
    public Vector4 Evaluate(float t)
    {
      List<ColorKey> keys = Keys;
      if (keys is null || keys.Count == 0)
        return Vector4.One;
      if (keys.Count == 1 || t <= keys[0].Time)
        return keys[0].Color;
      for (int i = 0; i < keys.Count - 1; i++)
      {
        if (t <= keys[i + 1].Time)
        {
          ColorKey a = keys[i], b = keys[i + 1];
          float span = b.Time - a.Time;
          float f = span > 1e-6f ? (t - a.Time) / span : 1f;
          if (Smooth)
            f = f * f * (3f - 2f * f);
          return a.Color + (b.Color - a.Color) * f;
        }
      }
      return keys[keys.Count - 1].Color;
    }

    /// <summary>追加关键帧并保持 Time 递增.</summary>
    public void AddKey(float time, Vector4 color)
    {
      Keys.Add(new ColorKey(time, color));
      Keys.Sort((x, y) => x.Time.CompareTo(y.Time));
      Version++;
    }

    /// <summary>移除指定序号的关键帧.</summary>
    public void RemoveKeyAt(int index)
    {
      if (index >= 0 && index < Keys.Count)
      {
        Keys.RemoveAt(index);
        Version++;
      }
    }

    /// <summary>深拷贝.</summary>
    public ColorCurve Clone() => new ColorCurve
    {
      Keys = Keys?.Select(k => k).ToList() ?? new List<ColorKey>(),
      Smooth = Smooth
    };

    /// <summary>从另一条曲线恢复内容 (撤销/重做用, 保留自身引用与订阅).</summary>
    public void RestoreFrom(ColorCurve source)
    {
      Keys = source.Keys?.Select(k => k).ToList() ?? new List<ColorKey>();
      Smooth = source.Smooth;
      Version++;
    }
  }
}
