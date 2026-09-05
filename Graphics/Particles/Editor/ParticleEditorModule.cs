using ImGuiNET;
using MonoGame.ImGuiNet;
using Particle.Core;
using Particle.Effects;
using Particle.Rendering;
using Particle.Serialization;
using CurveInterpolation = Particle.Core.CurveInterpolation;
using NV2 = System.Numerics.Vector2;
using NV4 = System.Numerics.Vector4;
using KeyboardState = Microsoft.Xna.Framework.Input.KeyboardState;
using XnaVector4 = Microsoft.Xna.Framework.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;

namespace Particle.Editor
{
  /// <summary>
  /// 粒子可视化编辑器 (MonoGame.ImGUI, 场景模块): 预设选择 / 参数编辑 / 实时预览视口 / 时间轴 / 刀光 Mesh.
  /// <br>以 <see cref="SceneRenderModule"/> 形式挂载于场景 —— 界面绘制到模块 RawRt 后透明合成,
  /// 不遮挡游戏画面; 参数修改经观察者模式实时生效; 撤销/重做经命令模式统一管理;
  /// F10 开关编辑器, 仅依赖核心库与渲染模块的公共 API.</br>
  /// </summary>
  public class ParticleEditorModule : SceneRenderModule
  {
    private ImGuiRenderer _imGui;
    private GraphicsDevice _device;
    private RenderTarget2D _previewRt;
    private IntPtr _previewBinding;
    private bool _previewBindingDirty = true;

    private ParticleEffectConfig _config;
    private ParticleEffect _previewEffect;
    private EmitterConfig _selectedEmitter;
    private Particle.Slash.SlashArcConfig _slashConfig;
    private Particle.Slash.SlashArc _slashArc;
    private int _selectedEmitterIndex = -1;
    private readonly EditorCommandHistory _history = new EditorCommandHistory();

    // —— 预览相机 (世界坐标, Y 向下) ——
    private XnaVector2 _cameraPosition = XnaVector2.Zero;
    private float _cameraZoom = 1f;
    private float _cameraRotation;

    // —— 播放控制 ——
    private bool _loopPreview = true;
    private float _previewSpeed = 1f;
    private bool _showEditor = true;
    private KeyboardState _previousKeys;

    // —— 文件路径 ——
    private string _currentPath;
    private bool _slashPresetRequested;

    private Func<object> _pendingUndoSnapshot;

    /// <summary>编辑器是否显示 (F10 切换).</summary>
    public bool ShowEditor
    {
      get => _showEditor;
      set => _showEditor = value;
    }

    public ParticleEditorModule(Scene scene)
    {
      Scene = scene;
      _config = ParticlePresetFactory.Create("刀光");
      _slashPresetRequested = true;
    }

    public override void DoInitialize()
    {
      // —— ImGuiRenderer 跨场景共享 (字体图集只构建一次) ——
      _device = Scene.Game.GraphicsDevice;
      _imGui = GetSharedImGui(Scene.Game);

      int width = 640, height = 360;
      _previewRt = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.None);

      // —— 模块渲染旗标: RawRt 透明合成, 不遮挡游戏 ——
      RawRtVisible = _showEditor;
      Presentation = true;

      RebuildPreview();
    }

    /// <summary>ImGuiRenderer 跨场景共享实例 (字体图集只构建一次).</summary>
    private static ImGuiRenderer _sharedImGui;

    private static ImGuiRenderer GetSharedImGui(Game game)
    {
      if (_sharedImGui is null)
      {
        _sharedImGui = new ImGuiRenderer(game);

        // —— 中文界面字体 ——
        string fontPath = Path.Combine(Asset.FontDir, "MiSansNormal.ttf");
        if (!File.Exists(fontPath))
        {
          string[] fonts = Directory.Exists(Asset.FontDir) ? Directory.GetFiles(Asset.FontDir, "*.ttf") : Array.Empty<string>();
          if (fonts.Length > 0)
            fontPath = fonts[0];
        }
        if (File.Exists(fontPath))
          ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 16f, IntPtr.Zero, ImGui.GetIO().Fonts.GetGlyphRangesChineseFull());
        _sharedImGui.RebuildFontAtlas();

        // —— 半透明窗口底色: 编辑器浮于游戏画面之上而不遮死 ——
        ImGuiStylePtr style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.WindowBg] = new NV4(0.07f, 0.07f, 0.10f, 0.82f);
        style.Colors[(int)ImGuiCol.PopupBg] = new NV4(0.07f, 0.07f, 0.10f, 0.92f);
      }
      return _sharedImGui;
    }

    /// <summary>加载预设 (工厂模式入口).</summary>
    public void LoadPreset(string name)
    {
      _config = ParticlePresetFactory.Create(name);
      _currentPath = null;
      _history.Clear();
      _slashPresetRequested = name == "刀光";
      RebuildPreview();
    }

    /// <summary>加载配置文件.</summary>
    public void LoadConfig(string path)
    {
      if (!ParticleConfigIO.TryLoad(path, out ParticleEffectConfig config))
        return;
      _config = config;
      _currentPath = path;
      _history.Clear();
      RebuildPreview();
    }

    /// <summary>重建预览效果实例 (配置结构变化后).</summary>
    public void RebuildPreview()
    {
      if (_previewEffect is not null)
        ParticleManager.Instance.Stop(_previewEffect);

      if (!ParticleManager.Instance.IsInitialized)
        ParticleManager.Instance.Initialize(_device);

      _previewEffect = ParticleManager.Instance.Play(_config, XnaVector2.Zero);
      _previewEffect.PreviewOnly = true;

      // —— 刀光预设: 同步加载弧形刀光 Mesh (拉刀光本体) ——
      if (_config.Name == "刀光" && _currentPath is null && _slashPresetRequested)
      {
        _slashConfig = Particle.Effects.ParticlePresetFactory.CreateBladeSlashArc();
        _slashConfig.Subscribe(_ => { /* 曲线版本由编辑触发 */ });
        _slashArc = new Particle.Slash.SlashArc(_slashConfig);
      }
      else if (_config.Name != "刀光")
      {
        _slashConfig = null;
        _slashArc = null;
      }
      _selectedEmitterIndex = _config.Emitters.Count > 0 ? 0 : -1;
      _selectedEmitter = _selectedEmitterIndex >= 0 ? _config.Emitters[_selectedEmitterIndex] : null;
      _cameraPosition = XnaVector2.Zero;
      _cameraZoom = 1f;
      _cameraRotation = 0f;
      _previewBindingDirty = true;
    }

    public override void DoUpdate(GameTime time)
    {
      // —— F10 开关 (切换模块 RawRt 渲染) ——
      KeyboardState keys = Microsoft.Xna.Framework.Input.Keyboard.GetState();
      if (keys.IsKeyDown(Keys.F10) && _previousKeys.IsKeyUp(Keys.F10))
      {
        _showEditor = !_showEditor;
        RawRtVisible = _showEditor;
      }
      _previousKeys = keys;

      if (_showEditor && _previewEffect is not null)
      {
        // —— 循环预览: 非循环效果播完后自动重置 ——
        if (_previewEffect.IsFinished && !_config.Looping && _loopPreview)
          _previewEffect.Reset();

        // —— 编辑器先行驱动模拟 (帧守卫: 其他调用者本帧的更新会被自动跳过) ——
        ParticleManager.Instance.UpdateAll(Time.DeltaTime * _previewSpeed);

        // —— 刀光 Mesh 动画 (循环预览) ——
        if (_slashArc is not null)
        {
          if (_slashArc.IsFinished)
          {
            if (_loopPreview || _config.Looping)
              _slashArc.Reset();
          }
          else
          {
            _slashArc.Update(Time.DeltaTime * _previewSpeed);
          }
        }
      }
    }

    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (!_showEditor || _imGui is null)
        return;

      // 渲染预览画面 (粒子模拟已在 DoUpdate 阶段推进), 随后恢复到模块 RawRt.
      RenderPreview();
      device.SetRenderTarget(RawRt);
      device.Clear(Color.Transparent);   // 双保险: 防止上一帧的采样绑定使外层 Clear 无效 (残影).

      _imGui.BeforeLayout(CoreInfo.GameTimeCache);
      try
      {
        BuildUi();
      }
      finally
      {
        _imGui.AfterLayout();
      }
    }

    // =====================================================================
    //  UI 构建
    // =====================================================================
    private void BuildUi()
    {
      if (_config is null)
        return;

      ImGui.SetNextWindowSize(new NV2(1180, 720), ImGuiCond.FirstUseEver);
      ImGui.Begin("粒子编辑器", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse);

      BuildMenuBar();
      BuildMainLayout();

      ImGui.End();
    }

    private void BuildMenuBar()
    {
      if (!ImGui.BeginMenuBar())
        return;

      if (ImGui.BeginMenu("文件"))
      {
        if (ImGui.MenuItem("新建"))
        {
          _config = new ParticleEffectConfig();
          _currentPath = null;
          RebuildPreview();
        }
        if (ImGui.MenuItem("打开 (JSON)"))
        {
          string path = PickFile(open: true);
          if (path is not null)
            LoadConfig(path);
        }
        if (ImGui.MenuItem("保存"))
        {
          if (_currentPath is null)
            _currentPath = PickFile(open: false);
          if (_currentPath is not null)
            ParticleConfigIO.Save(_config, _currentPath);
        }
        if (ImGui.MenuItem("另存为..."))
        {
          string path = PickFile(open: false);
          if (path is not null)
          {
            ParticleConfigIO.Save(_config, path);
            _currentPath = path;
          }
        }
        ImGui.EndMenu();
      }

      if (ImGui.BeginMenu("预设"))
      {
        foreach (string preset in ParticlePresetFactory.PresetNames)
        {
          if (ImGui.MenuItem(preset))
            LoadPreset(preset);
        }
        ImGui.EndMenu();
      }

      if (ImGui.BeginMenu("编辑"))
      {
        if (ImGui.MenuItem("撤销 (Ctrl+Z)", enabled: _history.CanUndo))
          _history.Undo();
        if (ImGui.MenuItem("重做 (Ctrl+Y)", enabled: _history.CanRedo))
          _history.Redo();
        ImGui.EndMenu();
      }

      ImGui.EndMenuBar();

      // —— 快捷键 ——
      if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.Z, false))
        _history.Undo();
      if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.Y, false))
        _history.Redo();
    }

    private void BuildMainLayout()
    {
      NV2 region = ImGui.GetContentRegionAvail();
      float leftWidth = 200f;
      float rightWidth = 348f;

      // ---- 左列: 预设 + 发射器 ----
      ImGui.BeginChild("left", new NV2(leftWidth, region.Y), true);
      ImGui.TextUnformatted("预设");
      ImGui.Separator();
      foreach (string preset in ParticlePresetFactory.PresetNames)
      {
        if (ImGui.Button(preset, new NV2(leftWidth - 20f, 0f)))
          LoadPreset(preset);
      }

      ImGui.Spacing();
      ImGui.TextUnformatted("发射器 (组合)");
      ImGui.Separator();
      for (int i = 0; i < _config.Emitters.Count; i++)
      {
        bool selected = i == _selectedEmitterIndex;
        if (ImGui.Selectable($"{_config.Emitters[i].Name}##{i}", selected))
        {
          _selectedEmitterIndex = i;
          _selectedEmitter = _config.Emitters[i];
        }
      }
      if (ImGui.Button("+ 添加发射器"))
      {
        ParticleEffectConfig before = _config.Clone();
        _config.Emitters.Add(new EmitterConfig { Name = $"发射器 {_config.Emitters.Count + 1}" });
        ParticleEffectConfig after = _config.Clone();
        _history.Execute(new DelegateCommand("添加发射器",
          () => RestoreConfig(after),
          () => RestoreConfig(before)));
        RebuildPreview();
      }
      ImGui.SameLine();
      ImGui.BeginDisabled(_selectedEmitter is null);
      if (ImGui.Button("- 删除"))
      {
        ParticleEffectConfig before = _config.Clone();
        _config.Emitters.RemoveAt(_selectedEmitterIndex);
        ParticleEffectConfig after = _config.Clone();
        _history.Execute(new DelegateCommand("删除发射器",
          () => RestoreConfig(after),
          () => RestoreConfig(before)));
        RebuildPreview();
      }
      ImGui.EndDisabled();

      ImGui.Spacing();
      ImGui.Separator();
      ImGui.TextDisabled($"策略: {ParticleManager.Instance.StrategyPath}");
      ImGui.TextDisabled($"实例数: {_previewEffect?.InstanceCount ?? 0}");
      ImGui.TextDisabled($"效果: {ParticleManager.Instance.EffectCount}");

      ImGui.EndChild();
      ImGui.SameLine();

      // ---- 中列: 播放控制 + 预览视口 ----
      ImGui.BeginChild("center", new NV2(region.X - leftWidth - rightWidth - 16f, region.Y), true);

      if (ImGui.Button("▶ 播放"))
        _previewEffect?.Play();
      ImGui.SameLine();
      if (ImGui.Button("⏸ 暂停"))
        _previewEffect?.Pause();
      ImGui.SameLine();
      if (ImGui.Button("⏹ 停止"))
        _previewEffect?.Stop();
      ImGui.SameLine();
      if (ImGui.Button("⟲ 重置"))
        _previewEffect?.Reset();
      ImGui.SameLine();
      if (ImGui.Button("🎲 种子"))
        _previewEffect?.Reset(_config.Seed != 0 ? _config.Seed : Environment.TickCount);

      ImGui.SameLine();
      bool looping = _loopPreview;
      if (ImGui.Checkbox("循环预览", ref looping))
        _loopPreview = looping;

      ImGui.SameLine();
      ImGui.TextDisabled($"相机: 滚轮缩放 | 中键平移 | 右键旋转");

      BuildPreviewViewport();

      ImGui.EndChild();
      ImGui.SameLine();

      // ---- 右列: 一级页签 (发射参数 / 时间轴 / 刀光 Mesh) ----
      ImGui.BeginChild("right", new NV2(rightWidth, region.Y), true);
      if (ImGui.BeginTabBar("rightTabs"))
      {
        if (ImGui.BeginTabItem("发射参数"))
        {
          if (_selectedEmitter is not null && _selectedEmitterIndex >= 0)
          {
            BuildEmitterPanel(_selectedEmitter);
          }
          else
          {
            ImGui.TextDisabled("选择一个发射器以编辑参数.");
          }
          ImGui.Separator();
          BuildRenderPanel();
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("时间轴"))
        {
          BuildTimelineContent();
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("刀光 Mesh"))
        {
          BuildSlashContent();
          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
      }
      ImGui.EndChild();
    }

    private void BuildPreviewViewport()
    {
      if (_previewRt is null)
        return;

      if (_previewBindingDirty || _previewBinding == IntPtr.Zero)
      {
        _previewBinding = _imGui.BindTexture(_previewRt);
        _previewBindingDirty = false;
      }

      NV2 available = ImGui.GetContentRegionAvail();
      float size = MathF.Min(available.X, available.Y);
      NV2 viewport = new NV2(size, size);
      ImGui.Image(_previewBinding, viewport);
      bool hovered = ImGui.IsItemHovered();
      bool active = ImGui.IsItemActive();

      // —— 相机交互 ——
      if (hovered)
      {
        float wheel = ImGui.GetIO().MouseWheel;
        if (wheel != 0f)
          _cameraZoom = Math.Clamp(_cameraZoom * (1f + wheel * 0.1f), 0.05f, 8f);
      }
      if (active && ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
      {
        NV2 delta = ImGui.GetIO().MouseDelta;
        _cameraPosition -= new XnaVector2(delta.X, delta.Y) / _cameraZoom;
      }
      if (active && ImGui.IsMouseDragging(ImGuiMouseButton.Right))
      {
        NV2 delta = ImGui.GetIO().MouseDelta;
        _cameraRotation += delta.X * 0.01f;
      }
    }

    private void RenderPreview()
    {
      if (_previewRt is null || _previewEffect is null)
        return;

      GraphicsDevice device = _device;
      device.SetRenderTarget(_previewRt);
      device.Clear(new Color(16, 16, 22));

      int width = _previewRt.Width;
      int height = _previewRt.Height;
      XnaMatrix projection = XnaMatrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
      XnaMatrix view =
        XnaMatrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0)
        * XnaMatrix.CreateRotationZ(_cameraRotation)
        * XnaMatrix.CreateScale(_cameraZoom)
        * XnaMatrix.CreateTranslation(width / 2f, height / 2f, 0);

      (Texture2D dataTexture, int count) = _previewEffect.Strategy.ResolveFrame();
      if (dataTexture is not null && count > 0)
        ParticleManager.Instance.Renderer.Draw(dataTexture, count, _config.Render, view * projection);

      // —— 刀光 Mesh 本体 (拉刀光条带) ——
      if (_slashArc is not null)
        Particle.Slash.SlashRenderer.GetOrCreate().DrawOne(_slashArc, view * projection);
    }

    // =====================================================================
    //  参数面板
    // =====================================================================
    private void BuildEmitterPanel(EmitterConfig emitter)
    {
      ImGui.TextUnformatted($"发射器: {emitter.Name}");
      ImGui.Separator();

      if (ImGui.CollapsingHeader("发射", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedString("名称", () => emitter.Name, v => emitter.Name = v, NotifyAll);
        TrackedInt("槽位上限", () => emitter.Capacity, v => emitter.Capacity = v, 1, 8192, NotifyAll);
        TrackedFloat("发射率 (个/秒)", () => emitter.EmissionRate, v => emitter.EmissionRate = v, 0f, 2000f, NotifyAll);
        TrackedFloat("开始延迟 (秒)", () => emitter.StartTime, v => emitter.StartTime = v, 0f, 10f, NotifyAll);
        bool stretched = emitter.StretchedBillboard;
        if (ImGui.Checkbox("速度拉伸 (刀光)", ref stretched))
        {
          emitter.StretchedBillboard = stretched;
          NotifyAll();
        }
        TrackedInt("随机种子 (0=随机)", () => emitter.Seed, v => emitter.Seed = v, 0, 999999, NotifyAll);
      }

      if (ImGui.CollapsingHeader("形状", ImGuiTreeNodeFlags.DefaultOpen))
      {
        EmissionShapeConfig shape = emitter.Shape;
        TrackedCombo("形状类型", () => (int)shape.Shape, v => shape.Shape = (EmissionShapeType)v,
          new[] { "点", "线段", "圆环", "圆弧" }, NotifyAll);

        switch (shape.Shape)
        {
          case EmissionShapeType.Segment:
            TrackedVector2("起点偏移", () => shape.SegmentFrom, v => shape.SegmentFrom = v, -400f, 400f, NotifyAll);
            TrackedVector2("终点偏移", () => shape.SegmentTo, v => shape.SegmentTo = v, -400f, 400f, NotifyAll);
            break;
          case EmissionShapeType.Circle:
            TrackedFloat("半径", () => shape.Radius, v => shape.Radius = v, 1f, 400f, NotifyAll);
            bool edge = shape.EdgeOnly;
            if (ImGui.Checkbox("仅圆周发射", ref edge))
            {
              shape.EdgeOnly = edge;
              NotifyAll();
            }
            break;
          case EmissionShapeType.Arc:
            TrackedFloat("半径", () => shape.Radius, v => shape.Radius = v, 1f, 400f, NotifyAll);
            TrackedFloat("起始角 (度)", () => shape.ArcStart, v => shape.ArcStart = v, -360f, 360f, NotifyAll);
            TrackedFloat("弧心角 (度)", () => shape.ArcAngle, v => shape.ArcAngle = v, -360f, 360f, NotifyAll);
            break;
        }

        TrackedCombo("速度方向", () => (int)shape.Velocity, v => shape.Velocity = (VelocityMode)v,
          new[] { "径向向外", "弧线切向", "固定朝向", "随机" }, NotifyAll);
        if (shape.Velocity == VelocityMode.Direction)
        {
          TrackedFloat("基准朝向 (度)", () => shape.DirectionBase, v => shape.DirectionBase = v, -360f, 360f, NotifyAll);
          TrackedFloat("散布半角 (度)", () => shape.DirectionSpread, v => shape.DirectionSpread = v, 0f, 180f, NotifyAll);
        }
        TrackedFloat("抖动半径", () => shape.Jitter, v => shape.Jitter = v, 0f, 100f, NotifyAll);
      }

      if (ImGui.CollapsingHeader("速度与生命", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedFloat("初速下限 (像素/秒)", () => emitter.SpeedMin, v => emitter.SpeedMin = v, 0f, 4000f, NotifyAll);
        TrackedFloat("初速上限 (像素/秒)", () => emitter.SpeedMax, v => emitter.SpeedMax = v, 0f, 4000f, NotifyAll);
        TrackedFloat("生命下限 (秒)", () => emitter.LifeMin, v => emitter.LifeMin = MathF.Max(0.01f, v), 0.01f, 10f, NotifyAll);
        TrackedFloat("生命上限 (秒)", () => emitter.LifeMax, v => emitter.LifeMax = MathF.Max(0.01f, v), 0.01f, 10f, NotifyAll);
      }

      if (ImGui.CollapsingHeader("外观", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedFloat("尺寸下限 (像素)", () => emitter.SizeMin, v => emitter.SizeMin = MathF.Max(0.1f, v), 0.1f, 128f, NotifyAll);
        TrackedFloat("尺寸上限 (像素)", () => emitter.SizeMax, v => emitter.SizeMax = MathF.Max(0.1f, v), 0.1f, 128f, NotifyAll);
        TrackedFloat("长宽比", () => emitter.Aspect, v => emitter.Aspect = MathF.Max(0.05f, v), 0.05f, 8f, NotifyAll);
        TrackedFloat("旋转下限 (度)", () => emitter.RotationMin, v => emitter.RotationMin = v, -720f, 720f, NotifyAll);
        TrackedFloat("旋转上限 (度)", () => emitter.RotationMax, v => emitter.RotationMax = v, -720f, 720f, NotifyAll);
        TrackedFloat("角速度下限 (度/秒)", () => emitter.AngularVelocityMin, v => emitter.AngularVelocityMin = v, -1080f, 1080f, NotifyAll);
        TrackedFloat("角速度上限 (度/秒)", () => emitter.AngularVelocityMax, v => emitter.AngularVelocityMax = v, -1080f, 1080f, NotifyAll);
        TrackedFloat("色调下限", () => emitter.TintMin, v => emitter.TintMin = v, 0f, 2f, NotifyAll);
        TrackedFloat("色调上限", () => emitter.TintMax, v => emitter.TintMax = v, 0f, 2f, NotifyAll);
      }

      if (ImGui.CollapsingHeader("物理", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedFloat("重力 X (像素/秒²)", () => emitter.Gravity.X, v => emitter.Gravity = new XnaVector2(v, emitter.Gravity.Y), -2000f, 2000f, NotifyAll);
        TrackedFloat("重力 Y (像素/秒²)", () => emitter.Gravity.Y, v => emitter.Gravity = new XnaVector2(emitter.Gravity.X, v), -2000f, 2000f, NotifyAll);
        TrackedFloat("线性阻尼 (1/秒)", () => emitter.Drag, v => emitter.Drag = v, 0f, 20f, NotifyAll);
      }
    }

    private void BuildRenderPanel()
    {
      RenderConfig render = _config.Render;
      if (!ImGui.CollapsingHeader("渲染", ImGuiTreeNodeFlags.DefaultOpen))
        return;

      TrackedCombo("混合模式", () => (int)render.Blend, v => render.Blend = (ParticleBlendMode)v,
        new[] { "加法 (发光)", "Alpha 混合", "非预乘 Alpha", "不混合" }, NotifyAll);
      TrackedFloat("拉伸系数 (秒)", () => render.StretchFactor, v => render.StretchFactor = v, 0f, 0.3f, NotifyAll);
      TrackedFloat("拉伸上限 (像素)", () => render.MaxStretchLength, v => render.MaxStretchLength = v, 0f, 600f, NotifyAll);

      TrackedString("纹理 (内置: white/glow/blade/spark/smoke)", () => render.Texture, v => render.Texture = v, NotifyAll);
      ImGui.SameLine();
      if (ImGui.SmallButton("浏览... (粒子层)"))
      {
        string file = PickTexture();
        if (file is not null)
        {
          string name = "file:" + file;
          ParticleManager.Instance.Renderer.RegisterTexture(name, Texture2D.FromFile(ParticleManager.Instance.GraphicsDevice, file));
          render.Texture = name;
          NotifyAll();
        }
      }

      if (ImGui.TreeNodeEx("效果", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedString("效果名称", () => _config.Name, v => _config.Name = v, NotifyAll);
        TrackedBool("循环播放", () => _config.Looping, v => _config.Looping = v, NotifyAll);
        TrackedFloat("时长 (秒)", () => _config.Duration, v => _config.Duration = MathF.Max(0.05f, v), 0.05f, 10f, NotifyAll);
        ImGui.TreePop();
      }
    }

    // =====================================================================
    //  刀光 Mesh 面板 (拉刀光条带)
    // =====================================================================
    private void BuildSlashContent()
    {
      if (_slashConfig is null)
      {
        ImGui.TextDisabled("加载 \"刀光\" 预设后可编辑刀光 Mesh.");
        return;
      }

      Particle.Slash.SlashArcConfig cfg = _slashConfig;

      if (ImGui.Button("▶ 重新挥砍"))
        _slashArc?.Reset();
      ImGui.SameLine();
      ImGui.TextDisabled(IsSlashActive() ? "挥扫中" : "待机");

      // —— 收起方式: 渐隐 / 横扫 (尾部跟随) ——
      int retire = (int)cfg.Retire;
      if (ImGui.Combo("收起方式", ref retire, "扫完后整体渐隐\0横扫 (尾部跟随消亡)\0"))
      {
        cfg.Retire = (Particle.Slash.SlashRetireMode)retire;
        NotifySlash();
      }

      TrackedFloat("弧线半径 (像素)", () => cfg.Radius, v => cfg.Radius = v, 10f, 600f, NotifySlash);
      TrackedFloat("起始角 (度)", () => cfg.ArcFrom, v => cfg.ArcFrom = v, -360f, 360f, NotifySlash);
      TrackedFloat("结束角 (度)", () => cfg.ArcTo, v => cfg.ArcTo = v, -360f, 360f, NotifySlash);
      TrackedFloat("挥扫时长 (秒)", () => cfg.SweepTime, v => cfg.SweepTime = MathF.Max(0.02f, v), 0.02f, 2f, NotifySlash);
      TrackedFloat("渐隐时长 (秒)", () => cfg.FadeTime, v => cfg.FadeTime = MathF.Max(0.02f, v), 0.02f, 3f, NotifySlash);

      if (cfg.Retire == Particle.Slash.SlashRetireMode.Sweep)
      {
        ImGui.Separator();
        ImGui.TextUnformatted("横扫参数 (尾端向刃头收去)");
        TrackedFloat("收拢速度 (度/秒)", () => cfg.CatchupSpeed, v => cfg.CatchupSpeed = Math.Clamp(v, 0f, 20000f), 0f, 20000f, NotifySlash);
      }
      TrackedFloat("最大全宽 (像素)", () => cfg.Width, v => cfg.Width = MathF.Max(1f, v), 1f, 200f, NotifySlash);
      TrackedInt("采样段数", () => cfg.Segments, v => cfg.Segments = Math.Clamp(v, 4, 512), 4, 512, NotifySlash);
      TrackedString("纹理", () => cfg.Texture, v => cfg.Texture = v, NotifySlash);
      ImGui.SameLine();
      if (ImGui.SmallButton("浏览... (刀光本体)##slash"))
      {
        string file = PickTexture();
        if (file is not null)
        {
          string name = "file:" + file;
          ParticleManager.Instance.Renderer.RegisterTexture(name, Texture2D.FromFile(ParticleManager.Instance.GraphicsDevice, file));
          cfg.Texture = name;
          NotifySlash();
        }
      }

      bool reversed = cfg.Reversed;
      if (ImGui.Checkbox("反向挥扫", ref reversed))
      {
        cfg.Reversed = reversed;
        NotifySlash();
      }

      NV4 head = new NV4(cfg.HeadColor.X, cfg.HeadColor.Y, cfg.HeadColor.Z, cfg.HeadColor.W);
      if (ImGui.ColorEdit4("头部颜色", ref head))
        cfg.HeadColor = new XnaVector4(head.X, head.Y, head.Z, head.W);
      NV4 tail = new NV4(cfg.TailColor.X, cfg.TailColor.Y, cfg.TailColor.Z, cfg.TailColor.W);
      if (ImGui.ColorEdit4("尾部颜色", ref tail))
        cfg.TailColor = new XnaVector4(tail.X, tail.Y, tail.Z, tail.W);

      ImGui.TextDisabled("顶点直接摆在弧线上 (拉刀光 Mesh), 宽度呈月牙分布, 头亮尾隐.");

      // —— 整体坐标系: X/Y 缩放决定长宽, 整体旋转 ——
      ImGui.Separator();
      ImGui.TextUnformatted("整体坐标系 (2D)");
      TrackedFloat("横向缩放 (长)", () => cfg.ScaleX, v => cfg.ScaleX = v, 0.1f, 4f, NotifySlash);
      TrackedFloat("纵向缩放 (宽)", () => cfg.ScaleY, v => cfg.ScaleY = v, 0.1f, 4f, NotifySlash);
      TrackedFloat("整体旋转 (度)", () => cfg.RotationDeg, v => cfg.RotationDeg = v, -360f, 360f, NotifySlash);
    }

    private bool IsSlashActive() => _slashArc is not null && !_slashArc.IsFinished;

    /// <summary>统一设置本发射器全部曲线的插值模式.</summary>
    private void SetCurveInterpolation(CurveInterpolation mode)
    {
      EmitterConfig emitter = _selectedEmitter;
      if (emitter is null)
        return;
      emitter.RateCurve.Interpolation = mode;
      emitter.ColorOverLife.Interpolation = mode;
      emitter.OpacityOverLife.Interpolation = mode;
      emitter.SizeOverLife.Interpolation = mode;
      emitter.SpeedCurve.Interpolation = mode;
    }

    private void NotifySlash() => _slashConfig?.NotifyChanged();

    // =====================================================================
    //  时间轴窗口
    // =====================================================================
    private void BuildTimelineContent()
    {
      if (_selectedEmitter is null)
      {
        ImGui.TextDisabled("选择一个发射器以编辑时间轴.");
        return;
      }

      // —— 插值模式: 统一作用于本发射器的全部曲线 (CPU/GPU 求值同源) ——
      int interp = (int)_selectedEmitter.OpacityOverLife.Interpolation;
      if (ImGui.Combo("曲线插值", ref interp, "线性折线 平滑 (SmoothStep) 平滑样条 (Catmull-Rom) "))
      {
        SetCurveInterpolation((CurveInterpolation)interp);
        NotifyAll();
      }

      float duration = _config.Duration;
      ParticleTimeline.RateTimeline(
        $"##rate{_selectedEmitterIndex}",
        _selectedEmitter,
        ref duration,
        _previewEffect?.Time ?? 0f,
        NotifyAll);
      _config.Duration = duration;

      if (ImGui.BeginTabBar("curveTabs"))
      {
        if (ImGui.BeginTabItem("透明度曲线"))
        {
          ParticleTimeline.FloatCurveEditor(
            $"##opacity{_selectedEmitterIndex}",
            _selectedEmitter.OpacityOverLife,
            new NV2(ImGui.GetContentRegionAvail().X, 110f),
            NotifyAll);
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("尺寸曲线"))
        {
          ParticleTimeline.FloatCurveEditor(
            $"##size{_selectedEmitterIndex}",
            _selectedEmitter.SizeOverLife,
            new NV2(ImGui.GetContentRegionAvail().X, 110f),
            NotifyAll);
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("速度曲线"))
        {
          ParticleTimeline.FloatCurveEditor(
            $"##speed{_selectedEmitterIndex}",
            _selectedEmitter.SpeedCurve,
            new NV2(ImGui.GetContentRegionAvail().X, 110f),
            NotifyAll);
          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("颜色曲线"))
        {
          ParticleTimeline.ColorCurveEditor(
            $"##color{_selectedEmitterIndex}",
            _selectedEmitter.ColorOverLife,
            new NV2(ImGui.GetContentRegionAvail().X, 110f),
            NotifyAll);
          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
      }

      ImGui.TextDisabled("双击添加关键帧 | 拖拽移动 | 右键删除");
    }

    // =====================================================================
    //  变更通知与撤销辅助
    // =====================================================================
    private void NotifyAll()
    {
      _selectedEmitter?.NotifyChanged();
      _config.NotifyChanged();
    }

    /// <summary>用快照恢复配置 (撤销/重做发射器增删).</summary>
    private void RestoreConfig(ParticleEffectConfig snapshot)
    {
      _config.Name = snapshot.Name;
      _config.Duration = snapshot.Duration;
      _config.Looping = snapshot.Looping;
      _config.Render = snapshot.Render.Clone();
      _config.Emitters = snapshot.Emitters.Select(e => e.Clone()).ToList();
      RebuildPreview();
    }

    private void TrackedFloat(string label, Func<float> get, Action<float> set, float min, float max, Action onChanged)
    {
      float value = get();
      if (ImGui.SliderFloat(label, ref value, min, max))
      {
        set(value);
        onChanged();
      }
      CommitUndo(label, () => (object)get(), v => set((float)v), onChanged);
    }

    private void TrackedInt(string label, Func<int> get, Action<int> set, int min, int max, Action onChanged)
    {
      int value = get();
      if (ImGui.InputInt(label, ref value))
      {
        set(Math.Clamp(value, min, max));
        onChanged();
      }
      CommitUndo(label, () => (object)get(), v => set((int)v), onChanged);
    }

    private void TrackedBool(string label, Func<bool> get, Action<bool> set, Action onChanged)
    {
      bool value = get();
      if (ImGui.Checkbox(label, ref value))
      {
        bool before = get();
        set(value);
        onChanged();
        _history.Execute(new DelegateCommand(label,
          () => { set(value); onChanged(); },
          () => { set(before); onChanged(); }));
      }
    }

    private void TrackedString(string label, Func<string> get, Action<string> set, Action onChanged)
    {
      string value = get() ?? string.Empty;
      if (ImGui.InputText(label, ref value, 128))
      {
        string before = get();
        set(value);
        onChanged();
        _history.Execute(new DelegateCommand(label,
          () => { set(value); onChanged(); },
          () => { set(before); onChanged(); }));
      }
    }

    private void TrackedVector2(string label, Func<XnaVector2> get, Action<XnaVector2> set, float min, float max, Action onChanged)
    {
      NV2 value = new NV2(get().X, get().Y);
      if (ImGui.DragFloat2(label, ref value, 0.5f, min, max))
      {
        set(new XnaVector2(value.X, value.Y));
        onChanged();
      }
      CommitUndo(label, () => (object)get(), v => set((XnaVector2)v), onChanged);
    }

    private void TrackedCombo(string label, Func<int> get, Action<int> set, string[] items, Action onChanged)
    {
      int value = get();
      if (ImGui.Combo(label, ref value, items, items.Length))
      {
        int before = get();
        set(value);
        onChanged();
        _history.Execute(new DelegateCommand(label,
          () => { set(value); onChanged(); },
          () => { set(before); onChanged(); }));
      }
    }

    /// <summary>滑条类控件松开时提交撤销命令 (IsItemActivated 时记录起始快照).</summary>
    private void CommitUndo(string label, Func<object> get, Action<object> set, Action onChanged)
    {
      if (ImGui.IsItemActivated())
        _pendingUndoSnapshot = get;
      if (ImGui.IsItemDeactivatedAfterEdit() && _pendingUndoSnapshot is not null)
      {
        object before = _pendingUndoSnapshot();
        object after = get();
        _pendingUndoSnapshot = null;
        if (Equals(before, after))
          return;
        _history.Execute(new DelegateCommand(label,
          () => { set(after); onChanged(); },
          () => { set(before); onChanged(); }));
      }
    }

    // =====================================================================
    //  文件对话框 (WinForms)
    // =====================================================================
    private string PickTexture()
    {
      using System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
      {
        Filter = "图片 (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|全部文件 (*.*)|*.*",
        Title = "选择刀光纹理"
      };
      return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
    }

    private string PickFile(bool open)
    {
      using System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
      {
        Filter = "粒子配置 (*.json)|*.json|全部文件 (*.*)|*.*",
        Title = open ? "打开粒子配置" : "保存粒子配置"
      };
      if (open)
      {
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          return dialog.FileName;
      }
      else
      {
        using System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog
        {
          Filter = "粒子配置 (*.json)|*.json",
          Title = "保存粒子配置"
        };
        if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          return saveDialog.FileName;
      }
      return null;
    }
  }
}
