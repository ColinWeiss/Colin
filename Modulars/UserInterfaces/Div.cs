using Colin.Core.Modulars.UserInterfaces.Events;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 指示划分元素布局的归属方.
  /// </summary>
  public enum DivLayoutOwnership
  {
    /// <summary>
    /// 未声明: 排列控制器/预制件可在运行期把自动标记升级为 <see cref="Scripted"/>.
    /// </summary>
    Auto,
    /// <summary>
    /// 代码接管: 布局被逐帧回写, 编辑器锁定移动/缩放 (拖了会被弹回).
    /// </summary>
    Scripted,
    /// <summary>
    /// 手动布局: 排列控制器跳过该子级 (不计入尺寸也不摆放), 编辑器自由手 K, 以编辑器/文档为准.
    /// </summary>
    Manual,
  }

  /// <summary>
  /// 指代用户交互界面中的一个划分元素.
  /// </summary>
  public class Div : IDisposable
  {
    /// <summary>
    /// 划分元素的名称.
    /// <br>于文档加载后可经 <see cref="Rename"/> 修改.</br>
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 重命名划分元素.
    /// </summary>
    public void Rename(string name)
    {
      if (string.IsNullOrEmpty(name))
        return;
      Name = name;
    }

    private bool isVisible = true;
    /// <summary>
    /// 指示划分元素是否可见.
    /// </summary>
    public bool IsVisible
    {
      get => isVisible;
      set
      {
        //遍历改用 for 循环而非 ForEach 委托, 避免逐帧闭包分配.
        List<Div> children = Children;
        for (int count = 0; count < children.Count; count++)
          children[count].IsVisible = value;
        isVisible = value;
      }
    }

    /// <summary>
    /// 指示划分元素是否被裁剪剔除.
    /// <br>被剔除的元素不更新、不渲染、不响应事件; 与 <see cref="IsVisible"/> 相互独立, 不会被可见性级联重置 —— 供滚动视口虚拟化等外部批量剔除使用.</br>
    /// </summary>
    public bool IsCulled = false;

    /// <summary>
    /// 指示本帧是否被线性排列控制器摆放 (编辑器展示与交互判定用, 逐帧刷新).
    /// <br>拖拽/缩放这类元素时编辑器会自动把它转为 <see cref="DivLayoutOwnership.Manual"/> —— 拖动即脱离排列流, 与样式烘焙同一手势.</br>
    /// </summary>
    public bool LayoutArranged;

    /// <summary>
    /// 布局归属声明 (见 <see cref="DivLayoutOwnership"/>).
    /// <br><see cref="DivLayoutOwnership.Manual"/> 声明后排列控制器不再摆它 —— 这是把代码固定布局迁入文档/编辑器手 K 的出口.</br>
    /// <br>约定: 把元素标成 <see cref="DivLayoutOwnership.Scripted"/> 的类型必须自行保证尺寸 (自设或支持 Auto 测量).</br>
    /// </summary>
    public DivLayoutOwnership LayoutOwnership { get; set; } = DivLayoutOwnership.Auto;

    /// <summary>
    /// 布局是否由代码接管; 即 <see cref="LayoutOwnership"/> 为 <see cref="DivLayoutOwnership.Scripted"/>.
    /// <br>置真等效于声明 <see cref="DivLayoutOwnership.Scripted"/>; 置假回落到 <see cref="DivLayoutOwnership.Auto"/> (不动已声明的 Manual).</br>
    /// </summary>
    public bool LayoutScripted
    {
      get => LayoutOwnership == DivLayoutOwnership.Scripted;
      set
      {
        if (LayoutOwnership == DivLayoutOwnership.Manual)
          return;
        LayoutOwnership = value ? DivLayoutOwnership.Scripted : DivLayoutOwnership.Auto;
      }
    }

    public event Action OnDoWakeUpStart;

    public event Action OnDoHibernateStart;

    public void DoWakeUp()
    {
      OnDoWakeUpStart?.Invoke();
      if (Controller is null)
        IsVisible = true;
      else
        Controller.DoWakeUp(this);
    }

    public void DoHibernate()
    {
      OnDoHibernateStart?.Invoke();
      if (Controller is null)
        IsVisible = false;
      else
        Controller.DoHibernate(this);
    }

    /// <summary>
    /// 用于存放该划分元素的子元素.
    /// </summary>
    public List<Div> Children;

    public Div GetChild(string name)
    {
      return Children.Find(a => a.Name == name);
    }

    /// <summary>
    /// 指示划分元素的布局样式
    /// </summary>
    public DivLayout Layout;

    /// <summary>
    /// 划分元素的布局样式.
    /// <br>声明式锚定 / 停靠 / 百分比 / 自适应布局; 默认全部关闭, 不影响手动布局.</br>
    /// </summary>
    public DivLayoutStyle Style;

    /// <summary>
    /// 划分元素的交互样式.
    /// </summary>
    public InteractStyle Interact;

    /// <summary>
    /// 划分元素的设计样式.
    /// </summary>
    public DivDesign Design;

    public DivEvents Events;

    private DivRenderer _renderer;
    /// <summary>
    /// 获取划分元素的渲染器实例对象.
    /// </summary>
    public DivRenderer Renderer => _renderer;
    public T BindRenderer<T>() where T : DivRenderer, new()
    {
      _renderer = new T();
      _renderer.div = this;
      _renderer.OnBinded();
      if (InitializationCompleted)
        _renderer.OnDivInitialize();
      return _renderer as T;
    }

    /// <summary>
    /// 绑定一个已构造好的渲染器实例 (文档加载与编辑器替换渲染器用).
    /// <br>生命周期与 <see cref="BindRenderer{T}"/> 一致: 立即 <see cref="DivRenderer.OnBinded"/>, 已完成初始化的元素补发 <see cref="DivRenderer.OnDivInitialize"/>.</br>
    /// </summary>
    public T BindRenderer<T>(T renderer) where T : DivRenderer
    {
      _renderer = renderer;
      _renderer.div = this;
      _renderer.OnBinded();
      if (InitializationCompleted)
        _renderer.OnDivInitialize();
      return _renderer as T;
    }
    public T GetRenderer<T>() where T : DivRenderer
    {
      if (_renderer is T)
        return _renderer as T;
      else
        return null;
    }
    public void ClearRenderer() => _renderer = null;

    private DivController _controller;
    /// <summary>
    /// 获取划分元素的控制器实例对象.
    /// </summary>
    public DivController Controller => _controller;
    public T BindController<T>() where T : DivController, new()
    {
      _controller = new T();
      _controller.OnBinded(this);
      if (InitializationCompleted)
        _controller.OnDivInitialize(this);
      return _controller as T;
    }

    /// <summary>
    /// 绑定一个已构造好的控制器实例 (文档加载与编辑器替换控制器用).
    /// </summary>
    public T BindController<T>(T controller) where T : DivController
    {
      _controller = controller;
      _controller.OnBinded(this);
      if (InitializationCompleted)
        _controller.OnDivInitialize(this);
      return _controller as T;
    }
    public T GetController<T>() where T : DivController
    {
      if (_controller is T)
        return _controller as T;
      else
        return null;
    }

    private Div _parent;
    /// <summary>
    /// 指示划分元素的父元素.
    /// </summary>
    public Div Parent => _parent;

    private Div _upperCanvas;
    /// <summary>
    /// 获取划分元素可溯到的最近的上一层画布元素.
    /// </summary>
    public Div UpperCanvas
    {
      get
      {
        if (_parent is null)
          return null;
        if (_upperCanvas is null)
        {
          Div result;
          if (Parent.IsCanvas)
            result = Parent;
          else
            result = Parent.UpperCanvas;
          _upperCanvas = result;
        }
        return _upperCanvas;
      }
    }

    private Div _upperScissor;
    public Div UpperScissor
    {
      get
      {
        if (_parent is null || IsCanvas)
          return null;
        if (_upperScissor is null)
        {
          Div result;
          if (Parent.Layout.ScissorEnable && !Parent.IsCanvas)
            result = Parent;
          else
            result = Parent.UpperScissor;
          _upperScissor = result;
        }
        return _upperScissor;
      }
    }

    private Div _upperBatch;
    public Div UpperBatch
    {
      get
      {
        if (_parent is null)
          return null;
        if (_upperBatch is null)
        {
          Div result;
          if (Parent.UseBatch)
            result = Parent;
          else
            result = Parent.UpperBatch;
          _upperBatch = result;
        }
        return _upperBatch;
      }
    }

    /// <summary>
    /// 计算后的剪裁矩形; 当前剪裁矩形.
    /// </summary>
    public Rectangle ScissorBounds;

    /// <summary>
    /// 计算当前剪裁矩形.
    /// <br>未启用自身剪裁时沿用最近剪裁祖先的区域; 启用时与祖先区域取交集.</br>
    /// </summary>
    public void CalculateScissorBounds()
    {
      if (UpperScissor is null)
        ScissorBounds = Layout.ScissorRectangle;
      else if (Layout.ScissorEnable is false)
        ScissorBounds = UpperScissor.ScissorBounds;
      else
        ScissorBounds = Rectangle.Intersect(UpperScissor.ScissorBounds, Layout.ScissorRectangle);
    }

    /// <summary>
    /// 若<see cref="IsCanvas"/> 为 <see langword="true"/>, 则该对象用于分配其渲染目标. 
    /// <br>否则为 <see langword="null"/>.</br>
    /// </summary>
    public RenderTarget2D Canvas;

    internal UserInterface _module;
    /// <summary>
    /// 获取划分元素所属的用户交互界面.
    /// </summary>
    public UserInterface Module => _module;

    internal DivRoot _root;
    /// <summary>
    /// 获取划分元素之「阈点」.
    /// </summary>
    public DivRoot Root => _root;

    /// <summary>
    /// 指示该划分元素是否为可作为渲染目标的画布元素.
    /// </summary>
    public readonly bool IsCanvas;

    public bool InitializationCompleted = false;

    /// <summary>
    /// 实例化一个划分元素, 并用名称加以区分.
    /// <br>[!] 虽然此处的名称可重复, 但该名称的作用是利于调试, 故建议使用不同的、可辨识的名称加以区分.</br>
    /// </summary>
    /// <param name="name">划分元素的名称.</param>
    public Div(string name, bool isCanvas = false)
    {
      Name = name;
      Children = new List<Div>();
      Events = new DivEvents(this);
      Events.DoBlockOut();
      Layout.Scale = Vector2.One;
      Interact.IsInteractive = true;
      Design.Color = Color.White;
      IsCanvas = isCanvas;
    }

    public void SetCanvas(float width, float height)
    {
      Layout.Width = width;
      Layout.Height = height;
      Layout.Anchor = new Vector2(Layout.Width / 2, Layout.Height / 2);
      Canvas?.Dispose();
      Canvas = RenderTargetExt.CreateDefault((int)width, (int)height);
    }

    /// <summary>
    /// 确保画布渲染目标与布局尺寸一致; 布局尺寸变化 (如经布局样式拉伸) 时重建渲染目标.
    /// </summary>
    private void EnsureCanvasSize()
    {
      int width = (int)MathF.Max(1f, Layout.Width);
      int height = (int)MathF.Max(1f, Layout.Height);
      if (Canvas is null || Canvas.Width != width || Canvas.Height != height)
        SetCanvas(width, height);
    }

    public void DoInitialize()
    {
      if (this is DivRoot divThreshold)
        _root = divThreshold;
      if (Parent is not null)
      {
        _module = _parent._module;
        _root = _parent._root;
      }
      if (InitializationCompleted)
        return;
      DivInit();
      _controller?.OnDivInitialize(this);
      _renderer?.OnDivInitialize();
      DivLayout.Calculate(this);
      if (IsCanvas)
      {
        if (Layout.Width == 0)
          Layout.Width = 1;
        if (Layout.Height == 0)
          Layout.Height = 1;
        SetCanvas(Layout.Width, Layout.Height);
      }
      ForEach(child => child?.DoInitialize());
      InitializationCompleted = true;
    }

    /// <summary>
    /// 发生于划分元素执行 <see cref="DoInitialize"/> 时, 可于此自定义初始化操作.
    /// </summary>
    public virtual void DivInit() { }

    private bool _started = false;

    /// <summary>
    /// 执行划分元素的逻辑刷新.
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
    public void DoUpdate(GameTime time)
    {
      PreUpdate(time);
      if (IsVisible is false || IsCulled)
        return;
      if (this is DivRoot is false)
      {
        _module = Parent?._module;
        _root = Parent?._root;
      }
      if (!_started)
      {
        Start(time);
        _started = true;
      }
      Controller?.Layout(this, ref Layout);
      Controller?.Interact(this, ref Interact);
      Controller?.Design(this, ref Design);
      Style.Apply(this, Parent);
      LayoutCalculate(ref Layout);
      LayoutEvent?.Invoke(this);
      InteractCalculate(ref Interact);
      DesignCalculate(ref Design);
      DivLayout.Calculate(this);
      if (IsCanvas)
        EnsureCanvasSize();
      CalculateScissorBounds();
      Events.DoUpdate();
      OnUpdate(time);
      UpdateChildren(time);
    }

    public virtual void LayoutCalculate(ref DivLayout layout) { }

    public virtual void InteractCalculate(ref InteractStyle interact) { }

    public virtual void DesignCalculate(ref DivDesign design) { }

    /// <summary>
    /// 测量划分元素所期望的内容尺寸.
    /// <br>于布局样式 (见 <see cref="DivLayoutStyle"/>) 使用 <see cref="DivSizeMode.Auto"/> 尺寸模式时被布局系统调用.</br>
    /// <br>解析优先级: 渲染器内容测量 → 控制器内容测量 → 子级覆盖范围聚合 (容器包裹内容) → 当前尺寸. 可于子类重写以自定义测量逻辑.</br>
    /// </summary>
    public virtual Vector2 MeasureDesired()
    {
      if (_renderer is not null)
      {
        Vector2 desired = _renderer.MeasureContent();
        if (desired.X >= 0f && desired.Y >= 0f)
          return desired;
      }
      if (_controller is not null)
      {
        Vector2 desired = _controller.MeasureContent(this);
        if (desired.X >= 0f && desired.Y >= 0f)
          return desired;
      }
      if (Children.Count > 0)
      {
        //容器聚合缓存: 子级结构未变且无待算/新算布局时直接复用上一帧结果.
        List<Div> children = Children;
        bool valid = _desiredValid && _desiredCount == children.Count && _desiredStructure == _structureVersion;
        if (valid)
        {
          for (int count = 0; count < children.Count; count++)
          {
            Div child = children[count];
            if (child.Layout.IsDirty || child.Layout.TransformStamp > _desiredMaxStamp)
            {
              valid = false;
              break;
            }
          }
        }
        if (valid is false)
        {
          //期望尺寸为全部子级于相对坐标下向右/下方向的最大延伸.
          float right = 0f, bottom = 0f;
          uint maxStamp = 0;
          for (int count = 0; count < children.Count; count++)
          {
            Div child = children[count];
            if (child.Layout.Right > right)
              right = child.Layout.Right;
            if (child.Layout.Bottom > bottom)
              bottom = child.Layout.Bottom;
            if (child.Layout.TransformStamp > maxStamp)
              maxStamp = child.Layout.TransformStamp;
          }
          _desiredCache = new Vector2(right, bottom);
          _desiredMaxStamp = maxStamp;
          _desiredCount = children.Count;
          _desiredStructure = _structureVersion;
          _desiredValid = true;
        }
        return _desiredCache;
      }
      return new Vector2(Layout.Width, Layout.Height);
    }

    private Vector2 _desiredCache;
    private bool _desiredValid;
    private uint _desiredMaxStamp;
    private int _desiredCount;
    private uint _desiredStructure;

    private uint _structureVersion;

    /// <summary>
    /// 发生于 <see cref="DoUpdate(GameTime)"/> 第一帧执行时.
    /// </summary>
    public virtual void Start(GameTime time) { }
    /// <summary>
    /// 发生于 <see cref="DoUpdate"/> 执行时, 但不受 <see cref="IsVisible"/> 控制.
    /// <br>相较于 <see cref="UpdateChildren"/> 与 <see cref="OnUpdate"/> 最先执行.</br>
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
    public virtual void PreUpdate(GameTime time) { }
    /// <summary>
    /// 发生于 <see cref="DoUpdate"/> 执行时, 受 <see cref="IsVisible"/> 控制.
    /// <br>相较于 <see cref="UpdateChildren"/> 更快执行.</br>
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
    public virtual void OnUpdate(GameTime time) { }
    /// <summary>
    /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoUpdate"/>.
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
    public virtual void UpdateChildren(GameTime time)
    {
      //遍历改用 for 循环而非 ForEach 委托, 避免每帧每元素一次的闭包分配.
      List<Div> children = Children;
      for (int count = 0; count < children.Count; count++)
        children[count]?.DoUpdate(time);
    }

    public event Action<Div> LayoutEvent;

    /// <summary>
    /// 请求以指定混合状态开始绘制.
    /// <br>实际批次由 <see cref="UIBatch"/> 状态机管理: 请求状态与激活状态一致时不会重新开启批次.</br>
    /// </summary>
    public void BeginRender(BlendState blendState, SamplerState samplerState, SpriteSortMode mode = SpriteSortMode.Deferred)
    {
      if (UpperBatch is not null)
        return; //位于批次容器内: 直接绘制于上级批次, 不自行开启.
      Matrix transform = UpperCanvas is null ? Module.UICamera.View : Matrix.Identity;
      if (UpperScissor is not null)
        UIBatch.Request(blendState, samplerState, DepthStencilState.Default, UIBatch.ScissorTestRasterizer, ScissorBounds, transform);
      else
        UIBatch.Request(blendState, samplerState, DepthStencilState.None, UIBatch.DefaultRasterizer, default, transform);
    }

    public bool UseBatch = false;

    /// <summary>
    /// 执行划分元素的渲染.
    /// </summary>
    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (IsVisible is false || IsCulled)
        return;
      if (IsCanvas)
      {
        //切换渲染目标前提交既有批次内容.
        UIBatch.Flush();
        device.SetRenderTarget(Canvas);
        device.Clear(Color.Transparent);
      }

      BeginRender(BlendState.AlphaBlend, SamplerState.PointWrap);
      OnRender(device, batch);
      _renderer?.DoRender(device, batch);//渲染器进行渲染.
      RenderChildren(device, batch);
      if (UseBatch)
        UIBatch.Flush(); //批次容器: 子树绘制完毕后一并提交.
      if (IsCanvas)
      {
        //切回上级渲染目标, 将画布内容合成绘制; 提交顺序保持与绘制顺序一致.
        UIBatch.Flush();
        if (UpperCanvas is not null)
          device.SetRenderTarget(UpperCanvas.Canvas);
        else
          device.SetRenderTarget(Module.RawRt);
        Vector2 canvasRenderPos = Layout.ScreenLocation + Layout.Anchor;
        if (UpperCanvas is not null)
          canvasRenderPos = Layout.RenderTargetLocation + Layout.Anchor;
        bool rootScissor = _root.Layout.ScissorEnable;
        UIBatch.Request(
          BlendState.AlphaBlend, SamplerState.PointWrap,
          DepthStencilState.None,
          rootScissor ? UIBatch.ScissorTestRasterizer : UIBatch.DefaultRasterizer,
          rootScissor ? _root.ScissorBounds : default,
          UpperCanvas is not null ? Matrix.Identity : Module.UICamera.View);
        batch.Draw(Canvas, canvasRenderPos, null, Design.Color, 0f, Layout.Anchor, Layout.Scale, SpriteEffects.None, 0f);
      }
    }

    public virtual void OnRender(GraphicsDevice device, SpriteBatch batch)
    {

    }

    /// <summary>
    /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoRender"/>.
    /// </summary>
    public virtual void RenderChildren(GraphicsDevice device, SpriteBatch batch)
    {
      //遍历改用 for 循环而非 ForEach 委托, 避免每帧每元素一次的闭包分配.
      List<Div> children = Children;
      for (int count = 0; count < children.Count; count++)
        children[count]?.DoRender(device, batch);
    }

    public void ForEach(Action<Div> action)
    {
      Div _div;
      for (int count = 0; count < Children.Count; count++)
      {
        _div = Children[count];
        action.Invoke(_div);
      }
    }

    /// <summary>
    /// 添加子元素.
    /// </summary>
    /// <param name="div">需要添加的划分元素.</param>
    /// <param name="doInit">执行添加划分元素的初始化.</param>
    /// <returns>若添加成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public virtual bool Register(Div div, bool doInit = false)
    {
      div._parent = this;
      div._module = _module;
      div._root = _root;
      div.Layout.Invalidate(); //重挂载后强制重算, 防止版本戳巧合时沿用旧父级的布局结果.
      Events.Register(div.Events);
      if (doInit)
        div.DoInitialize();
      Children.Add(div);
      _structureVersion++;
      return true;
    }

    /// <summary>
    /// 于子级序列的指定位置插入子元素 (层级编辑器重排用).
    /// </summary>
    public virtual bool Insert(int index, Div div, bool doInit = false)
    {
      div._parent = this;
      div._module = _module;
      div._root = _root;
      div.Layout.Invalidate();
      index = Math.Clamp(index, 0, Children.Count);
      Events.Insert(index, div.Events);
      if (doInit)
        div.DoInitialize();
      Children.Insert(index, div);
      _structureVersion++;
      return true;
    }

    /// <summary>
    /// 移除子元素.
    /// </summary>
    /// <param name="div">需要移除的划分元素.</param>
    /// <returns>若移除成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public virtual bool Remove(Div div)
    {
      div._parent = null;
      div._root = null;
      div._upperScissor = null;
      div._upperBatch = null;
      div._upperCanvas = null;
      Events.Remove(div.Events);
      bool removed = Children.Remove(div);
      if (removed)
        _structureVersion++;
      return removed;
    }

    /// <summary>
    /// 移除所有子元素.
    /// </summary>
    public virtual void Clear(bool dispose = true)
    {
      Div _div;
      for (int count = 0; count < Children.Count; count++)
      {
        _div = Children[count];
        Remove(_div);
        if (dispose)
        {
          _div._module = null;
          _div._root = null;
          _div.Dispose();
        }
        count--;
      }
      //Clear();
    }

    public void SetTop(Div division)
    {
      Remove(division);
      Register(division);
    }

    public void Do(Action<Div> action) => action(this);

    /// <summary>
    /// 判断该划分元素是否包含屏幕上的指定点.
    /// </summary>
    /// <param name="point">输入的点.</param>
    /// <returns>如果包含则返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public virtual bool ContainsScreenPoint(Point point)
    {
      if (this == Root)
        return true;
      else
        return Layout.Bounds.Contains(point);
    }

    public bool ContainsScreenPoint(Vector2 pos)
      => ContainsScreenPoint(pos.ToPoint());

    public Vector2 MousePos => Module.UICamera.ConvertToWorld(MouseResponder.Position);

    public Vector2 RelativeMousePos => MousePos - Layout.ScreenLocation;

    public Vector2 RelativeRenderMousePos => MousePos - Layout.RenderTargetLocation;

    public event Action OnDispose;

    public bool Disposed = false;

    public virtual void Dispose()
    {
      Disposed = true;
      _parent = null;
      _renderer = null;
      _controller = null;
      _module = null;
      _root = null;
      _upperCanvas = null;
      _upperScissor = null;
      Events?.Dispose();
      Canvas?.Dispose();
      for (int count = 0; count < Children.Count; count++)
        Children[count].Dispose();
      OnDispose?.Invoke();
    }
  }
}