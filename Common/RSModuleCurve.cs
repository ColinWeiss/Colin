using Colin.Core.Graphics.Visual.Curve;
using Colin.Core.IO;
using CurveKey = Colin.Core.Graphics.Visual.Curve.CurveKey;

namespace Colin.Core.Common
{
  public sealed class RSModuleCurveLayer
  {
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

    public bool IsIdentity
    {
      get
      {
        if (Curve is null)
          return true;
        if (!ReferenceEquals(_identityCurve, Curve) || _identityKey != Curve.Version)
        {
          _identity = RSModuleCurveSet.IsIdentityShape(Channel, Curve);
          _identityCurve = Curve;
          _identityKey = Curve.Version;
        }
        return _identity;
      }
    }

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
        data[i] = new Vector4(Math.Clamp(Curve.Evaluate(value), 0f, 1f), 0f, 0f, 1f);
      }
      _lut.SetData(data);
      _bakedCurve = Curve;
      _bakedVersion = Curve.Version;
    }
  }

  public sealed class RSModuleCurveSet
  {
    public const int ChannelCount = 5;

    public static readonly string[] ChannelNames = { "亮度", "饱和度", "R", "G", "B" };

    public const int LutBins = 256;

    public bool Enabled = true;

    public List<RSModuleCurveLayer> Layers = new List<RSModuleCurveLayer>();

    public int StructureVersion;

    public RSModuleCurveSet() => MarkSaved();

    public bool AddLayer(int channel)
    {
      for (int i = 0; i < Layers.Count; i++)
        if (Layers[i].Channel == channel)
          return false;
      Layers.Add(new RSModuleCurveLayer(channel));
      StructureVersion++;
      return true;
    }

    public void RemoveLayerAt(int index)
    {
      if (index < 0 || index >= Layers.Count)
        return;
      Layers.RemoveAt(index);
      StructureVersion++;
    }

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

    public void ResetAll()
    {
      Enabled = true;
      Layers.Clear();
      StructureVersion++;
    }

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

    public bool IsIdentity
    {
      get
      {
        long key = VersionKey;
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
    private long _identityKey = long.MinValue;
    private bool _identity;

    /// <summary>
    /// 结构版本占高位、各曲线版本和占低位, 合成 64 位变更键.
    /// <br>不能把两者相加: ResetAll 会 StructureVersion+1 同时移除贡献了同量版本的曲线, 键不变化导致
    /// IsIdentity/IsDirty/LUT 银行等缓存全部漏检.</br>
    /// </summary>
    internal long VersionKey
    {
      get
      {
        long key = (long)StructureVersion << 32;
        for (int i = 0; i < Layers.Count; i++)
          key += (uint)(Layers[i].Curve is null ? 0 : Layers[i].Curve.Version);
        return key;
      }
    }

    private long _savedKey;

    public bool IsDirty => VersionKey != _savedKey;

    public void MarkSaved() => _savedKey = VersionKey;

    public TagCompound ToTag()
    {
      TagCompound root = new TagCompound();
      root["Enabled"] = Enabled;
      TagSeq layers = new TagSeq();
      for (int i = 0; i < Layers.Count; i++)
      {
        TagCompound layerTag = new TagCompound();
        layerTag["Channel"] = Layers[i].Channel;
        layerTag["Curve"] = CurveToTag(Layers[i].Curve);
        layers.Add(layerTag);
      }
      root["Layers"] = layers;
      return root;
    }

    public void LoadFrom(TagCompound root)
    {
      Enabled = root.GetBool("Enabled", true);
      Layers.Clear();
      bool[] seenChannels = new bool[ChannelCount];
      TagSeq layers = root.GetSeq("Layers");
      if (layers is not null)
        for (int i = 0; i < layers.Count; i++)
        {
          TagCompound layerTag = layers.GetCompound(i);
          if (layerTag is null)
            continue;
          int channel = layerTag.GetInt("Channel", 0);
          if (channel < 0 || channel >= ChannelCount || seenChannels[channel])
            continue;
          seenChannels[channel] = true;
          RSModuleCurveLayer layer = new RSModuleCurveLayer(channel);
          layer.Curve = CurveFromTag(layerTag.GetCompound("Curve"), channel);
          Layers.Add(layer);
        }
      StructureVersion++;
      MarkSaved();
    }

    public static TagCompound CurveToTag(FloatCurve curve)
    {
      TagCompound tag = new TagCompound();
      tag["Interpolation"] = (int)curve.Interpolation;
      float[] times = new float[curve.Keys.Count];
      float[] values = new float[curve.Keys.Count];
      for (int i = 0; i < curve.Keys.Count; i++)
      {
        times[i] = curve.Keys[i].Time;
        values[i] = curve.Keys[i].Value;
      }
      tag["Times"] = times;
      tag["Values"] = values;
      return tag;
    }

    public static FloatCurve CurveFromTag(TagCompound tag, int channel)
    {
      if (tag is null)
        return DefaultCurve(channel);
      float[] times = tag.GetArray<float>("Times");
      float[] values = tag.GetArray<float>("Values");
      if (times is null || values is null || times.Length != values.Length || times.Length == 0)
        return DefaultCurve(channel);
      FloatCurve curve = new FloatCurve();
      curve.Interpolation = (CurveInterpolation)tag.GetInt("Interpolation", (int)CurveInterpolation.CatmullRom);
      for (int i = 0; i < times.Length; i++)
        curve.Keys.Add(new CurveKey(times[i], values[i]));
      Sanitize(curve);
      return curve;
    }

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
  }

  public static class RSModuleCurve
  {
    private static readonly Dictionary<string, RSModuleCurveSet> Sets = new Dictionary<string, RSModuleCurveSet>();

    private static readonly Dictionary<object, bool> BindStates = new Dictionary<object, bool>();

    private static readonly Dictionary<object, string> ChronoStates = new Dictionary<object, string>();

    private static readonly Dictionary<object, RenderTarget2D[]> Scratches = new Dictionary<object, RenderTarget2D[]>();

    private static readonly Dictionary<Type, string[]> ChronoScratchNames = new Dictionary<Type, string[]>();

    private static Dictionary<string, RSModuleCurveSet> _chronoA;

    private static Dictionary<string, RSModuleCurveSet> _chronoB;

    private static float _chronoBlend;

    private static Effect _effect;
    private static EffectParameter _modeParam;
    private static EffectParameter _lutParam;
    private static EffectParameter _blendParam;
    private static bool _effectBroken;

    public static RSModuleCurveSet GetOrLoad(Type moduleType) => Get(moduleType.Name);

    public static RSModuleCurveSet Get(string typeName)
    {
      if (Sets.TryGetValue(typeName, out RSModuleCurveSet set) is false)
        Sets[typeName] = set = new RSModuleCurveSet();
      return set;
    }

    /// <summary>把当前所有非恒等的模块曲线组打包成预设包的 ModuleCurves 段.</summary>
    public static TagSeq CaptureToTag()
    {
      TagSeq seq = new TagSeq();
      foreach (KeyValuePair<string, RSModuleCurveSet> pair in Sets)
      {
        RSModuleCurveSet set = pair.Value;
        if (set.Enabled is false || set.IsIdentity)
          continue;
        TagCompound entry = new TagCompound();
        entry["Module"] = pair.Key;
        entry["Curves"] = set.ToTag();
        seq.Add(entry);
      }
      return seq;
    }

    /// <summary>用预设包的 ModuleCurves 段整体替换运行时曲线组; 传 null 等于只清空.</summary>
    public static void ApplyPreset(TagSeq moduleCurves)
    {
      Sets.Clear();
      if (moduleCurves is null)
        return;
      for (int i = 0; i < moduleCurves.Count; i++)
      {
        TagCompound entry = moduleCurves.GetCompound(i);
        if (entry is null)
          continue;
        string module = entry.GetString("Module", string.Empty);
        TagCompound curves = entry.GetCompound("Curves");
        if (module.Length == 0 || curves is null)
          continue;
        RSModuleCurveSet set = new RSModuleCurveSet();
        set.LoadFrom(curves);
        Sets[module] = set;
      }
    }

    /// <summary>
    /// 时间插值覆盖: 两个"模块类型名 → 曲线组"字典与混合系数; 传 (null, null, 0) 恢复运行时曲线组.
    /// <br>某模块只在一侧有曲线时该侧全额生效, 两侧都有时按系数把两组各自完整作用的结果逐值混合.</br>
    /// </summary>
    public static void SetChronoOverride(Dictionary<string, RSModuleCurveSet> a, Dictionary<string, RSModuleCurveSet> b, float blend)
    {
      _chronoA = a;
      _chronoB = b;
      _chronoBlend = Math.Clamp(blend, 0f, 1f);
    }

    public static IReadOnlyDictionary<string, RSModuleCurveSet> ChronoSetsA => _chronoA;

    public static IReadOnlyDictionary<string, RSModuleCurveSet> ChronoSetsB => _chronoB;

    public static float ChronoBlend => _chronoBlend;

    public static Texture2D Apply(IRenderableISceneModule module, Texture2D contentRt)
    {
      Type type = module.GetType();
      if (_chronoA is null && _chronoB is null)
        return RunLayers(GetOrLoad(type), type, type.Name, contentRt, "RSModuleCurve");
      RSModuleCurveSet setA = null;
      RSModuleCurveSet setB = null;
      _chronoA?.TryGetValue(type.Name, out setA);
      _chronoB?.TryGetValue(type.Name, out setB);
      bool hasA = setA is not null && setA.Enabled && setA.IsIdentity is false;
      bool hasB = setB is not null && setB.Enabled && setB.IsIdentity is false;
      if (hasA is false && hasB is false)
        return RunLayers(GetOrLoad(type), type, type.Name, contentRt, "RSModuleCurve");
      if (hasB is false)
        return RunLayers(setA, type, type.Name, contentRt, "RSModuleCurve");
      if (hasA is false)
        return RunLayers(setB, type, type.Name, contentRt, "RSModuleCurve");
      return RunBlended(setA, setB, _chronoBlend, type, type.Name, contentRt);
    }

    internal static Texture2D RunLayers(RSModuleCurveSet set, object key, string targetName, Texture2D contentRt, string logTag)
    {
      bool bind = set.Enabled && set.IsIdentity is false;
      if (logTag is not null && (BindStates.TryGetValue(key, out bool last) is false || last != bind))
      {
        BindStates[key] = bind;
        Console.Log(ConsoleTextType.Remind, logTag, string.Concat(
          bind ? "曲线生效: " : "曲线旁路: ", targetName,
          bind ? string.Empty : set.Enabled ? " (全部图层恒等)" : " (已停用)"));
      }
      if (bind is false)
        return contentRt;
      Effect effect = AcquireEffect();
      if (effect is null || _modeParam is null || _lutParam is null)
        return contentRt;

      GraphicsDevice device = CoreInfo.Graphics.GraphicsDevice;
      RenderTargetBinding[] restore = device.GetRenderTargets();
      RenderTarget2D scratchA = GetScratch(key, 0, contentRt.Width, contentRt.Height);
      Texture2D input = contentRt;
      bool applied = false;
      foreach (RSModuleCurveLayer layer in set.Layers)
      {
        if (layer.IsIdentity)
          continue;
        RenderTarget2D output = input == scratchA
          ? GetScratch(key, 1, contentRt.Width, contentRt.Height)
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

    private static Texture2D RunBlended(RSModuleCurveSet setA, RSModuleCurveSet setB, float blend, Type type, string targetName, Texture2D contentRt)
    {
      float f = Math.Clamp(blend, 0f, 1f);
      string state = f <= 0f ? "本时段" : f >= 1f ? "下一时段" : "混合";
      if (ChronoStates.TryGetValue(type, out string last) is false || last != state)
      {
        ChronoStates[type] = state;
        Console.Log(ConsoleTextType.Remind, "RSModuleCurve", string.Concat("时段曲线: ", targetName, " → ", state));
      }
      string[] keys = ChronoKeys(type);
      Texture2D resultA = f >= 1f ? contentRt : RunLayers(setA, keys[0], targetName, contentRt, null);
      if (f <= 0f)
        return resultA;
      Texture2D resultB = RunLayers(setB, keys[1], targetName, contentRt, null);
      if (f >= 1f)
        return resultB;
      if (resultA == contentRt && resultB == contentRt)
        return contentRt;
      if (resultB == contentRt)
        return resultA;
      if (resultA == contentRt)
        return resultB;
      Effect effect = AcquireEffect();
      if (effect is null || _modeParam is null || _lutParam is null || _blendParam is null)
        return resultA;
      GraphicsDevice device = CoreInfo.Graphics.GraphicsDevice;
      RenderTargetBinding[] restore = device.GetRenderTargets();
      RenderTarget2D output = GetScratch(keys[2], 0, contentRt.Width, contentRt.Height);
      _modeParam.SetValue(5f);
      _blendParam.SetValue(f);
      _lutParam.SetValue(resultB);
      device.SetRenderTarget(output);
      device.Clear(Color.Transparent);
      CoreInfo.Batch.Begin(SpriteSortMode.Deferred, effect: effect);
      CoreInfo.Batch.Draw(resultA, new Rectangle(0, 0, output.Width, output.Height), Color.White);
      CoreInfo.Batch.End();
      if (restore.Length > 0)
        device.SetRenderTargets(restore);
      else
        device.SetRenderTarget(null);
      return output;
    }

    private static string[] ChronoKeys(Type type)
    {
      if (ChronoScratchNames.TryGetValue(type, out string[] keys) is false)
        ChronoScratchNames[type] = keys = new[]
        {
          string.Concat(type.Name, "#ChronoA"),
          string.Concat(type.Name, "#ChronoB"),
          string.Concat(type.Name, "#ChronoBlend")
        };
      return keys;
    }

    private static RenderTarget2D GetScratch(object key, int index, int width, int height)
    {
      if (Scratches.TryGetValue(key, out RenderTarget2D[] pair) is false)
        Scratches[key] = pair = new RenderTarget2D[2];
      RenderTarget2D scratch = pair[index];
      if (scratch is null || scratch.IsDisposed || scratch.Width != width || scratch.Height != height)
      {
        scratch?.Dispose();
        scratch = new RenderTarget2D(CoreInfo.Graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        pair[index] = scratch;
      }
      return scratch;
    }

    private static Effect AcquireEffect()
    {
      if (_effect is not null || _effectBroken)
        return _effect;
      try
      {
        _effect = EngineAssets.Effect("Effects/RSModuleCurve.fx");
        _modeParam = _effect.Parameters["layerMode"];
        _lutParam = _effect.Parameters["curveLut"];
        _blendParam = _effect.Parameters["chronoBlend"];
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
