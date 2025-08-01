﻿using Colin.Core.Modulars.UserInterfaces.Events;
using Colin.Core.Modulars.UserInterfaces.Prefabs;
using System.Diagnostics;
using System.Windows.Forms;

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

    public int Order;

    /// <summary>
    /// 用于存放该划分元素的子元素.
    /// </summary>
    public List<Div> Children;

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

    private DivRenderer renderer;
    /// <summary>
    /// 获取划分元素的渲染器实例对象.
    /// </summary>
    public DivRenderer Renderer => renderer;
    public T BindRenderer<T>() where T : DivRenderer, new()
    {
      renderer = new T();
      renderer.div = this;
      renderer.OnBinded();
      return renderer as T;
    }
    public T GetRenderer<T>() where T : DivRenderer
    {
      if (renderer is T)
        return renderer as T;
      else
        return null;
    }
    public void ClearRenderer() => renderer = null;

    private DivController controller;
    /// <summary>
    /// 获取划分元素的控制器实例对象.
    /// </summary>
    public DivController Controller => controller;
    public T BindController<T>() where T : DivController, new()
    {
      controller = new T();
      controller.div = this;
      controller.OnBinded();
      return controller as T;
    }
    public T GetController<T>() where T : DivController
    {
      if (controller is T)
        return controller as T;
      else
        return null;
    }

    private Div parent;
    /// <summary>
    /// 指示划分元素的父元素.
    /// </summary>
    public Div Parent => parent;

    private Div upperCanvas;
    /// <summary>
    /// 获取划分元素可溯到的最近的上一层画布元素.
    /// </summary>
    public Div UpperCanvas
    {
      get
      {
        if (parent is null)
          return null;
        if (upperCanvas is null)
        {
          Div result;
          if (Parent.IsCanvas)
            result = Parent;
          else
            result = Parent.UpperCanvas;
          upperCanvas = result;
        }
        return upperCanvas;
      }
    }

    private Div upperScissor;
    public Div UpperScissor
    {
      get
      {
        if (parent is null)
          return null;
        if (upperScissor is null)
        {
          Div result;
          if (Parent.Layout.ScissorEnable)
            result = Parent;
          else
            result = Parent.UpperScissor;
          upperScissor = result;
        }
        return upperScissor;
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

    internal DivRoot root;
    /// <summary>
    /// 获取划分元素之「阈点」.
    /// </summary>
    public DivRoot Root => root;

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

    public void DoInitialize()
    {
      if (this is DivRoot divThreshold)
        root = divThreshold;
      if (Parent is not null)
      {
        _module = parent._module;
        root = parent.root;
      }
      if (InitializationCompleted)
        return;
      DivInit();
      controller?.OnDivInitialize();
      renderer?.OnDivInitialize();
      DivLayout.Calculate(this);
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
        root = Parent?.root;
      }
      if (!_started)
      {
        Start(time);
        _started = true;
      }
      Controller?.Layout(ref Layout);
      Controller?.Interact(ref Interact);
      Controller?.Design(ref Design);
      LayoutCalculate(ref Layout);
      InteractCalculate(ref Interact);
      DesignCalculate(ref Design);
      DivLayout.Calculate(this);
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

    public RasterizerState ScissiorRasterizer = new RasterizerState()
    {
      CullMode = CullMode.None,
      ScissorTestEnable = true,
    };

    public void BeginRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (UpperScissor is not null)
      {
        UpperScissor.Layout.ScissorRectangleCache = device.ScissorRectangle; //针对剪裁测试进行剪裁矩形暂存
        device.ScissorRectangle = UpperScissor.Layout.ScissorRectangle;
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, UpperScissor.ScissiorRasterizer, transformMatrix: UpperCanvas is null ? Module.UICamera.View : null);
      }
      else
        Module.BatchNormalBegin(this);
    }

    /// <summary>
    /// 执行划分元素的渲染.
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (!IsVisible && !IsHidden)
        return;
      if (IsCanvas)
      {
        device.SetRenderTarget(Canvas);
        device.Clear(Color.Transparent);
      }
      BeginRender(device, batch);
      OnRender(device, batch);
      renderer?.DoRender(device, batch);//渲染器进行渲染.
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
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, root.Layout.ScissorEnable ? root.ScissiorRasterizer : null, transformMatrix: UpperCanvas is not null ? null : Module.UICamera.View);
        batch.Draw(Canvas, Layout.ScreenLocation + Layout.Anchor, null, Design.Color, 0f, Layout.Anchor, Layout.Scale, SpriteEffects.None, 0f);
        batch.End();
      }
    }

    public virtual void OnRender(GraphicsDevice device, SpriteBatch batch)
    {

    }

    /// <summary>
    /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoRender"/>.
    /// </summary>
    /// <param name="time">游戏计时状态快照.</param>
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
        _div.Order = count;
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
      div.parent = this;
      div._module = _module;
      div.root = root;
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
      div.parent = null;
      div.root = null;
      Events.Remove(div.Events);
      return Children.Remove(div);
    }

    public void MarkRemove(Div div)
    {
      Module.Removes.Add(div);
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
          _div.Dispose();
        }
        count--;
      }
      //Clear();
    }

    public void Do(Action<Div> action) => action(this);

    /// <summary>
    /// 检查该划分元素是否为某个划分元素的后代.
    /// </summary>
    /// <param name="div"></param>
    /// <returns></returns>
    public bool DescendantsOf(Div div)
    {
      if (div.parent is not null)
      {
        if (div.parent.Equals(div))
          return true;
        else
          return div.parent.DescendantsOf(div);
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

    public Vector2 MousePos
      => Module.UICamera.ConvertToWorld(MouseResponder.Position) / CoreInfo.ScreenSizeF * CoreInfo.ViewSizeF;

    public Vector2 RelativeMousePos => MousePos - Layout.ScreenLocation;

    public Vector2 RelativeRenderMousePos => MousePos - Layout.RenderTargetLocation;

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Canvas?.Dispose();
          for (int count = 0; count < Children.Count; count++)
            Children[count].Dispose();
          OnDispose?.Invoke();
        }
        renderer = null;
        disposedValue = true;
      }
    }
    public event Action OnDispose;
    ~Div()
    {
      Dispose(disposing: false);
    }
    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}