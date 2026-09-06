using System.Text.Json.Serialization;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>突发发射事件: 效果进行到指定时间点时一次性喷发.</summary>
  [Serializable]
  public class BurstEvent
  {
    /// <summary>触发时间 (秒, 相对效果开始).</summary>
    public float Time;
    /// <summary>喷发数量.</summary>
    public int Count = 10;
    /// <summary>喷发时速度倍率 (用于强调爆发感).</summary>
    public float VelocityScale = 1f;

    public BurstEvent() { }

    public BurstEvent(float time, int count) { Time = time; Count = count; }

    /// <summary>深拷贝.</summary>
    public BurstEvent Clone() => new BurstEvent { Time = Time, Count = Count, VelocityScale = VelocityScale };
  }

  /// <summary>
  /// 单个发射器的完整可序列化配置.
  /// <br>包含发射时机 (速率曲线 + 突发事件)、形状、初速度、生命周期、
  /// 颜色/尺寸/透明度随生命变化的曲线、外力与渲染相关参数.</br>
  /// </summary>
  [Serializable]
  public class EmitterConfig
  {
    // ---- 发射时机 ----

    /// <summary>发射器名称 (编辑器中显示).</summary>
    public string Name = "发射器";
    /// <summary>发射器槽位上限 (同时存活的粒子数上限).</summary>
    public int Capacity = 512;
    /// <summary>持续发射速率 (个/秒); 配合 <see cref="RateCurve"/> 使用.</summary>
    public float EmissionRate = 60f;
    /// <summary>速率倍率曲线 (横轴为效果时间 0~1, 乘算在 <see cref="EmissionRate"/> 上).</summary>
    public FloatCurve RateCurve = FloatCurve.Constant(1f);
    /// <summary>突发事件列表 (按 Time 升序).</summary>
    public List<BurstEvent> Bursts = new List<BurstEvent>();
    /// <summary>发射器开始工作的延迟 (秒).</summary>
    public float StartTime = 0f;

    // ---- 形状与速度 ----

    /// <summary>发射形状.</summary>
    public EmissionShapeConfig Shape = new EmissionShapeConfig();
    /// <summary>初速度下限 (像素/秒).</summary>
    public float SpeedMin = 32f;
    /// <summary>初速度上限 (像素/秒).</summary>
    public float SpeedMax = 64f;
    /// <summary>初速度倍率曲线 (横轴为效果时间 0~1).</summary>
    public FloatCurve SpeedCurve = FloatCurve.Constant(1f);

    // ---- 生命周期 ----

    /// <summary>生命下限 (秒).</summary>
    public float LifeMin = 0.5f;
    /// <summary>生命上限 (秒).</summary>
    public float LifeMax = 1f;

    // ---- 外观 ----

    /// <summary>颜色随生命变化的曲线 (rgb).</summary>
    public ColorCurve ColorOverLife = new ColorCurve
    {
      Keys =
      {
        new ColorKey(0f, new Vector4(1f, 1f, 1f, 1f)),
        new ColorKey(1f, new Vector4(1f, 1f, 1f, 1f))
      }
    };
    /// <summary>透明度随生命变化的曲线.</summary>
    public FloatCurve OpacityOverLife = new FloatCurve
    {
      Keys = { new CurveKey(0f, 1f), new CurveKey(0.7f, 1f), new CurveKey(1f, 0f) }
    };
    /// <summary>尺寸随生命变化的倍率曲线 (乘算在基准尺寸上).</summary>
    public FloatCurve SizeOverLife = FloatCurve.Constant(1f);
    /// <summary>基准尺寸下限 (像素).</summary>
    public float SizeMin = 4f;
    /// <summary>基准尺寸上限 (像素).</summary>
    public float SizeMax = 8f;
    /// <summary>粒子长宽比 (高度/宽度; 拉伸公告牌模式下为最短厚度比).</summary>
    public float Aspect = 1f;
    /// <summary>初始旋转下限 (度).</summary>
    public float RotationMin = 0f;
    /// <summary>初始旋转上限 (度).</summary>
    public float RotationMax = 0f;
    /// <summary>角速度下限 (度/秒).</summary>
    public float AngularVelocityMin = 0f;
    /// <summary>角速度上限 (度/秒).</summary>
    public float AngularVelocityMax = 0f;
    /// <summary>色调随机系数下限 (乘算在颜色上, 制造明暗变化).</summary>
    public float TintMin = 1f;
    /// <summary>色调随机系数上限 (乘算在颜色上).</summary>
    public float TintMax = 1f;

    // ---- 物理 ----

    /// <summary>重力加速度 (像素/秒², +Y 向下).</summary>
    public Vector2 Gravity = Vector2.Zero;
    /// <summary>线性阻尼 (1/秒), 速度按 exp(-drag·dt) 衰减.</summary>
    public float Drag = 0f;

    /// <summary>是否启用速度拉伸 (刀光等高速条状效果必开).</summary>
    public bool StretchedBillboard = false;

    /// <summary>固定随机种子 (0 = 每次播放随机).</summary>
    public int Seed = 0;

    /// <summary>配置版本号 (任何修改自增, 驱动观察者与 GPU 资源增量重建).</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>标示配置已被修改 (观察者模式的通知入口).</summary>
    public event Action<EmitterConfig> Changed;

    /// <summary>触发配置变更通知.</summary>
    public void NotifyChanged()
    {
      Version++;
      Changed?.Invoke(this);
    }

    /// <summary>订阅配置变更 (观察者模式).</summary>
    public void Subscribe(Action<EmitterConfig> observer) => Changed += observer;

    /// <summary>取消订阅配置变更.</summary>
    public void Unsubscribe(Action<EmitterConfig> observer) => Changed -= observer;

    /// <summary>深拷贝整份发射器配置.</summary>
    public EmitterConfig Clone() => new EmitterConfig
    {
      Name = Name,
      Capacity = Capacity,
      EmissionRate = EmissionRate,
      RateCurve = RateCurve?.Clone() ?? FloatCurve.Constant(),
      Bursts = Bursts?.Select(b => b.Clone()).ToList() ?? new List<BurstEvent>(),
      StartTime = StartTime,
      Shape = Shape?.Clone() ?? new EmissionShapeConfig(),
      SpeedMin = SpeedMin,
      SpeedMax = SpeedMax,
      SpeedCurve = SpeedCurve?.Clone() ?? FloatCurve.Constant(),
      LifeMin = LifeMin,
      LifeMax = LifeMax,
      ColorOverLife = ColorOverLife?.Clone() ?? new ColorCurve(),
      OpacityOverLife = OpacityOverLife?.Clone() ?? new FloatCurve(),
      SizeOverLife = SizeOverLife?.Clone() ?? FloatCurve.Constant(),
      SizeMin = SizeMin,
      SizeMax = SizeMax,
      Aspect = Aspect,
      RotationMin = RotationMin,
      RotationMax = RotationMax,
      AngularVelocityMin = AngularVelocityMin,
      AngularVelocityMax = AngularVelocityMax,
      TintMin = TintMin,
      TintMax = TintMax,
      Gravity = Gravity,
      Drag = Drag,
      StretchedBillboard = StretchedBillboard,
      Seed = Seed
    };

    /// <summary>打包 GPU 曲线数据: [颜色rgb键][透明度键][尺寸键], 与 ComputeSharp 集成着色器的求值顺序一致.</summary>
    public void PackGpuCurves(List<Vector4> output)
    {
      output.Clear();
      foreach (ColorKey key in ColorOverLife.Keys)
        output.Add(new Vector4(key.Time, key.Color.X, key.Color.Y, key.Color.Z));
      foreach (CurveKey key in OpacityOverLife.Keys)
        output.Add(new Vector4(key.Time, key.Value, 0f, 0f));
      foreach (CurveKey key in SizeOverLife.Keys)
        output.Add(new Vector4(key.Time, key.Value, 0f, 0f));
    }

    /// <summary>颜色曲线关键帧数量.</summary>
    [JsonIgnore]
    public int ColorKeyCount => ColorOverLife?.Keys.Count ?? 0;
    /// <summary>透明度曲线关键帧数量.</summary>
    [JsonIgnore]
    public int AlphaKeyCount => OpacityOverLife?.Keys.Count ?? 0;
    /// <summary>尺寸曲线关键帧数量.</summary>
    [JsonIgnore]
    public int SizeKeyCount => SizeOverLife?.Keys.Count ?? 0;
  }
}
