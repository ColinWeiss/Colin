using Particle.Core;

namespace Particle.Effects
{
  /// <summary>
  /// 刀光的纯代码驱动 API: 效果实例的粒子完全由游戏逻辑逐帧发射 (绕过形状/速率采样),
  /// 颜色/尺寸/透明度曲线与渲染配置沿用刀光预设.
  /// <br>典型用法 (挥砍中每帧):</br>
  /// <code>
  ///   _trail ??= BladeTrailAPI.CreateEffect();                 // 惰性创建 (持有句柄)
  ///   BladeTrailAPI.EmitSwing(_trail, grip, tip, tipVelocity); // 世界坐标
  /// </code>
  /// </summary>
  public static class BladeTrailAPI
  {
    private static ParticleEffectConfig _config;

    /// <summary>刀光轨迹的驱动型配置 (发射率 0, 仅提供曲线与渲染参数; 循环模式保持常驻).</summary>
    public static ParticleEffectConfig Config
    {
      get
      {
        if (_config is not null)
          return _config;

        ParticleEffectConfig preset = ParticlePresetFactory.Create("刀光");
        EmitterConfig driver = preset.Emitters[0].Clone();
        driver.Name = "刃锋轨迹";
        driver.EmissionRate = 0f;         // 发射完全由代码驱动
        driver.Bursts.Clear();
        driver.Capacity = 192;

        _config = new ParticleEffectConfig
        {
          Name = "刀光·剑用",
          Duration = 1f,
          Looping = true,                 // 循环模式 → 不参与自动回收, 由持有方 Stop
          Emitters = { driver },
          Render = preset.Render.Clone()
        };
        return _config;
      }
    }

    /// <summary>创建一个常驻刀光效果实例 (位置由发射参数决定, 效果变换保持零).</summary>
    public static ParticleEffect CreateEffect()
    {
      ParticleEffect effect = ParticleManager.Instance.Play(Config, Vector2.Zero);
      return effect;
    }

    /// <summary>
    /// 沿剑刃线段 (握柄 → 剑尖) 发射一帧的刀光粒子.
    /// </summary>
    /// <param name="effect">刀光效果实例 (<see cref="CreateEffect"/> 的返回值).</param>
    /// <param name="grip">剑柄 (握持点) 世界坐标.</param>
    /// <param name="tip">剑尖世界坐标.</param>
    /// <param name="tipVelocity">剑尖的挥动速度 (像素/秒) —— 决定粒子切向速度与拉伸方向.</param>
    /// <param name="samples">沿剑刃的采样数 (4~8, 越大弧线越致密).</param>
    /// <param name="intensity">强度 (0~1): 调制粒子生命与色调, 轻挥可调低.</param>
    public static void EmitSwing(ParticleEffect effect, Vector2 grip, Vector2 tip, Vector2 tipVelocity, int samples = 6, float intensity = 1f)
    {
      if (effect is null || !effect.IsPlaying)
        return;

      Vector2 blade = tip - grip;
      Vector2 sweep = tipVelocity;
      float sweepSpeed = sweep.Length();

      List<ParticleSpawnInit> batch = new List<ParticleSpawnInit>(samples);
      for (int i = 0; i < samples; i++)
      {
        float fraction = (i + 0.5f) / samples;
        // 切向速度随半径增长 (角速度一致 → 线速度 ∝ 半径), 剑尖处最强.
        Vector2 velocity = sweep * fraction * intensity;

        batch.Add(new ParticleSpawnInit
        {
          Position = grip + blade * fraction,
          Velocity = velocity,
          Size = MathHelper.Lerp(2.6f, 5.2f, fraction) * (0.85f + 0.3f * fraction),
          Rotation = 0f,
          Life = MathHelper.Lerp(0.045f, 0.115f, fraction) * intensity + 0.015f,
          Aspect = 1.8f,
          AngularVel = 0f,
          Tint = 0.9f + 0.25f * fraction
        });
      }

      effect.EmitCustom(batch, 0);
    }

    /// <summary>
    /// 停止并回收刀光效果实例 (角色销毁时调用; 不调用则实例常驻).
    /// </summary>
    public static void DestroyEffect(ParticleEffect effect) => ParticleManager.Instance.Stop(effect);
  }
}
