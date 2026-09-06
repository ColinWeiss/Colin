using ImGuiNET;
using MonoGame.ImGuiNet;
using Particle.Core;
using Particle.Effects;
using Particle.Rendering;
using Particle.Serialization;
using Particle.Slash;
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
  /// 粒子 / 刀光可视化编辑器 (MonoGame.ImGUI, 场景模块).
  /// <br>两种编辑对象经左栏顶部切换:</br>
  /// <br>- <b>粒子</b>: 预设选择 / 发射器组合 / 参数编辑 / 时间轴 (ParticlePresetFactory + ParticleEffect);</br>
  /// <br>- <b>刀光</b>: 独立大功能 (SlashPresetFactory + SlashEffect) —— 刀光本体 (拉刀光 Mesh) 与
  /// 与前缘角度绑定的刃花层分页签编辑, 播放控制统一驱动.</br>
  /// <br>界面绘制到模块 RawRt 后透明合成, 不遮挡游戏画面; F11 开关编辑器.</br>
  /// </summary>
  public class ParticleEditorModule : SceneRenderModule
  {
    private enum EditorMode
    {
      Particle,
      Slash
    }

    private ImGuiRenderer _imGui;
    private GraphicsDevice _device;
    private RenderTarget2D _previewRt;
    private IntPtr _previewBinding;
    private bool _previewBindingDirty = true;

    /// <summary>文件对话框打开期间挂起编辑器渲染/更新 —— WinForms 模态对话框的嵌套消息循环
    /// 会重入游戏主循环, 不挂起会导致 ImGui 帧重入与点击穿透 (选完图跳预设的 BUG 根因).</summary>
    private bool _modalDialogOpen;

    // —— 编辑对象 ——
    private EditorMode _mode = EditorMode.Slash;

    // —— 粒子模式状态 ——
    private ParticleEffectConfig _config;
    private ParticleEffect _previewEffect;
    private EmitterConfig _selectedEmitter;
    private int _selectedEmitterIndex = -1;
    private string _currentPath;

    // —— 刀光模式状态 ——
    private SlashEffectConfig _slashEffectConfig;
    private SlashEffect _slashEffect;
    private string _slashPath;

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

    private Func<object> _pendingUndoSnapshot;

    /// <summary>编辑器是否显示 (F11 切换).</summary>
    public bool ShowEditor
    {
      get => _showEditor;
      set => _showEditor = value;
    }

    public ParticleEditorModule(Scene scene)
    {
      Scene = scene;
      _config = ParticlePresetFactory.Create("爆炸");
      _slashEffectConfig = SlashPresetFactory.CreateStandard();
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

      RebuildParticlePreview();
      RebuildSlashPreview();
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

    // =====================================================================
    //  预览实例管理
    //  (编辑器对文件对话框的挂起也在此层: 对话框期间冻结全部推进, 防止重入)
    // =====================================================================

    /// <summary>重建粒子预览效果实例 (配置结构变化后).</summary>
    public void RebuildParticlePreview()
    {
      if (_previewEffect is not null)
        ParticleManager.Instance.Stop(_previewEffect);

      if (!ParticleManager.Instance.IsInitialized)
        ParticleManager.Instance.Initialize(_device);

      _previewEffect = ParticleManager.Instance.Play(_config, XnaVector2.Zero);
      _previewEffect.PreviewOnly = true;
      _selectedEmitterIndex = _config.Emitters.Count > 0 ? 0 : -1;
      _selectedEmitter = _selectedEmitterIndex >= 0 ? _config.Emitters[_selectedEmitterIndex] : null;
      _previewBindingDirty = true;
    }

    /// <summary>重建刀光预览实例 (配置结构变化后).</summary>
    public void RebuildSlashPreview()
    {
      _slashEffect?.Dispose();
      if (!ParticleManager.Instance.IsInitialized)
        ParticleManager.Instance.Initialize(_device);
      _slashEffect = new SlashEffect(_slashEffectConfig);
      _previewBindingDirty = true;
    }

    private void ResetCamera()
    {
      _cameraPosition = XnaVector2.Zero;
      _cameraZoom = 1f;
      _cameraRotation = 0f;
    }

    private void SetMode(EditorMode mode)
    {
      if (_mode == mode)
        return;
      _mode = mode;
      ResetCamera();
      _previewBindingDirty = true;
    }

    public override void DoUpdate(GameTime time)
    {
      // —— 文件对话框打开期间冻结 (嵌套消息循环重入防护) ——
      if (_modalDialogOpen)
        return;

      // —— F11 开关 (切换模块 RawRt 渲染) ——
      KeyboardState keys = Microsoft.Xna.Framework.Input.Keyboard.GetState();
      if (keys.IsKeyDown(Keys.F11) && _previousKeys.IsKeyUp(Keys.F11))
      {
        _showEditor = !_showEditor;
        RawRtVisible = _showEditor;
      }
      _previousKeys = keys;

      if (!_showEditor)
        return;

      if (_mode == EditorMode.Particle)
      {
        if (_previewEffect is not null)
        {
          // —— 循环预览: 非循环效果播完后自动重置 ——
          if (_previewEffect.IsFinished && !_config.Looping && _loopPreview)
            _previewEffect.Reset();

          // —— 编辑器先行驱动模拟 (帧守卫: 其他调用者本帧的更新会被自动跳过) ——
          ParticleManager.Instance.UpdateAll(Time.DeltaTime * _previewSpeed);
        }
      }
      else if (_slashEffect is not null)
      {
        if (_slashEffect.IsFinished && !_slashEffectConfig.Looping && _loopPreview)
          _slashEffect.Reset();
        _slashEffect.Update(Time.DeltaTime * _previewSpeed);
      }
    }

    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (!_showEditor || _imGui is null || _modalDialogOpen)
        return;

      // 渲染预览画面 (模拟已在 DoUpdate 阶段推进), 随后恢复到模块 RawRt.
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
      ImGui.SetNextWindowSize(new NV2(1180, 720), ImGuiCond.FirstUseEver);
      ImGui.Begin(_mode == EditorMode.Slash ? "刀光编辑器" : "粒子编辑器", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse);

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
        if (_mode == EditorMode.Particle)
          BuildParticleFileMenu();
        else
          BuildSlashFileMenu();
        ImGui.EndMenu();
      }

      if (ImGui.BeginMenu("预设"))
      {
        if (_mode == EditorMode.Particle)
        {
          foreach (string preset in ParticlePresetFactory.PresetNames)
            if (ImGui.MenuItem(preset))
              LoadParticlePreset(preset);
        }
        else
        {
          foreach (string preset in SlashPresetFactory.PresetNames)
            if (ImGui.MenuItem(preset))
              LoadSlashPreset(preset);
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

    private void BuildParticleFileMenu()
    {
      if (ImGui.MenuItem("新建"))
      {
        _config = new ParticleEffectConfig();
        _currentPath = null;
        RebuildParticlePreview();
      }
      if (ImGui.MenuItem("打开 (JSON)"))
      {
        string path = PickFile(open: true);
        if (path is not null && ParticleConfigIO.TryLoad(path, out ParticleEffectConfig config))
        {
          _config = config;
          _currentPath = path;
          _history.Clear();
          RebuildParticlePreview();
        }
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
    }

    private void BuildSlashFileMenu()
    {
      if (ImGui.MenuItem("新建"))
      {
        _slashEffectConfig = new SlashEffectConfig();
        _slashPath = null;
        RebuildSlashPreview();
      }
      if (ImGui.MenuItem("打开 (JSON)"))
      {
        string path = PickFile(open: true);
        if (path is not null && ParticleConfigIO.TryLoadSlash(path, out SlashEffectConfig config))
        {
          _slashEffectConfig = config;
          _slashPath = path;
          _history.Clear();
          RebuildSlashPreview();
        }
      }
      if (ImGui.MenuItem("保存"))
      {
        if (_slashPath is null)
          _slashPath = PickFile(open: false);
        if (_slashPath is not null)
          ParticleConfigIO.SaveSlash(_slashEffectConfig, _slashPath);
      }
      if (ImGui.MenuItem("另存为..."))
      {
        string path = PickFile(open: false);
        if (path is not null)
        {
          ParticleConfigIO.SaveSlash(_slashEffectConfig, path);
          _slashPath = path;
        }
      }
    }

    private void LoadParticlePreset(string name)
    {
      _config = ParticlePresetFactory.Create(name);
      _currentPath = null;
      _history.Clear();
      ResetCamera();
      RebuildParticlePreview();
    }

    private void LoadSlashPreset(string name)
    {
      _slashEffectConfig = SlashPresetFactory.Create(name);
      _slashPath = null;
      _history.Clear();
      ResetCamera();
      RebuildSlashPreview();
    }

    private void BuildMainLayout()
    {
      NV2 region = ImGui.GetContentRegionAvail();
      float leftWidth = 200f;
      float rightWidth = 348f;

      // ---- 左列: 编辑对象切换 + 对应内容 ----
      ImGui.BeginChild("left", new NV2(leftWidth, region.Y), true);
      BuildModeSwitch(leftWidth);
      ImGui.Separator();

      if (_mode == EditorMode.Particle)
        BuildParticleLeftColumn(leftWidth);
      else
        BuildSlashLeftColumn();

      ImGui.EndChild();
      ImGui.SameLine();

      // ---- 中列: 播放控制 + 预览视口 ----
      ImGui.BeginChild("center", new NV2(region.X - leftWidth - rightWidth - 16f, region.Y), true);
      BuildPlaybackRow();
      BuildPreviewViewport();
      ImGui.EndChild();
      ImGui.SameLine();

      // ---- 右列: 按模式切换页签 ----
      ImGui.BeginChild("right", new NV2(rightWidth, region.Y), true);
      if (_mode == EditorMode.Particle)
        BuildParticleRightTabs();
      else
        BuildSlashRightTabs();
      ImGui.EndChild();
    }

    /// <summary>左栏顶部的编辑对象切换 (当前模式高亮).</summary>
    private void BuildModeSwitch(float leftWidth)
    {
      ImGui.TextUnformatted("编辑对象");
      NV4 activeColor = new NV4(0.18f, 0.42f, 0.20f, 1f);
      float buttonWidth = (leftWidth - 28f) * 0.5f;

      bool particleActive = _mode == EditorMode.Particle;
      if (particleActive)
        ImGui.PushStyleColor(ImGuiCol.Button, activeColor);
      if (ImGui.Button("粒子", new NV2(buttonWidth, 0f)))
        SetMode(EditorMode.Particle);
      if (particleActive)
        ImGui.PopStyleColor();

      ImGui.SameLine();
      bool slashActive = _mode == EditorMode.Slash;
      if (slashActive)
        ImGui.PushStyleColor(ImGuiCol.Button, activeColor);
      if (ImGui.Button("刀光", new NV2(buttonWidth, 0f)))
        SetMode(EditorMode.Slash);
      if (slashActive)
        ImGui.PopStyleColor();
    }

    // =====================================================================
    //  粒子模式
    // =====================================================================
    private void BuildParticleLeftColumn(float leftWidth)
    {
      ImGui.TextUnformatted("预设");
      foreach (string preset in ParticlePresetFactory.PresetNames)
      {
        if (ImGui.Button(preset, new NV2(leftWidth - 20f, 0f)))
          LoadParticlePreset(preset);
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
        RebuildParticlePreview();
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
        RebuildParticlePreview();
      }
      ImGui.EndDisabled();

      ImGui.Spacing();
      ImGui.Separator();
      ImGui.TextDisabled($"策略: {ParticleManager.Instance.StrategyPath}");
      ImGui.TextDisabled($"实例数: {_previewEffect?.InstanceCount ?? 0}");
      ImGui.TextDisabled($"效果: {ParticleManager.Instance.EffectCount}");
    }

    private void BuildParticleRightTabs()
    {
      if (!ImGui.BeginTabBar("rightTabs"))
        return;

      if (ImGui.BeginTabItem("发射参数"))
      {
        if (_selectedEmitter is not null && _selectedEmitterIndex >= 0)
          BuildEmitterPanel(_selectedEmitter);
        else
          ImGui.TextDisabled("选择一个发射器以编辑参数.");
        ImGui.Separator();
        BuildRenderPanel();
        ImGui.EndTabItem();
      }
      if (ImGui.BeginTabItem("时间轴"))
      {
        BuildTimelineContent();
        ImGui.EndTabItem();
      }
      ImGui.EndTabBar();
    }

    private void BuildPlaybackRow()
    {
      bool particleMode = _mode == EditorMode.Particle;
      if (ImGui.Button("▶ 播放"))
      {
        if (particleMode) _previewEffect?.Play(); else _slashEffect?.Play();
      }
      ImGui.SameLine();
      if (ImGui.Button("⏸ 暂停"))
      {
        if (particleMode) _previewEffect?.Pause(); else _slashEffect?.Pause();
      }
      ImGui.SameLine();
      if (ImGui.Button("⏹ 停止"))
      {
        if (particleMode) _previewEffect?.Stop(); else _slashEffect?.Stop();
      }
      ImGui.SameLine();
      if (ImGui.Button("⟲ 重置"))
      {
        if (particleMode) _previewEffect?.Reset(); else _slashEffect?.Reset();
      }
      ImGui.SameLine();
      if (ImGui.Button("🎲 种子"))
      {
        int seed = Environment.TickCount;
        if (particleMode)
          _previewEffect?.Reset(_config.Seed != 0 ? _config.Seed : seed);
        else
          _slashEffect?.Reset(_slashEffectConfig.Seed != 0 ? _slashEffectConfig.Seed : seed);
      }

      ImGui.SameLine();
      bool looping = _loopPreview;
      if (ImGui.Checkbox("循环预览", ref looping))
        _loopPreview = looping;

      ImGui.SameLine();
      ImGui.TextDisabled($"相机: 滚轮缩放 | 中键平移 | 右键旋转 | 数值框双击输入");

      BuildPreviewViewport();
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
      if (_previewRt is null)
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
      XnaMatrix transform = view * projection;

      if (_mode == EditorMode.Particle)
      {
        if (_previewEffect is null)
          return;
        (Texture2D dataTexture, int count) = _previewEffect.Strategy.ResolveFrame();
        if (dataTexture is not null && count > 0)
          ParticleManager.Instance.Renderer.Draw(dataTexture, count, _config.Render, transform);
      }
      else if (_slashEffect is not null)
      {
        _slashEffect.Draw(transform);   // 刃花粒子 + 刀光 Mesh.
      }
    }

    // =====================================================================
    //  粒子模式: 参数面板
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

      TextureField("纹理 (粒子层)", () => render.Texture, v => render.Texture = v, "##texParticle");

      if (ImGui.TreeNodeEx("效果", ImGuiTreeNodeFlags.DefaultOpen))
      {
        TrackedString("效果名称", () => _config.Name, v => _config.Name = v, NotifyAll);
        TrackedBool("循环播放", () => _config.Looping, v => _config.Looping = v, NotifyAll);
        TrackedFloat("时长 (秒)", () => _config.Duration, v => _config.Duration = MathF.Max(0.05f, v), 0.05f, 10f, NotifyAll);
        ImGui.TreePop();
      }
    }

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
      RebuildParticlePreview();
    }

    /// <summary>用快照恢复刀光配置 (撤销/重做收尾修饰器与纹理层增删).</summary>
    private void RestoreSlash(SlashEffectConfig snapshot)
    {
      _slashEffectConfig = snapshot.Clone();
      RebuildSlashPreview();
    }

    // =====================================================================
    //  刀光模式
    // =====================================================================
    private void BuildSlashLeftColumn()
    {
      ImGui.TextUnformatted("预设");
      foreach (string preset in SlashPresetFactory.PresetNames)
      {
        if (ImGui.Button(preset, new NV2(180f - 20f, 0f)))
          LoadSlashPreset(preset);
      }

      ImGui.Spacing();
      ImGui.TextUnformatted("状态");
      ImGui.Separator();
      bool sweeping = _slashEffect is not null && !_slashEffect.Arc.IsFinished;
      ImGui.TextDisabled(sweeping ? "挥扫中" : "待机");
      ImGui.TextDisabled($"刃花实例: {_slashEffect?.SparkCount ?? 0}");
      ImGui.TextDisabled($"循环挥砍: {(_slashEffectConfig.Looping ? "开" : "关")}");

      ImGui.Spacing();
      ImGui.Separator();
      TrackedBool("循环挥砍 (游戏)", () => _slashEffectConfig.Looping, v => _slashEffectConfig.Looping = v, NotifySlash);
      TrackedFloat("挥砍间隔 (秒)", () => _slashEffectConfig.RestTime, v => _slashEffectConfig.RestTime = MathF.Max(0f, v), 0f, 5f, NotifySlash);
      TrackedInt("随机种子 (0=随机)", () => _slashEffectConfig.Seed, v => _slashEffectConfig.Seed = v, 0, 999999, NotifySlash);
    }

    private void BuildSlashRightTabs()
    {
      if (!ImGui.BeginTabBar("slashTabs"))
        return;

      if (ImGui.BeginTabItem("刀光本体"))
      {
        BuildSlashArcPanel();
        ImGui.EndTabItem();
      }
      if (ImGui.BeginTabItem("刃花"))
      {
        BuildSparkPanel();
        ImGui.EndTabItem();
      }
      ImGui.EndTabBar();
    }

    /// <summary>刀光本体参数: 阶段一 挥动 → 阶段二 收尾修饰器 → 纹理层 → 面片外观 → 整体坐标系.</summary>
    private void BuildSlashArcPanel()
    {
      SlashArcConfig cfg = _slashEffectConfig.Arc;

      // —— 阶段一 挥动 ——
      ImGui.Separator();
      ImGui.TextUnformatted("阶段一 挥动");
      TrackedFloat("挥扫时长 (秒)", () => cfg.SweepTime, v => cfg.SweepTime = MathF.Max(0.02f, v), 0.02f, 2f, NotifySlash);
      TrackedFloat("起始角 (度)", () => cfg.ArcFrom, v => cfg.ArcFrom = v, -360f, 360f, NotifySlash);
      TrackedFloat("结束角 (度)", () => cfg.ArcTo, v => cfg.ArcTo = v, -360f, 360f, NotifySlash);
      TrackedFloat("弧线半径 (像素)", () => cfg.Radius, v => cfg.Radius = v, 10f, 600f, NotifySlash);
      ImGui.TextUnformatted("挥扫速度曲线");
      ParticleTimeline.FloatCurveEditor("##sweepCurve", cfg.SweepCurve, new NV2(ImGui.GetContentRegionAvail().X, 110f), NotifySlash);
      ImGui.TextDisabled("纵轴: 相对速度 | 双击添加关键帧 | 拖拽 | 右键删除 (积分归一化, 时长内必完成)");

      // —— 阶段二 收尾 (修饰器, 可同时叠加) ——
      ImGui.Separator();
      ImGui.TextUnformatted("阶段二 收尾修饰器 (可同时叠加)");
      BuildFinisherToggles(cfg);
      SlashFadeFinish fade = cfg.Finishes.OfType<SlashFadeFinish>().FirstOrDefault();
      if (fade is not null)
      {
        TrackedFloat("渐隐时长 (秒)", () => fade.Duration, v => fade.Duration = Math.Clamp(v, 0.02f, 3f), 0.02f, 3f, NotifySlash);
        if (ImGui.TreeNodeEx("渐隐曲线 (纵轴: 渐隐速度)", ImGuiTreeNodeFlags.DefaultOpen))
        {
          ParticleTimeline.FloatCurveEditor("##fadeCurve", fade.Curve, new NV2(ImGui.GetContentRegionAvail().X, 90f), NotifySlash);
          ImGui.TreePop();
        }
      }
      SlashCollapseFinish collapse = cfg.Finishes.OfType<SlashCollapseFinish>().FirstOrDefault();
      if (collapse is not null)
      {
        TrackedFloat("收拢时长 (秒)", () => collapse.Duration, v => collapse.Duration = Math.Clamp(v, 0.02f, 3f), 0.02f, 3f, NotifySlash);
        if (ImGui.TreeNodeEx("收拢曲线 (纵轴: 收拢速度)", ImGuiTreeNodeFlags.DefaultOpen))
        {
          ParticleTimeline.FloatCurveEditor("##collapseCurve", collapse.Curve, new NV2(ImGui.GetContentRegionAvail().X, 90f), NotifySlash);
          ImGui.TreePop();
        }
      }

      // —— 纹理层 (可叠加) ——
      ImGui.Separator();
      ImGui.TextUnformatted("纹理层 (可叠加)");
      BuildLayerList(cfg);

      // —— 面片与外观 ——
      ImGui.Separator();
      ImGui.TextUnformatted("面片与外观");
      TrackedFloat("面片全宽 (像素)", () => cfg.Width, v => cfg.Width = MathF.Max(1f, v), 1f, 200f, NotifySlash);
      TrackedInt("采样段数", () => cfg.Segments, v => cfg.Segments = Math.Clamp(v, 4, 512), 4, 512, NotifySlash);
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

      // —— 整体坐标系 ——
      ImGui.Separator();
      ImGui.TextUnformatted("整体坐标系 (2D)");
      TrackedFloat("横向缩放 (长)", () => cfg.ScaleX, v => cfg.ScaleX = v, 0.1f, 4f, NotifySlash);
      TrackedFloat("纵向缩放 (宽)", () => cfg.ScaleY, v => cfg.ScaleY = v, 0.1f, 4f, NotifySlash);
      TrackedFloat("整体旋转 (度)", () => cfg.RotationDeg, v => cfg.RotationDeg = v, -360f, 360f, NotifySlash);
      ImGui.TextDisabled("刃花自动跟随本坐标系的前缘角度.");
    }

    /// <summary>收尾修饰器开关: 勾选即加入列表, 取消即移除 (走撤销命令).</summary>
    private void BuildFinisherToggles(SlashArcConfig cfg)
    {
      bool fadeOn = cfg.Finishes.OfType<SlashFadeFinish>().Any();
      bool fadeWanted = fadeOn;
      if (ImGui.Checkbox("渐隐收尾", ref fadeWanted) && fadeWanted != fadeOn)
        ToggleFinisher(cfg, new SlashFadeFinish(), fadeWanted, "渐隐收尾");

      ImGui.SameLine();
      bool collapseOn = cfg.Finishes.OfType<SlashCollapseFinish>().Any();
      bool collapseWanted = collapseOn;
      if (ImGui.Checkbox("收拢收尾", ref collapseWanted) && collapseWanted != collapseOn)
        ToggleFinisher(cfg, new SlashCollapseFinish(), collapseWanted, "收拢收尾");
    }

    private void ToggleFinisher(SlashArcConfig cfg, SlashFinishConfig finisher, bool add, string label)
    {
      SlashEffectConfig before = _slashEffectConfig.Clone();
      if (add)
        cfg.Finishes.Add(finisher);
      else
        cfg.Finishes.RemoveAll(f => f is not null && f.GetType() == finisher.GetType());
      SlashEffectConfig after = _slashEffectConfig.Clone();
      _history.Execute(new DelegateCommand(label,
        () => RestoreSlash(after),
        () => RestoreSlash(before)));
      NotifySlash();
    }

    /// <summary>纹理层列表: 逐层编辑 + 增删 (几何一次构建, 逐层叠加绘制).</summary>
    private void BuildLayerList(SlashArcConfig cfg)
    {
      for (int i = 0; i < cfg.Layers.Count; i++)
      {
        SlashTextureLayer layer = cfg.Layers[i];
        ImGui.PushID(i);
        string header = $"层 {i + 1}: {LayerTextureLabel(layer.Texture)}{(layer.Enabled ? "" : " (停用)")}";
        if (ImGui.TreeNodeEx(header, ImGuiTreeNodeFlags.DefaultOpen))
        {
          TrackedBool("启用", () => layer.Enabled, v => layer.Enabled = v, NotifySlash);
          TextureField("纹理", () => layer.Texture, v => layer.Texture = v, "##layerTex", embed: true);
          TrackedFloat("强度", () => layer.Intensity, v => layer.Intensity = Math.Clamp(v, 0f, 4f), 0f, 4f, NotifySlash);
          TrackedFloat("横向平铺", () => layer.UTiling, v => layer.UTiling = Math.Clamp(v, 0.25f, 8f), 0.25f, 8f, NotifySlash);
          TrackedFloat("横向偏移", () => layer.UOffset, v => layer.UOffset = Math.Clamp(v, -2f, 2f), -2f, 2f, NotifySlash);
          TrackedFloat("滚动速度 (u/秒)", () => layer.ScrollSpeed, v => layer.ScrollSpeed = Math.Clamp(v, -4f, 4f), -4f, 4f, NotifySlash);
          TrackedFloat("纵向缩放", () => layer.VScale, v => layer.VScale = Math.Clamp(v, 0.25f, 4f), 0.25f, 4f, NotifySlash);
          NV4 tint = new NV4(layer.Tint.X, layer.Tint.Y, layer.Tint.Z, layer.Tint.W);
          if (ImGui.ColorEdit4("层色调", ref tint))
            layer.Tint = new XnaVector4(tint.X, tint.Y, tint.Z, tint.W);
          if (ImGui.SmallButton("删除此层"))
          {
            SlashEffectConfig before = _slashEffectConfig.Clone();
            cfg.Layers.RemoveAt(i);
            SlashEffectConfig after = _slashEffectConfig.Clone();
            _history.Execute(new DelegateCommand("删除纹理层",
              () => RestoreSlash(after),
              () => RestoreSlash(before)));
            NotifySlash();
            ImGui.TreePop();
            ImGui.PopID();
            return;   // 列表已变更, 下帧重绘.
          }
          ImGui.TreePop();
        }
        ImGui.PopID();
      }

      if (ImGui.Button("＋ 添加纹理层"))
      {
        SlashEffectConfig before = _slashEffectConfig.Clone();
        cfg.Layers.Add(new SlashTextureLayer());
        SlashEffectConfig after = _slashEffectConfig.Clone();
        _history.Execute(new DelegateCommand("添加纹理层",
          () => RestoreSlash(after),
          () => RestoreSlash(before)));
        NotifySlash();
      }
      ImGui.SameLine();
      ImGui.TextDisabled("几何一次构建, 逐层叠加绘制");
    }

    private static string LayerTextureLabel(string texture)
    {
      if (texture is null)
        return "?";
      if (texture.StartsWith("embed:"))
        return "嵌入图 " + texture.Substring(6);
      if (texture.StartsWith("file:"))
        return Path.GetFileName(texture.Substring(5));
      return texture;
    }

    /// <summary>刃花参数 —— 与刀光前缘角度绑定, 沿弧当前位置/切向发射.</summary>
    private void BuildSparkPanel()
    {
      SlashSparkConfig s = _slashEffectConfig.Sparks;

      TrackedBool("启用刃花", () => s.Enabled, v => s.Enabled = v, NotifySlash);
      if (!s.Enabled)
      {
        ImGui.TextDisabled("刃花已关闭.");
        return;
      }

      TrackedFloat("发射率 (个/秒)", () => s.Rate, v => s.Rate = v, 0f, 2000f, NotifySlash);
      TrackedFloat("初速下限 (像素/秒)", () => s.SpeedMin, v => s.SpeedMin = v, 0f, 4000f, NotifySlash);
      TrackedFloat("初速上限 (像素/秒)", () => s.SpeedMax, v => s.SpeedMax = v, 0f, 4000f, NotifySlash);
      TrackedFloat("散布半角 (度)", () => s.SpreadDeg, v => s.SpreadDeg = v, 0f, 90f, NotifySlash);
      TrackedFloat("生命下限 (秒)", () => s.LifeMin, v => s.LifeMin = MathF.Max(0.01f, v), 0.01f, 3f, NotifySlash);
      TrackedFloat("生命上限 (秒)", () => s.LifeMax, v => s.LifeMax = MathF.Max(0.01f, v), 0.01f, 3f, NotifySlash);
      TrackedFloat("尺寸下限 (像素)", () => s.SizeMin, v => s.SizeMin = MathF.Max(0.1f, v), 0.1f, 32f, NotifySlash);
      TrackedFloat("尺寸上限 (像素)", () => s.SizeMax, v => s.SizeMax = MathF.Max(0.1f, v), 0.1f, 32f, NotifySlash);
      TrackedFloat("长宽比", () => s.Aspect, v => s.Aspect = MathF.Max(0.05f, v), 0.05f, 8f, NotifySlash);

      TrackedFloat("重力 X (像素/秒²)", () => s.Gravity.X, v => s.Gravity = new XnaVector2(v, s.Gravity.Y), -2000f, 2000f, NotifySlash);
      TrackedFloat("重力 Y (像素/秒²)", () => s.Gravity.Y, v => s.Gravity = new XnaVector2(s.Gravity.X, v), -2000f, 2000f, NotifySlash);
      TrackedFloat("线性阻尼 (1/秒)", () => s.Drag, v => s.Drag = v, 0f, 20f, NotifySlash);

      NV4 start = new NV4(s.StartColor.X, s.StartColor.Y, s.StartColor.Z, s.StartColor.W);
      if (ImGui.ColorEdit4("出生颜色", ref start))
        s.StartColor = new XnaVector4(start.X, start.Y, start.Z, start.W);
      NV4 end = new NV4(s.EndColor.X, s.EndColor.Y, s.EndColor.Z, s.EndColor.W);
      if (ImGui.ColorEdit4("消亡颜色", ref end))
        s.EndColor = new XnaVector4(end.X, end.Y, end.Z, end.W);

      TrackedBool("速度拉伸", () => s.Stretched, v => s.Stretched = v, NotifySlash);
      TrackedInt("槽位上限", () => s.Capacity, v => s.Capacity = v, 8, 4096, NotifySlash);
      TextureField("刃花纹理 (粒子)", () => s.Texture, v => s.Texture = v, "##texSpark", embed: true);

      ImGui.TextDisabled("刃花沿刀光当前前缘角度发射 (角度绑定).");
    }

    private void NotifySlash() => _slashEffectConfig?.NotifyChanged();

    // =====================================================================
    //  时间轴窗口 (粒子模式)
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
      if (ImGui.Combo("曲线插值", ref interp, "线性折线  平滑 (SmoothStep)  平滑样条 (Catmull-Rom) "))
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

    /// <summary>纹理名输入 + 浏览按钮 (同一行, 输入框宽度自适应避免按钮被裁剪).
    /// embed = true 时浏览的图片直接嵌入刀光预制件 (PNG base64, "embed:键") ——
    /// 保存的预制件自带贴图, 玩家机器无需原文件; 否则记为 "file:路径" (游戏资产/本地调试).</summary>
    private void TextureField(string label, Func<string> get, Action<string> set, string browseId, bool embed = false)
    {
      const string browseLabel = "浏览";
      ImGuiStylePtr style = ImGui.GetStyle();
      float labelWidth = ImGui.CalcTextSize(label).X + style.FramePadding.X;
      float buttonWidth = ImGui.CalcTextSize(browseLabel).X + style.FramePadding.X * 2f;
      float inputWidth = MathF.Max(60f, ImGui.GetContentRegionAvail().X - labelWidth - buttonWidth - style.ItemSpacing.X);

      ImGui.SetNextItemWidth(inputWidth);
      TrackedString(label, get, set, CurrentNotify);
      ImGui.SameLine();
      if (ImGui.SmallButton(browseLabel + browseId))
      {
        string file = PickTexture();
        if (file is not null)
        {
          set(embed ? ImportTexture(file) : "file:" + file);
          CurrentNotify();
        }
      }
    }

    /// <summary>把图片文件嵌入当前刀光预制件 (base64), 返回 "embed:键" 纹理名;
    /// 立即注册进渲染器缓存以供预览.</summary>
    private string ImportTexture(string file)
    {
      try
      {
        byte[] png = File.ReadAllBytes(file);
        string key = "tex" + _slashEffectConfig.EmbeddedTextures.Count;
        _slashEffectConfig.EmbeddedTextures[key] = Convert.ToBase64String(png);
        Particle.Rendering.ParticleRenderer.Shared?.LoadTextureBytes("embed:" + key, png);
        return "embed:" + key;
      }
      catch (Exception exception)
      {
        Console.WriteLine("Error", "嵌入纹理失败 (" + file + "): " + exception.Message);
        return "blade";
      }
    }

    /// <summary>按当前编辑对象路由变更通知 (TrackedX 与纹理浏览共用).</summary>
    private void CurrentNotify()
    {
      if (_mode == EditorMode.Particle)
        NotifyAll();
      else
        NotifySlash();
    }

    private void TrackedFloat(string label, Func<float> get, Action<float> set, float min, float max, Action onChanged)
    {
      float value = get();
      // 拖动粗调; 双击 (或 Ctrl+点击) 数值框可键入精确数值 —— 大范围参数 (如收拢速度) 靠拖动难以控制.
      float speed = MathF.Max((max - min) / 400f, 0.01f);
      if (ImGui.DragFloat(label, ref value, speed, min, max, "%.2f"))
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
      if (ImGui.InputText(label, ref value, 256))
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
    //  文件对话框 (WinForms; 打开期间挂起编辑器以防 ImGui 帧重入)
    // =====================================================================
    private string PickTexture()
    {
      using System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
      {
        Filter = "图片 (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|全部文件 (*.*)|*.*",
        Title = "选择纹理"
      };
      return RunFileDialog(dialog);
    }

    private string PickFile(bool open)
    {
      if (open)
      {
        using System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
        {
          Filter = "配置 (*.json)|*.json|全部文件 (*.*)|*.*",
          Title = "打开配置"
        };
        return RunFileDialog(dialog);
      }
      else
      {
        using System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
        {
          Filter = "配置 (*.json)|*.json",
          Title = "保存配置"
        };
        return RunFileDialog(dialog);
      }
    }

    /// <summary>以模态挂起标志运行文件对话框: 对话框的嵌套消息循环会重入游戏主循环,
    /// 标志使 DoUpdate/DoRawRender 在期间整体跳过, 杜绝 ImGui 帧重入与点击穿透.</summary>
    private string RunFileDialog(System.Windows.Forms.FileDialog dialog)
    {
      _modalDialogOpen = true;
      try
      {
        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
      }
      finally
      {
        _modalDialogOpen = false;
      }
    }
  }
}
