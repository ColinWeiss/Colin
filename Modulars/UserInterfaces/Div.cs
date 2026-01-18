using Colin.Core.Modulars.UserInterfaces.Events;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 指代用户交互界面中的一个划分元素.
  /// </summary>
  public class Div : IDisposable
  {
    /// <summary>
    /// 划分元素的名称.
    /// </summary>
    public readonly string Name;

    private bool isVisible = true;
    /// <summary>
    /// 指示划分元素是否可见.
    /// </summary>
    public bool IsVisible
    {
      get => isVisible;
      set
      {
        ForEach(a => a.IsVisible = value);
        isVisible = value;
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

    private bool isHidden = false;
    public bool IsHidden
    {
      get => isHidden;
      set
      {
        ForEach(a => a.isHidden = value);
        isHidden = value;
      }
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

    /// <summary>
    /// 计算后的剪裁矩形; 当前剪裁矩形.
    /// </summary>
    public Rectangle ScissorBounds;
    public void CalculateScissorBounds()
    {
      if (UpperScissor == null || UpperScissor == this)
      {
        ScissorBounds = Layout.ScissorRectangle;
      }
      else
      {
        var parentBounds = UpperScissor.ScissorBounds;
        var current = Layout.ScissorRectangle;
        ScissorBounds = Rectangle.Intersect(parentBounds, current);
        if (ScissorBounds.IsEmpty)
          ScissorBounds = parentBounds;
      }
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
      if (!IsVisible)
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
      LayoutCalculate(ref Layout);
      LayoutEvent?.Invoke(this);
      InteractCalculate(ref Interact);
      DesignCalculate(ref Design);
      DivLayout.Calculate(this);
      CalculateScissorBounds();
      Events.DoUpdate();
      OnUpdate(time);
      UpdateChildren(time);
    }

    public virtual void LayoutCalculate(ref DivLayout layout) { }

    public virtual void InteractCalculate(ref InteractStyle interact) { }

    public virtual void DesignCalculate(ref DivDesign design) { }

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
      ForEach(child =>
      {
        if (child.Layout.RenderTargetBounds.Intersects(Layout.RenderTargetBounds))
          child?.DoUpdate(time);
        else
          child?.DoUpdate(time);
      });
    }

    public event Action<Div> LayoutEvent;

    private static RasterizerState ScissiorRasterizer = new RasterizerState()
    {
      CullMode = CullMode.None,
      ScissorTestEnable = true,
    };

    public void BeginRender(BlendState blendState, SamplerState samplerState)
    {
      if (UpperScissor is not null)
      {
        UpperScissor.Layout.ScissorRectangleCache = CoreInfo.Graphics.GraphicsDevice.ScissorRectangle; //针对剪裁测试进行剪裁矩形暂存
        CoreInfo.Graphics.GraphicsDevice.ScissorRectangle = ScissorBounds;
        CoreInfo.Batch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointWrap, null, ScissiorRasterizer, transformMatrix: UpperCanvas is null ? Module.UICamera.View : null);
      }
      else
        Module.BatchNormalBegin(this, blendState);
    }

    /// <summary>
    /// 执行划分元素的渲染.
    /// </summary>
    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (!IsVisible && !IsHidden)
        return;
      if (IsCanvas)
      {
        device.SetRenderTarget(Canvas);
        device.Clear(Color.Transparent);
      }
      BeginRender(BlendState.AlphaBlend, SamplerState.PointWrap);
      OnRender(device, batch);
      _renderer?.DoRender(device, batch);//渲染器进行渲染.
      batch.End();
      if (UpperScissor is not null)
        device.ScissorRectangle = UpperScissor.Layout.ScissorRectangleCache;
      RenderChildren(device, batch);
      if (IsCanvas)
      {
        if (UpperCanvas is not null)
          device.SetRenderTarget(UpperCanvas.Canvas);
        else
          device.SetRenderTarget(Module.RawRt);
        Vector2 canvasRenderPos = Layout.ScreenLocation + Layout.Anchor;
        if (UpperCanvas is not null)
          canvasRenderPos = Layout.RenderTargetLocation + Layout.Anchor;
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, _root.Layout.ScissorEnable ? ScissiorRasterizer : null, transformMatrix: UpperCanvas is not null ? null : Module.UICamera.View);
        batch.Draw(Canvas, canvasRenderPos, null, Design.Color, 0f, Layout.Anchor, Layout.Scale, SpriteEffects.None, 0f);
        batch.End();
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
      ForEach(child =>
      {
        child?.DoRender(device, batch);
      });
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
      Events.Register(div.Events);
      if (doInit)
        div.DoInitialize();
      Children.Add(div);
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
      Events.Remove(div.Events);
      return Children.Remove(div);
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
    /// 检查该划分元素是否为某个划分元素的后代.
    /// </summary>
    /// <param name="div"></param>
    /// <returns></returns>
    public bool DescendantsOf(Div div)
    {
      if (div._parent is not null)
      {
        if (div._parent.Equals(div))
          return true;
        else
          return div._parent.DescendantsOf(div);
      }
      return false;
    }

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