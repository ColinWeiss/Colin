using Colin.Core.Events;
using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素的行为脚本: <b>Div 只留结构与数据, 行为 (事件挂接 / 每帧逻辑 / 布局推导) 全写脚本</b>.
  /// <br>脚本由代码 (编辑器导出的 builder 或手写工厂) 通过 <see cref="Div.BindScript{T}"/> 绑定到节点上;
  /// 节点本身不按类型区分 —— 同一个 <see cref="Div"/> 换一个脚本就是另一种控件.</br>
  /// <br>[!] 脚本可以在自己的 <see cref="OnBind"/> 里补建"固定形状"的内部件 (例如 <c>FormScript</c> 的边框+视图),
  /// 但页面/面板的结构应当由生成代码给出, 脚本只读它、不重复建.</br>
  /// <br>[!] 生命周期由 <see cref="Div"/> 驱动: 绑定 → <see cref="OnBind"/>; 重绑 → 先 <see cref="OnUnbind"/> 再 <see cref="OnBind"/>;
  /// 释放 → <see cref="OnUnbind"/>; 每帧 → <see cref="OnPreUpdate"/>(不受可见性限制) / <see cref="OnUpdate"/>; 布局 → <see cref="OnLayoutCalculate"/>.</br>
  /// <br>[!] 只在 <see cref="OnBind"/> 里挂事件并用 <see cref="Unhook"/> 登记解绑; 不要长期缓存 Div 引用 (热重载后节点会换).</br>
  /// </summary>
  public abstract class DivScript
  {
    private readonly List<Action> _unhooks = new List<Action>();
    private bool _started;

    /// <summary>脚本所属的元素 (由 <see cref="Div"/> 在绑定时填).</summary>
    public Div Owner { get; private set; }

    /// <summary>当前是否处于已绑定状态.</summary>
    public bool Bound { get; private set; }

    internal void Attach(Div owner) => Owner = owner;

    internal void BindInternal()
    {
      if (Bound || Owner is null)
        return;
      Bound = true;
      OnBind();
    }

    internal void UnbindInternal()
    {
      if (Bound is false)
        return;
      Bound = false;
      for (int index = 0; index < _unhooks.Count; index++)
        _unhooks[index]();
      _unhooks.Clear();
      OnUnbind();
    }

    internal void StartInternal(GameTime time)
    {
      if (_started)
        return;
      _started = true;
      OnStart(time);
    }

    internal void StructureReadyInternal()
    {
      if (Bound is false)
        return;
      OnStructureReady();
    }

    internal void PreUpdateInternal(GameTime time) => OnPreUpdate(time);

    internal void UpdateInternal(GameTime time) => OnUpdate(time);

    internal void LayoutInternal(ref DivLayout layout) => OnLayoutCalculate(ref layout);

    /// <summary>绑定: 取子节点、挂事件 (用下面的 OnClick/OnKey/OnHover 才会自动解绑).</summary>
    protected virtual void OnBind() { }

    /// <summary>解绑: 自定义清理; 由 <see cref="Unhook"/> 登记的动作已在此之前执行.</summary>
    protected virtual void OnUnbind() { }

    /// <summary>子树初始化完成后调用 (此刻可以读子级测量结果、给内部件设作者态参数).</summary>
    /// <summary>
    /// 这个脚本是否<b>自持结构</b>: 它在 <see cref="OnBind"/> 里创建内部节点 (Button 的 Text、Form 的 View/边框…).
    /// <br>导出时不再递归它的子级 —— 否则"生成代码建一遍 + 脚本再建一遍"会建出两套.</br>
    /// </summary>
    public virtual bool OwnsStructure => false;
    protected virtual void OnStructureReady() { }

    /// <summary>元素第一次更新时 (与 <see cref="Div.Start"/> 同帧, 在它之后).</summary>
    protected virtual void OnStart(GameTime time) { }

    /// <summary>每帧最先, 不受可见性限制 (适合维护数据; 显示相关放 <see cref="OnUpdate"/>).</summary>
    protected virtual void OnPreUpdate(GameTime time) { }

    /// <summary>每帧一次, 元素可见时才调.</summary>
    protected virtual void OnUpdate(GameTime time) { }

    /// <summary>布局推导: 位于 Div.LayoutCalculate 之后、布局事件之前.</summary>
    protected virtual void OnLayoutCalculate(ref DivLayout layout) { }

    /// <summary>
    /// 作者态参数: 交给代码生成器写进"绑定脚本 + 参数"那一段.
    /// <br>状态机类脚本 (按钮/进度条) 的真实作者值只存在脚本字段里 —— 现场值可能已被逐帧乘算污染.</br>
    /// </summary>
    public virtual void ExportValues(List<UIDesignValue> values) { }

    /// <summary>
    /// 作者态底色; 返回 <see langword="null"/> 表示"由 Div 自己的 <see cref="DivDesign.Color"/> 决定".
    /// <br>状态机脚本重写它以返回未被逐帧乘算污染的底色.</br>
    /// </summary>
    public virtual Color? AuthorColor => null;

    // —— 便捷: 取子节点 ——

    /// <summary>按名取直接子节点.</summary>
    protected Div Child(string name) => Owner?.GetChild(name);

    /// <summary>按名取直接子节点并转类型.</summary>
    protected T Child<T>(string name) where T : Div => Owner?.GetChild(name) as T;

    /// <summary>按名路径取后代.</summary>
    protected Div Node(string path) => Owner?.Find(path);

    // —— 便捷: 事件挂钩 (自动登记解绑) ——

    /// <summary>登记一次解绑动作; 手写 <c>Events.X += handler</c> 时配套使用.</summary>
    protected void Unhook(Action action)
    {
      if (action is not null)
        _unhooks.Add(action);
    }

    /// <summary>给元素挂左键点击.</summary>
    protected void OnClick(Div node, Action<Div, LeftClickedArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<LeftClickedArgs> hook = (sender, args) => handler(node, args);
      node.Events.LeftClicked += hook;
      Unhook(() => node.Events.LeftClicked -= hook);
    }

    /// <summary>给元素挂左键点击 (不关心事件参数).</summary>
    protected void OnClick(Div node, Action<Div> handler)
      => OnClick(node, (sender, args) => handler(sender));

    /// <summary>给元素挂"左键按下那一帧" (LeftClicking, 早于 LeftClicked).</summary>
    protected void OnLeftClicking(Div node, Action<Div, LeftClickingArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<LeftClickingArgs> hook = (sender, args) => handler(node, args);
      node.Events.LeftClicking += hook;
      Unhook(() => node.Events.LeftClicking -= hook);
    }

    /// <summary>给元素挂"左键按住" (LeftDown).</summary>
    protected void OnLeftDown(Div node, Action<Div, LeftDownArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<LeftDownArgs> hook = (sender, args) => handler(node, args);
      node.Events.LeftDown += hook;
      Unhook(() => node.Events.LeftDown -= hook);
    }

    /// <summary>给元素挂"左键抬起" (LeftUp).</summary>
    protected void OnLeftUp(Div node, Action<Div, LeftUpArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<LeftUpArgs> hook = (sender, args) => handler(node, args);
      node.Events.LeftUp += hook;
      Unhook(() => node.Events.LeftUp -= hook);
    }

    /// <summary>给元素挂右键点击 (RightClicked).</summary>
    protected void OnRightClicked(Div node, Action<Div, RightClickedArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<RightClickedArgs> hook = (sender, args) => handler(node, args);
      node.Events.RightClicked += hook;
      Unhook(() => node.Events.RightClicked -= hook);
    }

    /// <summary>给元素挂"按下某个键" (KeysClicked 到该节点时).</summary>
    protected void OnKey(Div node, Keys key, Action<Div, KeysClickedArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<KeysClickedArgs> hook = (sender, args) =>
      {
        if (args.Keys == key)
          handler(node, args);
      };
      node.Events.KeysClicked += hook;
      Unhook(() => node.Events.KeysClicked -= hook);
    }

    /// <summary>给元素挂"按住某个键期间持续触发" (KeysClicking).</summary>
    protected void OnKeyClicking(Div node, Keys key, Action<Div, KeysClickingArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<KeysClickingArgs> hook = (sender, args) =>
      {
        if (args.Keys == key)
          handler(node, args);
      };
      node.Events.KeysClicking += hook;
      Unhook(() => node.Events.KeysClicking -= hook);
    }

    /// <summary>给元素挂"某个键刚刚被按下" (KeysDown).</summary>
    protected void OnKeyDown(Div node, Keys key, Action<Div, KeysDownArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<KeysDownArgs> hook = (sender, args) =>
      {
        if (args.Keys == key)
          handler(node, args);
      };
      node.Events.KeysDown += hook;
      Unhook(() => node.Events.KeysDown -= hook);
    }

    /// <summary>给元素挂"任意键点击" (不按键位过滤; 适合内部再走输入映射的热键).</summary>
    protected void OnKeysClicked(Div node, Action<Div, KeysClickedArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<KeysClickedArgs> hook = (sender, args) => handler(node, args);
      node.Events.KeysClicked += hook;
      Unhook(() => node.Events.KeysClicked -= hook);
    }

    /// <summary>给元素挂"悬停开始" (HoverBegan).</summary>
    protected void OnHover(Div node, Action<Div> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<MouseHoverArgs> hook = (sender, args) => handler(node);
      node.Events.HoverBegan += hook;
      Unhook(() => node.Events.HoverBegan -= hook);
    }

    /// <summary>给元素挂"悬停期间每帧" (MouseHover).</summary>
    protected void OnHovering(Div node, Action<Div, MouseHoverArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<MouseHoverArgs> hook = (sender, args) => handler(node, args);
      node.Events.MouseHover += hook;
      Unhook(() => node.Events.MouseHover -= hook);
    }

    /// <summary>给元素挂"悬停结束" (HoverEnded).</summary>
    protected void OnHoverEnd(Div node, Action<Div, MouseHoverArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<MouseHoverArgs> hook = (sender, args) => handler(node, args);
      node.Events.HoverEnded += hook;
      Unhook(() => node.Events.HoverEnded -= hook);
    }

    /// <summary>给元素挂右键按下那一帧 (RightClicking).</summary>
    protected void OnRightClicking(Div node, Action<Div, RightClickingArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<RightClickingArgs> hook = (sender, args) => handler(node, args);
      node.Events.RightClicking += hook;
      Unhook(() => node.Events.RightClicking -= hook);
    }

    /// <summary>给元素挂右键按住 (RightDown).</summary>
    protected void OnRightDown(Div node, Action<Div, RightDownArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<RightDownArgs> hook = (sender, args) => handler(node, args);
      node.Events.RightDown += hook;
      Unhook(() => node.Events.RightDown -= hook);
    }

    /// <summary>给元素挂右键抬起 (RightUp).</summary>
    protected void OnRightUp(Div node, Action<Div, RightUpArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<RightUpArgs> hook = (sender, args) => handler(node, args);
      node.Events.RightUp += hook;
      Unhook(() => node.Events.RightUp -= hook);
    }

    /// <summary>给元素挂滚轮上滚.</summary>
    protected void OnScrollUp(Div node, Action<Div, ScrollUpArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<ScrollUpArgs> hook = (sender, args) => handler(node, args);
      node.Events.ScrollUp += hook;
      Unhook(() => node.Events.ScrollUp -= hook);
    }

    /// <summary>给元素挂滚轮下滚.</summary>
    protected void OnScrollDown(Div node, Action<Div, ScrollDownArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<ScrollDownArgs> hook = (sender, args) => handler(node, args);
      node.Events.ScrollDown += hook;
      Unhook(() => node.Events.ScrollDown -= hook);
    }

    /// <summary>给元素挂滚轮中键点击.</summary>
    protected void OnScrollClicked(Div node, Action<Div, ScrollClickedArgs> handler)
    {
      if (node is null || handler is null)
        return;
      EventHandler<ScrollClickedArgs> hook = (sender, args) => handler(node, args);
      node.Events.ScrollClicked += hook;
      Unhook(() => node.Events.ScrollClicked -= hook);
    }

    /// <summary>元素唤醒 (DoWakeUp 开始时).</summary>
    protected void OnWake(Action handler)
    {
      if (Owner is null || handler is null)
        return;
      Action hook = handler;
      Owner.OnDoWakeUpStart += hook;
      Unhook(() => Owner.OnDoWakeUpStart -= hook);
    }

    /// <summary>元素休眠 (DoHibernate 开始时).</summary>
    protected void OnSleep(Action handler)
    {
      if (Owner is null || handler is null)
        return;
      Action hook = handler;
      Owner.OnDoHibernateStart += hook;
      Unhook(() => Owner.OnDoHibernateStart -= hook);
    }
  }
}
