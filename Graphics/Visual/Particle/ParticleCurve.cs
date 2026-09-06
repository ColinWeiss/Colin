using System.Text.Json.Serialization;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>曲线插值模式 (控制点曲线: 关键帧即控制点).</summary>
  public enum CurveInterpolation
  {
    /// <summary>线性折线.</summary>
    Linear,
    /// <summary>线性 + SmoothStep 缓动.</summary>
    SmoothStep,
    /// <summary>Catmull-Rom 样条 —— 过控制点的平滑曲线 (默认, 推荐塑形用).</summary>
    CatmullRom
  }

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
  /// 可序列化的控制点曲线: 关键帧即控制点, 默认 Catmull-Rom 平滑插值 (曲线过所有控制点).
  /// <br>编辑后版本号自增, GPU 缓冲区据此增量重建; 深拷贝与序列化友好.</br>
  /// </summary>
  [Serializable]
  public class FloatCurve
  {
    /// <summary>控制点列表 (Time 需递增).</summary>
    public List<CurveKey> Keys = new List<CurveKey>();

    /// <summary>插值模式.</summary>
    public CurveInterpolation Interpolation = CurveInterpolation.CatmullRom;

    /// <summary>[兼容旧配置] 旧的平滑开关 (true 视为 SmoothStep).</summary>
    [Obsolete]
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
      if (t >= keys[keys.Count - 1].Time)
        return keys[keys.Count - 1].Value;

      int k = FindSegment(keys, t);
      CurveKey a = keys[k], b = keys[k + 1];
      float span = b.Time - a.Time;
      float f = span > 1e-6f ? (t - a.Time) / span : 1f;

      switch (Interpolation)
      {
        case CurveInterpolation.SmoothStep:
          f = f * f * (3f - 2f * f);
          return a.Value + (b.Value - a.Value) * f;
        case CurveInterpolation.CatmullRom:
          float p0 = keys[Math.Max(0, k - 1)].Value;
          float p3 = keys[Math.Min(keys.Count - 1, k + 2)].Value;
          return CurveMath.CatmullRom(p0, a.Value, b.Value, p3, f);
        default:
          return a.Value + (b.Value - a.Value) * f;
      }
    }

    /// <summary>追加控制点并保持 Time 递增.</summary>
    public void AddKey(float time, float value)
    {
      Keys.Add(new CurveKey(time, value));
      Keys.Sort((x, y) => x.Time.CompareTo(y.Time));
      Version++;
    }

    /// <summary>移除指定序号的控制点.</summary>
    public void RemoveKeyAt(int index)
    {
      if (index >= 0 && index < Keys.Count)
      {
        Keys.RemoveAt(index);
        Version++;
      }
    }

    /// <summary>移动控制点 (时间钳制在相邻控制点之间, 保持递增 —— 拖拽安全).</summary>
    public void MoveKey(int index, float time, float value)
    {
      if (index < 0 || index >= Keys.Count)
        return;
      float min = index > 0 ? Keys[index - 1].Time : 0f;
      float max = index < Keys.Count - 1 ? Keys[index + 1].Time : 1f;
      Keys[index] = new CurveKey(Math.Clamp(time, min, max), value);
      Version++;
    }

    /// <summary>深拷贝.</summary>
    public FloatCurve Clone() => new FloatCurve
    {
      Keys = Keys?.Select(k => k).ToList() ?? new List<CurveKey>(),
      Interpolation = Interpolation
    };

    /// <summary>从另一条曲线恢复内容 (撤销/重做用, 保留自身引用与订阅).</summary>
    public void RestoreFrom(FloatCurve source)
    {
      Keys = source.Keys?.Select(k => k).ToList() ?? new List<CurveKey>();
      Interpolation = source.Interpolation;
      Version++;
    }

    /// <summary>定位 t 所在的段索引 (keys[i].Time <= t < keys[i+1].Time).</summary>
    public static int FindSegment(List<CurveKey> keys, float t)
    {
      for (int i = 0; i < keys.Count - 1; i++)
        if (t < keys[i + 1].Time)
          return i;
      return keys.Count - 2;
    }
  }

  /// <summary>
  /// 可序列化的颜色控制点曲线 (rgb, 默认 Catmull-Rom 平滑插值).
  /// </summary>
  [Serializable]
  public class ColorCurve
  {
    /// <summary>控制点列表 (Time 需递增).</summary>
    public List<ColorKey> Keys = new List<ColorKey>();

    /// <summary>插值模式.</summary>
    public CurveInterpolation Interpolation = CurveInterpolation.CatmullRom;

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
      if (t >= keys[keys.Count - 1].Time)
        return keys[keys.Count - 1].Color;

      int k = keys.FindSegment(t);
      ColorKey a = keys[k], b = keys[k + 1];
      float span = b.Time - a.Time;
      float f = span > 1e-6f ? (t - a.Time) / span : 1f;

      switch (Interpolation)
      {
        case CurveInterpolation.SmoothStep:
          f = f * f * (3f - 2f * f);
          return a.Color + (b.Color - a.Color) * f;
        case CurveInterpolation.CatmullRom:
          Vector4 p0 = keys[Math.Max(0, k - 1)].Color;
          Vector4 p3 = keys[Math.Min(keys.Count - 1, k + 2)].Color;
          float cr = CurveMath.CatmullRom(p0.X, a.Color.X, b.Color.X, p3.X, f);
          float cg = CurveMath.CatmullRom(p0.Y, a.Color.Y, b.Color.Y, p3.Y, f);
          float cb = CurveMath.CatmullRom(p0.Z, a.Color.Z, b.Color.Z, p3.Z, f);
          return new Vector4(cr, cg, cb, 1f);
        default:
          return a.Color + (b.Color - a.Color) * f;
      }
    }

    /// <summary>追加控制点并保持 Time 递增.</summary>
    public void AddKey(float time, Vector4 color)
    {
      Keys.Add(new ColorKey(time, color));
      Keys.Sort((x, y) => x.Time.CompareTo(y.Time));
      Version++;
    }

    /// <summary>移除指定序号的控制点.</summary>
    public void RemoveKeyAt(int index)
    {
      if (index >= 0 && index < Keys.Count)
      {
        Keys.RemoveAt(index);
        Version++;
      }
    }

    /// <summary>移动控制点 (时间钳制在相邻控制点之间, 保持递增).</summary>
    public void MoveKey(int index, float time, Vector4 color)
    {
      if (index < 0 || index >= Keys.Count)
        return;
      float min = index > 0 ? Keys[index - 1].Time : 0f;
      float max = index < Keys.Count - 1 ? Keys[index + 1].Time : 1f;
      Keys[index] = new ColorKey(Math.Clamp(time, min, max), color);
      Version++;
    }

    /// <summary>深拷贝.</summary>
    public ColorCurve Clone() => new ColorCurve
    {
      Keys = Keys?.Select(k => k).ToList() ?? new List<ColorKey>(),
      Interpolation = Interpolation
    };

    /// <summary>从另一条曲线恢复内容 (撤销/重做用).</summary>
    public void RestoreFrom(ColorCurve source)
    {
      Keys = source.Keys?.Select(k => k).ToList() ?? new List<ColorKey>();
      Interpolation = source.Interpolation;
      Version++;
    }
  }

  /// <summary>曲线静态工具 (CPU/GPU 求值公式同源).</summary>
  public static class CurveMath
  {
    /// <summary>Catmull-Rom 样条 (过控制点的三次插值).</summary>
    public static float CatmullRom(float p0, float p1, float p2, float p3, float f)
    {
      float f2 = f * f;
      float f3 = f2 * f;
      return 0.5f * ((2f * p1)
        + (-p0 + p2) * f
        + (2f * p0 - 5f * p1 + 4f * p2 - p3) * f2
        + (-p0 + 3f * p1 - 3f * p2 + p3) * f3);
    }
  }

  /// <summary>FloatCurve/ColorCurve 共享的段查找扩展 (供 ColorCurve 复用).</summary>
  public static class CurveSegmentExtensions
  {
    /// <summary>定位 t 所在的控制点段索引.</summary>
    public static int FindSegment(this List<ColorKey> keys, float t)
    {
      for (int i = 0; i < keys.Count - 1; i++)
        if (t < keys[i + 1].Time)
          return i;
      return keys.Count - 2;
    }
  }
}
