using Colin.Core.Graphics.Visual.Curve;
using System.Text.Json;
// XNA 自带 Curve/CurveKey (Microsoft.Xna.Framework), 与 VFX 曲线的 CurveKey 重名, 统一钉到本工程这份.
using CurveKey = Colin.Core.Graphics.Visual.Curve.CurveKey;

namespace Colin.Core.Common
{
  /// <summary>
  /// 调色图层: 每个图层只控制 R/G/B/亮度/饱和度 其中一种通道, 按所在列表的顺序依次应用.
  /// <br>PS 式重映射语义: 横轴 = 原始颜色 0~255 (归一 0~1), 纵轴 = 输出颜色 0~1, 45° = 不变 (饱和度为恒 1 因子).</br>
  /// </summary>
  public sealed class RSModuleCurveLayer
  {
    /// <summary>通道: 0=亮度 1=饱和度 2=R 3=G 4=B.</summary>
    public int Channel;

    public FloatCurve Curve;

    private Texture2D _lut;
    private FloatCurve _bakedCurve;
    private int _bakedVersion = -1;
    private FloatCurve _identityCurve;
    private int _identityKey = int.MinValue;
    private bool _identity;

    public RSModuleCurveLayer() { }

    public RSModuleCurveLayer(int channel)
    {
      Channel = channel;
      Curve = RSModuleCurveSet.DefaultCurve(channel);
    }

    /// <summary>该图层是否为所在通道的恒等形状 (45°/恒 1) —— 应用链路据此跳过空图层.</summary>
    public bool IsIdentity
    {
      get
      {
        if (Curve is null)
          return true;
        // 引用 + 版本双比对: 重置图层会换上新的恒等实例 (Version 同为 0), 只看版本会漏检.
        if (!ReferenceEquals(_identityCurve, Curve) || _identityKey != Curve.Version)
        {
          _identity = RSModuleCurveSet.IsIdentityShape(Channel, Curve);
          _identityCurve = Curve;
          _identityKey = Curve.Version;
        }
        return _identity;
      }
    }

    /// <summary>该图层曲线的 LUT (256x1, r = 重映射输出); 曲线编辑或替换后首次取用时重烘焙.</summary>
    public Texture2D Lut
    {
      get
      {
        if (_lut is null || !ReferenceEquals(_bakedCurve, Curve) || _bakedVersion != Curve.Version)
        {
          if (_lut is null)
            _lut = new Texture2D(CoreInfo.Graphics.GraphicsDevice, RSModuleCurveSet.LutBins, 1, false, SurfaceFormat.Vector4);
          Bake();
        }
        return _lut;
      }
    }

    private void Bake()
    {
      Vector4[] data = new Vector4[RSModuleCurveSet.LutBins];
      for (int i = 0; i < RSModuleCurveSet.LutBins; i++)
      {
        float value = i / (float)(RSModuleCurveSet.LutBins - 1);
        // 重映射输出钳 0~1 (样条过冲在此收口), 输出恒在 0~255 内不反色.
        data[i] = new Vector4(Math.Clamp(Curve.Evaluate(value), 0f, 1f), 0f, 0f, 1f);
      }
      _lut.SetData(data);
      _bakedCurve = Curve;
      _bakedVersion = Curve.Version;
    }
  }

  /// <summary>
  /// 场景渲染模块的调色图层集 —— 图层按列表顺序依次应用, 每层只控制 R/G/B/亮度/饱和度 其中一种通道.
  /// <br>全部图层恒等 (或无图层) 时等价于不调色, 呈现管线据此跳过整条特效链路;</br>
  /// <br>图层增删/换序使 <see cref="StructureVersion"/> 自增, 与各层曲线 <see cref="FloatCurve.Version"/> 一起驱动恒等判定与 LUT 重烘焙.</br>
  /// </summary>
  public sealed class RSModuleCurveSet
  {
    /// <summary>通道数: 亮度 / 饱和度 / R / G / B.</summary>
    public const int ChannelCount = 5;

    /// <summary>通道显示名 (索引即通道序号).</summary>
    public static readonly string[] ChannelNames = { "亮度", "饱和度", "R", "G", "B" };

    /// <summary>每层 LUT 的采样位数 (横轴 256 档).</summary>
    public const int LutBins = 256;

    public bool Enabled = true;

    /// <summary>图层列表 (处理顺序 = 列表顺序, 自上而下).</summary>
    public List<RSModuleCurveLayer> Layers = new List<RSModuleCurveLayer>();

    /// <summary>图层结构版本号 (增删/换序自增).</summary>
    public int StructureVersion;

    /// <summary>指示本图层集是否曾从磁盘文件载入.</summary>
    public bool LoadedFromFile { get; private set; }

    public RSModuleCurveSet() => MarkSaved();

    /// <summary>新增一个控制指定通道的图层 (追加到末尾).</summary>
    public void AddLayer(int channel)
    {
      Layers.Add(new RSModuleCurveLayer(channel));
      StructureVersion++;
    }

    /// <summary>删除指定序号的图层.</summary>
    public void RemoveLayerAt(int index)
    {
      if (index < 0 || index >= Layers.Count)
        return;
      Layers.RemoveAt(index);
      StructureVersion++;
    }

    /// <summary>把 from 序号的图层移动到 to 序号 (自由调整处理顺序).</summary>
    public void MoveLayer(int from, int to)
    {
      from = Math.Clamp(from, 0, Layers.Count - 1);
      to = Math.Clamp(to, 0, Layers.Count - 1);
      if (Layers.Count == 0 || from == to)
        return;
      RSModuleCurveLayer layer = Layers[from];
      Layers.RemoveAt(from);
      Layers.Insert(to, layer);
      StructureVersion++;
    }

    /// <summary>清空全部图层并重新启用 (还原为不调色).</summary>
    public void ResetAll()
    {
      Enabled = true;
      Layers.Clear();
      StructureVersion++;
    }

    /// <summary>通道默认曲线: 重映射通道 (亮度/R/G/B) 为 45° 恒等线, 饱和度为恒 1 因子.</summary>
    public static FloatCurve DefaultCurve(int channel) => channel == 1 ? FloatCurve.Constant(1f) : FloatCurve.Linear();

    internal static bool IsIdentityShape(int channel, FloatCurve curve)
      => channel == 1 ? IsNeutralConstant(curve) : IsIdentityRemap(curve);

    private static bool IsIdentityRemap(FloatCurve curve)
      => curve.Keys.Count == 2
      && curve.Keys[0].Time == 0f && curve.Keys[0].Value == 0f
      && curve.Keys[1].Time == 1f && curve.Keys[1].Value == 1f;

    private static bool IsNeutralConstant(FloatCurve curve)
      => curve.Keys.Count == 2
      && curve.Keys[0].Time == 0f && curve.Keys[0].Value == 1f
      && curve.Keys[1].Time == 1f && curve.Keys[1].Value == 1f;

    /// <summary>图层集是否等价于不调色 (无图层或全部图层恒等).</summary>
    public bool IsIdentity
    {
      get
      {
        int key = VersionKey;
        if (key != _identityKey)
        {
          _identity = true;
          for (int i = 0; i < Layers.Count; i++)
            if (Layers[i].IsIdentity is false)
            {
              _identity = false;
              break;
            }
          _identityKey = key;
        }
        return _identity;
      }
    }
    private int _identityKey = int.MinValue;
    private bool _identity;

    private int VersionKey
    {
      get
      {
        int key = StructureVersion;
        for (int i = 0; i < Layers.Count; i++)
          key += Layers[i].Curve is null ? 0 : Layers[i].Curve.Version;
        return key;
      }
    }

    // —— 未保存标记 (调试面板提示用) ——

    private int _savedKey;

    /// <summary>指示自上次保存/载入后图层或曲线是否又被编辑过.</summary>
    public bool IsDirty => VersionKey != _savedKey;

    /// <summary>把当前编辑状态标记为已保存.</summary>
    public void MarkSaved() => _savedKey = VersionKey;

    // —— 序列化 ——

    private sealed class CurveSetDto
    {
      public bool Enabled;
      public List<RSModuleCurveLayer> Layers;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      WriteIndented = true,
      IncludeFields = true
    };

    /// <summary>控制点非法值统一收口: 排序, 首尾钉死在 0 / 1, 中间点钳进 0~1.</summary>
    private static void Sanitize(FloatCurve curve)
    {
      curve.Keys ??= new List<CurveKey>();
      if (curve.Keys.Count == 0)
        return;
      curve.Keys.Sort((a, b) => a.Time.CompareTo(b.Time));
      for (int i = 0; i < curve.Keys.Count; i++)
      {
        CurveKey key = curve.Keys[i];
        float time = i == 0 ? 0f : i == curve.Keys.Count - 1 ? 1f : Math.Clamp(key.Time, 0f, 1f);
        curve.Keys[i] = new CurveKey(time, key.Value);
      }
      curve.Interpolation = Enum.IsDefined(curve.Interpolation) ? curve.Interpolation : CurveInterpolation.CatmullRom;
    }

    /// <summary>从文件载入; 文件缺失返回空图层集, 内容异常报警告并回退默认.</summary>
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
          set.Layers.Clear();
          if (dto.Layers is not null)
            foreach (RSModuleCurveLayer layer in dto.Layers)
            {
              if (layer.Channel < 0 || layer.Channel >= ChannelCount)
                continue;
              layer.Curve ??= DefaultCurve(layer.Channel);
              if (layer.Curve.Keys is null || layer.Curve.Keys.Count == 0)
                layer.Curve = DefaultCurve(layer.Channel);
              else
                Sanitize(layer.Curve);
              set.Layers.Add(layer);
            }
          set.LoadedFromFile = true;
        }
      }
      catch (Exception ex)
      {
        Console.Log(ConsoleTextType.Warning, "RSModuleCurve",
          string.Concat("曲线文件载入失败 '", file, "', 使用默认图层: ", ex.Message));
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
        Layers = Layers
      };
      File.WriteAllText(file, JsonSerializer.Serialize(dto, JsonOptions));
      MarkSaved();
      LoadedFromFile = true;
      Console.Log(ConsoleTextType.Normal, "RSModuleCurve", string.Concat("已保存曲线: ", file));
    }
  }

  /// <summary>
  /// 渲染模块调色图层的管理器: 以 <see cref="IRenderableISceneModule"/> 类型为键缓存图层集,
  /// 文件存于游戏根目录的 <c>Assets/Visual/RSModuleCurve</c> 下, 以模块类名命名, 游戏运行时按需读取.
  /// <br><see cref="Apply"/> 对无图层/全恒等的模块零开销直通 (原样返回内容纹理); 有图层时逐层 ping-pong 应用并返回承载结果的 scratch.</br>
  /// </summary>
  public static class RSModuleCurve
  {
    private static readonly Dictionary<Type, RSModuleCurveSet> Sets = new Dictionary<Type, RSModuleCurveSet>();

    private static readonly Dictionary<Type, bool> BindStates = new Dictionary<Type, bool>();

    /// <summary>各模块类型的调色 scratch (ping-pong 双缓冲, B 侧只在出现第二个图层时才建).</summary>
    private static readonly Dictionary<Type, RenderTarget2D[]> Scratches = new Dictionary<Type, RenderTarget2D[]>();

    private static Effect _effect;
    private static EffectParameter _modeParam;
    private static EffectParameter _lutParam;
    private static bool _effectBroken;

    /// <summary>曲线文件目录 (游戏根目录 / Assets / Visual / RSModuleCurve).</summary>
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "Assets", "Visual", "RSModuleCurve");

    /// <summary>模块类型对应的曲线文件路径.</summary>
    public static string FileOf(Type moduleType) => Path.Combine(DirectoryPath, moduleType.Name + ".json");

    /// <summary>指示该模块是否已有曲线文件.</summary>
    public static bool HasFile(Type moduleType) => File.Exists(FileOf(moduleType));

    /// <summary>取模块的图层集 (无文件时返回空图层集并缓存).</summary>
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

    /// <summary>
    /// 把模块的调色图层按顺序应用到 contentRt 上, 返回承载结果的纹理.
    /// <br>无生效图层时原样返回 contentRt (零开销直通); 有图层时逐层 ping-pong 应用并返回最后一个 scratch ——
    /// 调用方把返回值画进原目标即可 ( Scratch 与 contentRt 同尺寸, 不改变合成坐标). 无返回值语义上的副作用.</br>
    /// <br>必须在调用方自己的 batch.Begin 之前调用 —— 内部会临时借用 batch 完成各层绘制并恢复渲染目标.</br>
    /// </summary>
    public static Texture2D Apply(IRenderableISceneModule module, Texture2D contentRt)
    {
      RSModuleCurveSet set = GetOrLoad(module.GetType());
      if (ShouldBind(set, module.GetType()) is false)
        return contentRt;
      Effect effect = AcquireEffect();
      if (effect is null || _modeParam is null || _lutParam is null)
        return contentRt;

      GraphicsDevice device = CoreInfo.Graphics.GraphicsDevice;
      RenderTargetBinding[] restore = device.GetRenderTargets();
      RenderTarget2D scratchA = GetScratch(module.GetType(), 0, contentRt.Width, contentRt.Height);
      Texture2D input = contentRt;
      bool applied = false;
      foreach (RSModuleCurveLayer layer in set.Layers)
      {
        if (layer.IsIdentity)
          continue;
        RenderTarget2D output = input == scratchA
          ? GetScratch(module.GetType(), 1, contentRt.Width, contentRt.Height)
          : scratchA;
        _modeParam.SetValue((float)layer.Channel);
        _lutParam.SetValue(layer.Lut);
        device.SetRenderTarget(output);
        device.Clear(Color.Transparent);
        CoreInfo.Batch.Begin(SpriteSortMode.Deferred, effect: effect);
        CoreInfo.Batch.Draw(input, new Rectangle(0, 0, output.Width, output.Height), Color.White);
        CoreInfo.Batch.End();
        input = output;
        applied = true;
      }
      if (restore.Length > 0)
        device.SetRenderTargets(restore);
      else
        device.SetRenderTarget(null);
      return applied ? input : contentRt;
    }

    private static RenderTarget2D GetScratch(Type type, int index, int width, int height)
    {
      if (Scratches.TryGetValue(type, out RenderTarget2D[] pair) is false)
        Scratches[type] = pair = new RenderTarget2D[2];
      RenderTarget2D scratch = pair[index];
      if (scratch is null || scratch.IsDisposed || scratch.Width != width || scratch.Height != height)
      {
        scratch?.Dispose();
        scratch = new RenderTarget2D(CoreInfo.Graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        pair[index] = scratch;
      }
      return scratch;
    }

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
          bind ? string.Empty : set.Enabled ? " (全部图层恒等)" : " (已停用)"));
      }
      return bind;
    }

    private static Effect AcquireEffect()
    {
      if (_effect is not null || _effectBroken)
        return _effect;
      try
      {
        _effect = Assets.Effect("Effects/RSModuleCurve.fx");
        _modeParam = _effect.Parameters["layerMode"];
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
