using Colin.Core.Graphics.Visual.Curve;
using System.Text.Json;
// XNA 自带 Curve/CurveKey (Microsoft.Xna.Framework), 与 VFX 曲线的 CurveKey 重名, 统一钉到本工程这份.
using CurveKey = Colin.Core.Graphics.Visual.Curve.CurveKey;

namespace Colin.Core.Common
{
  /// <summary>
  /// 场景渲染模块的调色曲线集 —— PS 曲线通道: 亮度 / 饱和度 / R / G / B.
  /// <br>亮度与颜色通道是重映射曲线 (y=x 为默认), 饱和度是以亮度为输入的乘数曲线 (恒 1 为默认);</br>
  /// <br>编辑时各曲线的 <see cref="FloatCurve.Version"/> 自增, LUT 按需重烘焙, <see cref="RSModuleCurve.GetEffect"/> 据此把特效挂进呈现管线.</br>
  /// </summary>
  public sealed class RSModuleCurveSet
  {
    /// <summary>通道数: 亮度 / 饱和度 / R / G / B.</summary>
    public const int ChannelCount = 5;

    /// <summary>通道显示名 (索引即通道序号).</summary>
    public static readonly string[] ChannelNames = { "亮度", "饱和度", "R", "G", "B" };

    /// <summary>LUT 采样位数 (横轴 256 档, 两行: 第 0 行 = R/G/B 重映射 + 饱和度乘数, 第 1 行 = 亮度重映射).</summary>
    public const int LutBins = 256;

    public bool Enabled = true;

    /// <summary>亮度重映射曲线: 输入像素亮度, 输出目标亮度 (乘回颜色的比例由着色器换算).</summary>
    public FloatCurve Brightness;

    /// <summary>饱和度乘数曲线: 输入像素亮度, 输出饱和度乘数 (0 = 灰度, 1 = 不变, 可超 1 提饱和).</summary>
    public FloatCurve Saturation;

    public FloatCurve R;
    public FloatCurve G;
    public FloatCurve B;

    /// <summary>指示本曲线集是否曾从磁盘文件载入.</summary>
    public bool LoadedFromFile { get; private set; }

    public RSModuleCurveSet()
    {
      Brightness = FloatCurve.Linear();
      Saturation = FloatCurve.Constant(1f);
      R = FloatCurve.Linear();
      G = FloatCurve.Linear();
      B = FloatCurve.Linear();
      MarkSaved();
    }

    /// <summary>取指定通道的曲线.</summary>
    public FloatCurve GetCurve(int channel) => channel switch
    {
      0 => Brightness,
      1 => Saturation,
      2 => R,
      3 => G,
      _ => B
    };

    /// <summary>替换指定通道的曲线 (UI 重置用).</summary>
    public void SetCurve(int channel, FloatCurve curve)
    {
      switch (channel)
      {
        case 0: Brightness = curve; break;
        case 1: Saturation = curve; break;
        case 2: R = curve; break;
        case 3: G = curve; break;
        default: B = curve; break;
      }
    }

    /// <summary>把指定通道恢复为默认形状 (重映射通道 y=x, 饱和度恒 1).</summary>
    public void ResetChannel(int channel) => SetCurve(channel, DefaultCurve(channel));

    /// <summary>全部通道恢复默认并重新启用.</summary>
    public void ResetAll()
    {
      Enabled = true;
      for (int channel = 0; channel < ChannelCount; channel++)
        ResetChannel(channel);
    }

    /// <summary>通道默认曲线: 饱和度恒 1, 其余为 y=x 重映射.</summary>
    public static FloatCurve DefaultCurve(int channel) => channel == 1 ? FloatCurve.Constant(1f) : FloatCurve.Linear();

    /// <summary>通道画布纵轴范围: 重映射通道 [-0.5, 1.5] (允许压黑提亮超出), 饱和度 [0, 2].</summary>
    public static void ChannelRange(int channel, out float min, out float max)
    {
      if (channel == 1) { min = 0f; max = 2f; }
      else { min = -0.5f; max = 1.5f; }
    }

    /// <summary>曲线集是否等价于不调色 (全部通道为默认形状); 呈现管线据此跳过整条特效链路.</summary>
    public bool IsIdentity
    {
      get
      {
        int key = VersionKey;
        if (key != _identityKey)
        {
          _identity = IsIdentityRemap(Brightness) && IsNeutralConstant(Saturation)
            && IsIdentityRemap(R) && IsIdentityRemap(G) && IsIdentityRemap(B);
          _identityKey = key;
        }
        return _identity;
      }
    }
    private int _identityKey = int.MinValue;
    private bool _identity;

    private static bool IsIdentityRemap(FloatCurve curve)
      => curve.Keys.Count == 2
      && curve.Keys[0].Time == 0f && curve.Keys[0].Value == 0f
      && curve.Keys[1].Time == 1f && curve.Keys[1].Value == 1f;

    private static bool IsNeutralConstant(FloatCurve curve)
      => curve.Keys.Count == 2
      && curve.Keys[0].Time == 0f && curve.Keys[0].Value == 1f
      && curve.Keys[1].Time == 1f && curve.Keys[1].Value == 1f;

    private int VersionKey => Brightness.Version + Saturation.Version + R.Version + G.Version + B.Version;

    // —— LUT 烘焙 ——

    private Texture2D _lut;
    private int _bakedKey = -1;

    /// <summary>烘焙产物 (256x2, Vector4); 各曲线编辑后首次取用时重烘焙.</summary>
    public Texture2D Lut
    {
      get
      {
        int key = VersionKey;
        if (_lut is null)
        {
          _lut = new Texture2D(CoreInfo.Graphics.GraphicsDevice, LutBins, 2, false, SurfaceFormat.Vector4);
          Bake(key);
        }
        else if (key != _bakedKey)
        {
          Bake(key);
        }
        return _lut;
      }
    }

    private void Bake(int key)
    {
      Vector4[] data = new Vector4[LutBins * 2];
      for (int i = 0; i < LutBins; i++)
      {
        float value = i / (float)(LutBins - 1);
        data[i] = new Vector4(
          Math.Clamp(R.Evaluate(value), -1f, 3f),
          Math.Clamp(G.Evaluate(value), -1f, 3f),
          Math.Clamp(B.Evaluate(value), -1f, 3f),
          Math.Clamp(Saturation.Evaluate(value), 0f, 2f));
        data[LutBins + i] = new Vector4(Math.Clamp(Brightness.Evaluate(value), -1f, 3f), 0f, 0f, 1f);
      }
      _lut.SetData(data);
      _bakedKey = key;
    }

    // —— 未保存标记 (调试面板提示用) ——

    private int _savedKey;

    /// <summary>指示自上次保存/载入后曲线是否又被编辑过.</summary>
    public bool IsDirty => VersionKey != _savedKey;

    /// <summary>把当前编辑状态标记为已保存.</summary>
    public void MarkSaved() => _savedKey = VersionKey;

    // —— 序列化 ——

    private sealed class CurveSetDto
    {
      public bool Enabled;
      public FloatCurve Brightness;
      public FloatCurve Saturation;
      public FloatCurve R;
      public FloatCurve G;
      public FloatCurve B;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      WriteIndented = true,
      IncludeFields = true
    };

    /// <summary>通道非法值统一收口: 排序控制点, 首尾钉死在 0 / 1, 中间点钳进 0~1.</summary>
    private static FloatCurve Sanitize(FloatCurve curve, bool multiplier)
    {
      curve.Keys ??= new List<CurveKey>();
      if (curve.Keys.Count == 0)
        return multiplier ? FloatCurve.Constant(1f) : FloatCurve.Linear();
      curve.Keys.Sort((a, b) => a.Time.CompareTo(b.Time));
      for (int i = 0; i < curve.Keys.Count; i++)
      {
        CurveKey key = curve.Keys[i];
        float time = i == 0 ? 0f : i == curve.Keys.Count - 1 ? 1f : Math.Clamp(key.Time, 0f, 1f);
        curve.Keys[i] = new CurveKey(time, key.Value);
      }
      curve.Interpolation = Enum.IsDefined(curve.Interpolation) ? curve.Interpolation : CurveInterpolation.CatmullRom;
      return curve;
    }

    /// <summary>从文件载入; 文件缺失返回默认曲线集, 内容异常报警告并回退默认.</summary>
    public static RSModuleCurveSet Load(Type moduleType)
    {
      RSModuleCurveSet set = new RSModuleCurveSet();
      string file = RSModuleCurve.FileOf(moduleType);
      if (File.Exists(file) is false)
        return set;
      try
      {
        CurveSetDto dto = JsonSerializer.Deserialize<CurveSetDto>(File.ReadAllText(file), JsonOptions);
        if (dto is not null)
        {
          set.Enabled = dto.Enabled;
          set.Brightness = Sanitize(dto.Brightness ?? FloatCurve.Linear(), false);
          set.Saturation = Sanitize(dto.Saturation ?? FloatCurve.Constant(1f), true);
          set.R = Sanitize(dto.R ?? FloatCurve.Linear(), false);
          set.G = Sanitize(dto.G ?? FloatCurve.Linear(), false);
          set.B = Sanitize(dto.B ?? FloatCurve.Linear(), false);
          set.LoadedFromFile = true;
        }
      }
      catch (Exception ex)
      {
        Console.Log(ConsoleTextType.Warning, "RSModuleCurve",
          string.Concat("曲线文件载入失败 '", file, "', 使用默认曲线: ", ex.Message));
        set = new RSModuleCurveSet();
      }
      set.MarkSaved();
      return set;
    }

    /// <summary>保存为以模块类名命名的 json 文件.</summary>
    public void Save(Type moduleType)
    {
      string file = RSModuleCurve.FileOf(moduleType);
      Directory.CreateDirectory(RSModuleCurve.DirectoryPath);
      CurveSetDto dto = new CurveSetDto
      {
        Enabled = Enabled,
        Brightness = Brightness,
        Saturation = Saturation,
        R = R,
        G = G,
        B = B
      };
      File.WriteAllText(file, JsonSerializer.Serialize(dto, JsonOptions));
      MarkSaved();
      LoadedFromFile = true;
      Console.Log(ConsoleTextType.Normal, "RSModuleCurve", string.Concat("已保存曲线: ", file));
    }
  }

  /// <summary>
  /// 渲染模块调色曲线的管理器: 以 <see cref="IRenderableISceneModule"/> 类型为键缓存曲线集,
  /// 文件存于资产根的 <c>Visual/RSModuleCurve</c> 下, 以模块类名命名, 游戏运行时按需读取.
  /// <br><see cref="GetEffect"/> 返回 null 表示该模块走原样呈现 (无文件或曲线为默认形状, 零开销直通).</br>
  /// </summary>
  public static class RSModuleCurve
  {
    private static readonly Dictionary<Type, RSModuleCurveSet> Sets = new Dictionary<Type, RSModuleCurveSet>();

    private static Effect _effect;
    private static EffectParameter _lutParam;
    private static bool _effectBroken;

    /// <summary>
    /// 曲线文件目录 (游戏根目录 / Assets / Visual / RSModuleCurve).
    /// <br>固定落在 exe 侧 Assets, 与资产加载根 (Debug 构建指向开发目录 DeltaMachine.Assets) 解耦 ——
    /// 打包部署读写自己的 Assets, 开发运行落在 bin 的 Assets, 都不写进资产源目录; 打包时开发期调好的曲线随包带走.</br>
    /// </summary>
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "Assets", "Visual", "RSModuleCurve");

    /// <summary>模块类型对应的曲线文件路径.</summary>
    public static string FileOf(Type moduleType) => Path.Combine(DirectoryPath, moduleType.Name + ".json");

    /// <summary>指示该模块是否已有曲线文件.</summary>
    public static bool HasFile(Type moduleType) => File.Exists(FileOf(moduleType));

    /// <summary>取模块的曲线集 (无文件时返回默认形状并缓存).</summary>
    public static RSModuleCurveSet GetOrLoad(Type moduleType)
    {
      if (Sets.TryGetValue(moduleType, out RSModuleCurveSet set))
        return set;
      set = RSModuleCurveSet.Load(moduleType);
      Sets[moduleType] = set;
      return set;
    }

    /// <summary>丢弃缓存并从磁盘重新读取.</summary>
    public static RSModuleCurveSet Reload(Type moduleType)
    {
      Sets.Remove(moduleType);
      return GetOrLoad(moduleType);
    }

    /// <summary>删除曲线文件并把缓存还原为默认.</summary>
    public static void DeleteFile(Type moduleType)
    {
      string file = FileOf(moduleType);
      if (File.Exists(file))
      {
        File.Delete(file);
        Console.Log(ConsoleTextType.Normal, "RSModuleCurve", string.Concat("已删除曲线文件: ", file));
      }
      Sets.Remove(moduleType);
    }

    private static readonly Dictionary<Type, bool> BindStates = new Dictionary<Type, bool>();

    /// <summary>
    /// 生效判定 + 状态翻转日志 (生效/旁路只在一进一出时各记一条, 方便当场看出曲线挂在哪一环).
    /// </summary>
    private static bool ShouldBind(RSModuleCurveSet set, Type type)
    {
      bool bind = set.Enabled && set.IsIdentity is false;
      if (BindStates.TryGetValue(type, out bool last) is false || last != bind)
      {
        BindStates[type] = bind;
        Console.Log(ConsoleTextType.Remind, "RSModuleCurve", string.Concat(
          bind ? "曲线生效: " : "曲线旁路: ", type.Name,
          bind ? string.Empty : set.Enabled ? " (默认形状)" : " (已停用)"));
      }
      return bind;
    }

    /// <summary>
    /// 取模块的调色特效; 返回 null 表示直通 (未启用 / 默认形状 / 特效不可用).
    /// <br>调用方把返回值传进 <c>SpriteBatch.Begin(effect:)</c> 即可, 采样图元仍是模块的 <c>RawRt</c>.</br>
    /// </summary>
    public static Effect GetEffect(IRenderableISceneModule module)
    {
      RSModuleCurveSet set = GetOrLoad(module.GetType());
      if (ShouldBind(set, module.GetType()) is false)
        return null;
      Effect effect = AcquireEffect();
      if (effect is null)
        return null;
      _lutParam?.SetValue(set.Lut);
      return effect;
    }

    /// <summary>
    /// 把模块的调色 LUT 绑到指定特效的 <c>curveLut</c> 参数上 (供想把曲线合并进自家特效的调用方, 如 ToneMapping).
    /// <br>返回 true 表示绑定了有效曲线, 调用方应启用特效里的曲线分支; false 表示无曲线, 走原路径.</br>
    /// </summary>
    public static bool BindLut(Effect effect, IRenderableISceneModule module)
    {
      RSModuleCurveSet set = GetOrLoad(module.GetType());
      if (ShouldBind(set, module.GetType()) is false)
        return false;
      EffectParameter param = effect.Parameters["curveLut"];
      if (param is null)
        return false;
      param.SetValue(set.Lut);
      return true;
    }

    private static Effect AcquireEffect()
    {
      if (_effect is not null || _effectBroken)
        return _effect;
      try
      {
        _effect = Assets.Effect("Effects/RSModuleCurve.fx");
        _lutParam = _effect.Parameters["curveLut"];
      }
      catch (Exception ex)
      {
        _effectBroken = true;
        Console.Log(ConsoleTextType.Error, "RSModuleCurve",
          string.Concat("调色特效编译失败, 曲线调整停用: ", ex.Message));
      }
      return _effect;
    }
  }
}
