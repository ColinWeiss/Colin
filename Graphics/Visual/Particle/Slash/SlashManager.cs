namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>
  /// 刀光管理器 (单例): 统一持有活跃的 <see cref="SlashEffect"/>, 驱动每帧更新与渲染.
  /// <br>与 <see cref="Colin.Core.Graphics.Visual.Particle.ParticleManager"/> 平行的独立入口 —— 刀光不依附粒子发射器,
  /// 由引擎挂钩调用 <see cref="TickAndRender"/> 一步完成 (挥砍推进 → 余焰回收 → 相机绘制).</br>
  /// </summary>
  public sealed class SlashManager
  {
    private static SlashManager _instance;
    private static readonly object _lock = new object();

    /// <summary>全局实例 (首次访问时惰性创建).</summary>
    public static SlashManager Instance
    {
      get
      {
        if (_instance is null)
        {
          lock (_lock)
          {
            _instance ??= new SlashManager();
          }
        }
        return _instance;
      }
    }

    private readonly List<SlashEffect> _effects = new List<SlashEffect>();

    /// <summary>活跃刀光快照 (诊断与遍历).</summary>
    public IReadOnlyList<SlashEffect> ActiveEffects => _effects;

    /// <summary>当前活跃刀光数.</summary>
    public int EffectCount => _effects.Count;

    private SlashManager() { }

    /// <summary>
    /// 依据配置播放一段刀光 (每次播放基于全新实例).
    /// </summary>
    /// <param name="config">刀光配置 (管理器不克隆 —— 保持引用以支持实时修改).</param>
    /// <param name="position">弧心位置 (世界坐标, 像素).</param>
    /// <param name="rotation">整体旋转 (弧度).</param>
    /// <param name="scale">整体缩放.</param>
    public SlashEffect Play(SlashEffectConfig config, Vector2 position, float rotation = 0f, float scale = 1f)
    {
      lock (_lock)
      {
        SlashEffect effect = new SlashEffect(config)
        {
          Position = position,
          Rotation = rotation,
          Scale = scale
        };
        _effects.Add(effect);
        return effect;
      }
    }

    /// <summary>停止并移除一段刀光.</summary>
    public void Stop(SlashEffect effect)
    {
      if (effect is null)
        return;
      lock (_lock)
      {
        effect.Stop();
        _effects.Remove(effect);
        effect.Dispose();
      }
    }

    /// <summary>停止全部刀光.</summary>
    public void StopAll()
    {
      lock (_lock)
      {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
          _effects[i].Stop();
          _effects[i].Dispose();
        }
        _effects.Clear();
      }
    }

    /// <summary>
    /// 一步完成更新与渲染 (引擎挂钩的惯用入口):
    /// 推进全部刀光 → 回收挥完且余焰烧尽的非循环实例 → 以相机变换绘制.
    /// <br>编辑器预览实例不注册进管理器, 由编辑器自行驱动.</br>
    /// </summary>
    public void TickAndRender(float dt, Matrix cameraTransform)
    {
      lock (_lock)
      {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
          SlashEffect effect = _effects[i];
          effect.Update(dt);
          if (effect.CanRecycle)
          {
            _effects.RemoveAt(i);
            effect.Dispose();
          }
        }
        for (int i = 0; i < _effects.Count; i++)
          _effects[i].Draw(cameraTransform);
      }
    }
  }
}
