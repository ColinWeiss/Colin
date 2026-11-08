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
    /// 划分元素的稳定标识 (构造时生成, 重命名不变).
    /// <br>动画轨道与跨会话引用一律以它寻址 (见 <c>DivFinder.FindById</c>).</br>
    /// </summary>
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");

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
    /// 指示划分元素<b>自身</b>是否可见.
    /// <br>[!] 这是作者态, <b>不向下级联</b> —— 关掉父级不会改写子级的作者值, 打开父级也不会。
    /// 子级"实际是否显示"请看 <see cref="IsVisibleInTree"/>。</br>
    /// </summary>
    public bool IsVisible
    {
      get => isVisible;
      set => isVisible = value;
    }

    /// <summary>
    /// 指示本元素及其全部祖先是否都可见 (派生值; 不做缓存, 深度即调用次数).
    /// <br>更新/渲染靠递归入口自然收敛, 但事件命中与"这个元素现在能不能被看到"必须用它。</br>
    /// </summary>
    public bool IsVisibleInTree
    {
      get
      {
        for (Div node = this; node is not null; node = node._parent)
          if (node.isVisible is false)
            return false;
        return true;
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
    /// 按路径取后代元素: <c>"View/Panel/StartButton"</c> 相对本元素; <c>"/"</c> 开头则从根开始.
    /// <br>文档驱动的界面里, 逻辑靠"名字路径"定位节点 (热重载后重新绑即可), 不必长期持有引用.</br>
    /// </summary>
    public Div Find(string path)
    {
      if (string.IsNullOrEmpty(path))
        return null;
      bool fromRoot = path[0] == '/';
      Div current = fromRoot ? Root ?? this : this;
      int start = fromRoot ? 1 : 0;
      for (int cursor = start; cursor <= path.Length; cursor++)
      {
        if (cursor < path.Length && path[cursor] != '/')
          continue;
        if (cursor > start)
        {
          current = current?.GetChild(path.Substring(start, cursor - start));
          if (current is null)
            return null;
        }
        start = cursor + 1;
      }
      return current;
    }

    /// <summary>按路径取后代元素并转成指定类型; 找不到或类型不符返回 null.</summary>
    public T Find<T>(string path) where T : Div => Find(path) as T;

    /// <summary>
    /// 深度优先找第一个指定类型的后代 (不含自身).
    /// <br>[!] 用它而不是 <c>GetChild(名字) as T</c>: 名字是文档里的可改数据, 类型才是代码契约 — 同名副本/改名都不该骗过逻辑.</br>
    /// <br>[!] 类型标了 <see cref="UIUniqueAttribute"/> 而树上出现多个时, 会打一条警告并列出各自的路径 —— 不会静默找错.</br>
    /// </summary>
    public Div FindFirst(Type type) => FindFirst(type, null);

    /// <summary>深度优先找第一个满足条件的指定类型后代.</summary>
    public Div FindFirst(Type type, Func<Div, bool> filter)
    {
      if (type is null || Disposed)
        return null;
      Div found = FindFirstCore(type, filter);
      //唯一类型出现多份只查一次/报一次: 避免每帧为了诊断反复遍历 (这个 API 会被宿主每帧调用)
      if (found is not null && IsUniqueType(type) && _uniqueWarned.Contains(type) is false)
        WarnDuplicateUnique(type, filter, found);
      return found;
    }

    /// <summary>深度优先找第一个指定类型的后代.</summary>
    public T FindFirst<T>() where T : Div => FindFirst(typeof(T), null) as T;

    /// <summary>深度优先找第一个满足条件的指定类型后代; 多实例场景用它显式挑 (例如 <c>slot =&gt; slot.OwnerIndex == 1</c>).</summary>
    public T FindFirst<T>(Func<T, bool> filter) where T : Div
      => FindFirst(typeof(T), filter is null ? null : node => node is T typed && filter(typed)) as T;

    /// <summary>收集所有指定类型的后代 (不含自身; 跳过视图副本). 多实例场景用它自己决定取哪个.</summary>
    public List<Div> FindAll(Type type, List<Div> results = null)
    {
      List<Div> list = results ?? new List<Div>();
      if (type is null || Disposed)
        return list;
      List<Div> children = Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div child = children[index];
        if (child is null || child.Disposed)
          continue;
        if (type.IsInstanceOfType(child))
          list.Add(child);
        child.FindAll(type, list);
      }
      return list;
    }

    /// <summary>收集所有指定类型的后代.</summary>
    public List<T> FindAll<T>() where T : Div
    {
      List<Div> raw = FindAll(typeof(T));
      List<T> result = new List<T>(raw.Count);
      for (int index = 0; index < raw.Count; index++)
        result.Add((T)raw[index]);
      return result;
    }

    /// <summary>深度优先找第一个挂着指定脚本的元素 (含自身).</summary>
    public Div FindScript<T>() where T : DivScript => FindScriptCore<T>(this);

    private static Div FindScriptCore<T>(Div node) where T : DivScript
    {
      if (node is null || node.Disposed)
        return null;
      if (node.HasScript<T>())
        return node;
      List<Div> children = node.Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div found = FindScriptCore<T>(children[index]);
        if (found is not null)
          return found;
      }
      return null;
    }

    /// <summary>收集所有挂着指定脚本的后代 (可带过滤; 跳过已释放节点).</summary>
    public List<Div> FindAllScripts<T>(List<Div> results = null, Func<Div, bool> filter = null) where T : DivScript
    {
      List<Div> list = results ?? new List<Div>();
      if (Disposed)
        return list;
      List<Div> children = Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div child = children[index];
        if (child is null || child.Disposed)
          continue;
        if (child.HasScript<T>() && (filter is null || filter(child)))
          list.Add(child);
        child.FindAllScripts<T>(list, filter);
      }
      return list;
    }

    private Div FindFirstCore(Type type, Func<Div, bool> filter)
    {
      List<Div> children = Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div child = children[index];
        if (child is null || child.Disposed)
          continue;
        if (type.IsInstanceOfType(child) && (filter is null || filter(child)))
          return child;
        Div nested = child.FindFirstCore(type, filter);
        if (nested is not null)
          return nested;
      }
      return null;
    }

    /// <summary>唯一类型出现多份时的告警 (同一类型只报一次, 附路径方便定位).</summary>
    private void WarnDuplicateUnique(Type type, Func<Div, bool> filter, Div used)
    {
      List<Div> all = new List<Div>();
      FindAll(type, all);
      int count = 0;
      for (int index = 0; index < all.Count; index++)
        if (filter is null || filter(all[index]))
          count++;
      if (count <= 1)
        return;
      lock (_uniqueWarned)
      {
        if (_uniqueWarned.Add(type) is false)
          return;
      }
      string paths = string.Empty;
      int listed = 0;
      for (int index = 0; index < all.Count && listed < 4; index++)
      {
        if (filter is not null && filter(all[index]) is false)
          continue;
        paths = string.Concat(paths, listed == 0 ? string.Empty : " | ", DescribePath(all[index]));
        listed++;
      }
      Console.Log(ConsoleTextType.Warning, "UI",
        string.Concat(type.Name, " 标记为唯一, 但树上找到 ", count.ToString(), " 个: ", paths,
          " —— 本次使用 ", DescribePath(used), "; 多余的那个请删掉或用 FindFirst(filter) 显式指定"));
    }

    private static readonly HashSet<Type> _uniqueWarned = new HashSet<Type>();
    private static readonly Dictionary<Type, bool> _uniqueTypes = new Dictionary<Type, bool>();

    private static bool IsUniqueType(Type type)
    {
      if (_uniqueTypes.TryGetValue(type, out bool unique))
        return unique;
      unique = type.IsDefined(typeof(UIUniqueAttribute), true);
      _uniqueTypes[type] = unique;
      return unique;
    }

    /// <summary>可读路径 (从根到本元素), 诊断/日志用.</summary>
    public string DescribePath()
    {
      List<Div> chain = new List<Div>();
      Div node = this;
      while (node is not null && chain.Count < 16)
      {
        chain.Add(node);
        node = node.Parent;
      }
      chain.Reverse();
      return string.Join("/", chain.Select(item => item.Name));
    }

    private static string DescribePath(Div div) => div is null ? "(null)" : div.DescribePath();

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

    /// <summary>
    /// 获取划分元素可溯到的最近的上一层画布元素.
    /// <br>[!] 不做缓存: 重挂载/改 <see cref="UseBatch"/> / 改剪裁开关都会让缓存失效,
    /// 而祖先链本来就短 (指针跳几次), 省下的时间不值得换这一类"搬到新父级后还按旧祖先裁剪"的错.</br>
    /// </summary>
    public Div UpperCanvas
    {
      get
      {
        for (Div node = _parent; node is not null; node = node._parent)
          if (node.IsCanvas)
            return node;
        return null;
      }
    }

    /// <summary>
    /// 获取划分元素可溯到的最近一层启用了剪裁的祖先.
    /// <br>画布是剪裁边界: 不会再向画布之外找 (画布内容自行裁剪).
    /// </summary>
    public Div UpperScissor
    {
      get
      {
        if (IsCanvas)
          return null;
        for (Div node = _parent; node is not null; node = node._parent)
        {
          if (node.IsCanvas)
            return null;
          if (node.Layout.ScissorEnable)
            return node;
        }
        return null;
      }
    }

    /// <summary>
    /// 获取划分元素可溯到的最近的批次容器 (<see cref="UseBatch"/> 为真的祖先).
    /// </summary>
    public Div UpperBatch
    {
      get
      {
        for (Div node = _parent; node is not null; node = node._parent)
          if (node.UseBatch)
            return node;
        return null;
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

    internal Div _root;
    /// <summary>
    /// 获取划分元素之「阈点」.
    /// </summary>
    public Div Root => _root;

    /// <summary>
    /// 指示该划分元素是否为可作为渲染目标的画布元素.
    /// <br>[!] 这是结构身份, 只能在构造后、初始化前确定 (见 <see cref="MarkAsCanvas"/>); 运行期不可改。</br>
    /// </summary>
    public bool IsCanvas { get; private set; }

    /// <summary>
    /// 把本元素标记为画布 (渲染目标).
    /// <br>供"由生成代码重建结构"与文档反序列化使用 —— 画布身份无法用 <c>new T("名字")</c> 表达,
    /// 只能由 <c>X.UI.cs</c> 在建节点后补一句.</br>
    /// </summary>
    public void MarkAsCanvas() => IsCanvas = true;

    public bool InitializationCompleted = false;

    /// <summary>
    /// 指示本元素是否为"界面根" (原 <c>DivRoot</c> 的职责): 尺寸每帧同步视口, 并充当子树里最近的根.
    /// <br>[!] 与 <see cref="IsCanvas"/> 一样是结构身份, 请在初始化前设置 (生成代码/工厂里紧跟 <c>new Div</c>).</br>
    /// </summary>
    public bool IsRoot { get; private set; }

    /// <summary>
    /// 这是"运行态生成"的节点 (槽位按数量现建、图标框/数量字这类脚本内部件):
    /// <br>导出代码时跳过它和它的子树 —— 结构权威在脚本, 生成物只写 <c>BindScript</c>.</br>
    /// <br>[!] 与 <see cref="DivScript.OwnsStructure"/> 的区别: 后者是"整棵子树都归脚本",
    /// 这里是"只有我自己是运行态产物", 兄弟节点照常导出.</br>
    /// </summary>
    public bool ExcludeFromExport { get; set; }
    /// <summary>把本元素标记为界面根 (视口尺寸同步).</summary>
    public void MarkAsRoot()
    {
      IsRoot = true;
      _root = this;
    }

    private DivScript _script;

    /// <summary>
    /// 本元素的行为脚本 (见 <see cref="DivScript"/>): 结构留在 Div, 事件/逐帧逻辑写脚本.
    /// <br>由 <see cref="BindScript{T}"/> 绑定 (生成代码 / 手写工厂), 随初始化与重绑驱动.</br>
    /// </summary>
    public DivScript Script => _script;

    /// <summary>
    /// 绑定行为脚本 (替换既有脚本; 已初始化时立刻绑定, 否则等到初始化末尾).
    /// <br>同一个节点只保留一个脚本 —— 换脚本就是换控件.</br>
    /// </summary>
    public T BindScript<T>(T script) where T : DivScript
    {
      if (script is null)
        return null;
      if (ReferenceEquals(_script, script))
      {
        _script.BindInternal();
        return script;
      }
      _script?.UnbindInternal();
      _script = script;
      _script.Attach(this);
      if (InitializationCompleted)
        _script.BindInternal();
      return script;
    }

    /// <summary>
    /// 编辑器用: 已经创建、但尚未编译的脚本类型全名 (创建空脚本时预绑定).
    /// <br>导出时若 <see cref="Script"/> 为空而这里非空, 就按类型名写 <c>BindScript(new global::Ns.XxxScript())</c>,
    /// 让"新建脚本 + 导出"合并进同一次编译.</br>
    /// </summary>
    public string PendingScriptType { get; set; }
    /// <summary>取本元素上指定类型的脚本; 没有 (或类型不符) 返回 null.</summary>
    public T GetScript<T>() where T : DivScript => _script as T;

    /// <summary>本元素是否挂着指定类型的脚本.</summary>
    public bool HasScript<T>() where T : DivScript => _script is T;

    /// <summary>解绑并移除脚本 (节点保留, 行为消失).</summary>
    public void UnbindScript()
    {
      _script?.UnbindInternal();
      _script = null;
    }

    /// <summary>确保脚本进入绑定状态 (幂等; 初始化末尾与重绑时调用).</summary>
    public void EnsureScript()
    {
      if (Disposed)
        return;
      _script?.BindInternal();
    }

    /// <summary>重绑: 先解绑再绑, 保证事件不叠加.</summary>
    public void RebindScript()
    {
      if (Disposed || _script is null)
        return;
      _script.UnbindInternal();
      _script.BindInternal();
    }

    /// <summary>本元素及整棵子树的脚本全部重绑; 返回重绑的脚本数.</summary>
    public int RebindScriptTree()
    {
      if (Disposed)
        return 0;
      int count = 0;
      if (_script is not null)
      {
        _script.UnbindInternal();
        _script.BindInternal();
        count++;
      }
      List<Div> nodes = Children;
      for (int index = 0; index < nodes.Count; index++)
        count += nodes[index].RebindScriptTree();
      return count;
    }

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
      Layout.Scale = Vector2.One;
      Interact.IsInteractive = true;
      Design.Color = Color.White;
      IsCanvas = isCanvas;
    }

    /// <summary>
    /// 重建画布渲染目标并同步布局尺寸.
    /// <br>[!] 不碰 <see cref="DivLayout.Anchor"/> —— 轴点是作者态布局数据, 不该被"重建渲染目标"顺带改写
    /// (旧实现这么干, 和 <c>Form.LayoutCalculate</c> 每帧写的视图中心轴点互相打架).</br>
    /// </summary>
    public void SetCanvas(float width, float height)
    {
      Layout.Width = width;
      Layout.Height = height;
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
      if (Disposed)
        return;
      if (IsRoot)
        _root = this;
      if (Parent is not null)
      {
        _module = _parent._module;
        _root = _parent._root;
      }
      if (InitializationCompleted)
        return;
      DivInit();
      BuildUI();
      if (IsRoot)
      {
        Layout.Width = CoreInfo.ViewWidth;
        Layout.Height = CoreInfo.ViewHeight;
      }
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
      //子级逐个初始化 (用下标循环而不是 ForEach(lambda): 这里每帧不跑, 但每节点一次委托分配也没必要).
      List<Div> nodes = Children;
      for (int index = 0; index < nodes.Count; index++)
        nodes[index]?.DoInitialize();
      //子树已全部初始化: 此时才允许读子级尺寸、往节点里塞内容 (见 OnStructureReady).
      //[!] 先置位再调用: 就绪钩子里新 Attach 的子级必须能被自动初始化 (见 AttachDirect).
      InitializationCompleted = true;
      EnsureScript();
      OnStructureReady();
      _script?.StructureReadyInternal();
    }

    /// <summary>
    /// 子树初始化完成后调用 (每个元素一次): 读子级测量结果、给内部件设作者态参数、挂一次性引用.
    /// <br>脚本的同名钩子 (<see cref="DivScript"/> 的 <c>OnStructureReady</c>) 在本方法之后调用.</br>
    /// </summary>
    /// <summary>
    /// 结构挂钩点: 手写页面/面板可以在建自己的子级 (生成代码走静态 builder, 不依赖它).
    /// <br>由 <see cref="DoInitialize"/> 在 <see cref="DivInit"/> 之后、子级初始化之前调用.</br>
    /// </summary>
    public virtual void BuildUI() { }

    public virtual void OnStructureReady() { }

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
      if (Disposed)
        return;
      _script?.PreUpdateInternal(time);
      PreUpdate(time);
      if (IsRoot is false)
      {
        //只补空, 不清空: 父级为空时把 module 抹掉会让"临时摘下来再挂回去"的节点丢上下文.
        _module ??= Parent?._module;
        _root ??= Parent?._root;
      }
      //焦点状态镜像: InteractStyle.Focus/FocusLast 是对外契约的一部分, 不能是永远为假的死字段.
      Interact.FocusLast = Interact.Focus;
      Interact.Focus = _module is not null && ReferenceEquals(_module.Focus, this);
      if (IsVisible is false || IsCulled)
        return;
      if (!_started)
      {
        Start(time);
        _script?.StartInternal(time);
        _started = true;
      }
      Controller?.Layout(this, ref Layout);
      Controller?.Interact(this, ref Interact);
      Controller?.Design(this, ref Design);
      Style.Apply(this, Parent);
      LayoutCalculate(ref Layout);
      _script?.LayoutInternal(ref Layout);
      LayoutEvent?.Invoke(this);
      InteractCalculate(ref Interact);
      DesignCalculate(ref Design);
      DivLayout.Calculate(this);
      if (IsCanvas)
        EnsureCanvasSize();
      CalculateScissorBounds();
      Events.DoUpdate();
      //事件处理器可能把自己释放掉 (关弹窗/删条目): 释放后别再继续跑本帧的更新与子级遍历.
      if (Disposed)
        return;
      OnUpdate(time);
      _script?.UpdateInternal(time);
      UpdateChildren(time);
    }

    /// <summary>
    /// 布局推导钩子; 界面根在这里把尺寸同步到视口 (原 <c>DivRoot</c> 的职责).
    /// </summary>
    public virtual void LayoutCalculate(ref DivLayout layout)
    {
      if (IsRoot)
      {
        layout.Width = CoreInfo.ViewWidth;
        layout.Height = CoreInfo.ViewHeight;
      }
    }

    public virtual void InteractCalculate(ref InteractStyle interact) { }

    public virtual void DesignCalculate(ref DivDesign design) { }

    /// <summary>
    /// 作者态底色 —— 代码导出读取这个值, 而不是 <see cref="DivDesign.Color"/> 的当帧快照.
    /// <br>状态机脚本 (如 <c>ButtonScript</c>) 每帧把乘算结果写回 <see cref="DivDesign.Color"/>, 直接读会把
    /// "恰好悬停时的提亮色"钉进生成文件; 脚本通过 <see cref="DivScript.AuthorColor"/> 提供未被污染的底色。</br>
    /// </summary>
    public Color AuthorColor => _script?.AuthorColor ?? Design.Color;

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
      //未挂载界面时 (编辑器预览 / 构造后未初始化) 用单位阵, 不该为了画一个预览去空引用.
      Matrix transform = UpperCanvas is not null || Module is null ? Matrix.Identity : Module.UICamera.View;
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
      if (Disposed || IsVisible is false || IsCulled)
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
      _script?.RenderInternal(device, batch);//脚本的自定义绘制 (与视图状态密不可分的那类).
      _renderer?.DoRender(device, batch);//渲染器进行渲染.
      RenderChildren(device, batch);
      if (UseBatch)
        UIBatch.Flush(); //批次容器: 子树绘制完毕后一并提交.
      if (IsCanvas)
      {
        //切回上级渲染目标, 将画布内容合成绘制; 提交顺序保持与绘制顺序一致.
        UIBatch.Flush();
        Div upperCanvas = UpperCanvas;
        if (upperCanvas is not null)
          device.SetRenderTarget(upperCanvas.Canvas);
        else if (Module is not null)
          device.SetRenderTarget(Module.RawRt);
        Vector2 canvasRenderPos = upperCanvas is not null
          ? Layout.RenderTargetLocation + Layout.Anchor
          : Layout.ScreenLocation + Layout.Anchor;
        bool rootScissor = _root is not null && _root.Layout.ScissorEnable;
        UIBatch.Request(
          BlendState.AlphaBlend, SamplerState.PointWrap,
          DepthStencilState.None,
          rootScissor ? UIBatch.ScissorTestRasterizer : UIBatch.DefaultRasterizer,
          rootScissor ? _root.ScissorBounds : default,
          upperCanvas is not null || Module is null ? Matrix.Identity : Module.UICamera.View);
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
    public virtual bool Register(Div div, bool doInit = false) => AttachDirect(div, doInit);

    /// <summary>
    /// 按"子级集合"语义挂载子元素, 不经过 <see cref="Register"/> 的虚分派.
    /// <br>[!] 文档装载专用: <see cref="Common.UserInterfaces.Forms.Form"/> 这类重写了 <see cref="Register"/> 的类型会把子级挂进 View,</br>
    /// <br>而文档存的是它自己的 <see cref="Children"/> —— 用 Register 还原会把文档里的 View/Border 挪成 View 的子级, 层级被悄悄改写.</br>
    /// </summary>
    internal bool AttachDirect(Div div, bool doInit = false)
    {
      if (div is null || ReferenceEquals(div, this) || Disposed)
        return false;
      div._parent = this;
      div._module = _module;
      div._root = _root;
      div.Layout.Invalidate(); //重挂载后强制重算, 防止版本戳巧合时沿用旧父级的布局结果.
      Events.Register(div.Events);
      Children.Add(div);
      _structureVersion++;
      //已初始化的树上新挂节点 (生成代码的 OnStructureReady / 编辑器添加子级 / 运行时塞内容):
      //立刻补一次初始化, 否则它的 DivInit 与生成结构永远不会执行.
      if (doInit || (InitializationCompleted && div.InitializationCompleted is false))
        div.DoInitialize();
      return true;
    }

    /// <summary>
    /// 按"子级集合"语义挂载子元素, 供<b>生成的布局代码</b>与结构重建使用.
    /// <br>与 <see cref="Register"/> 的区别: 这里不做虚分派 —— 生成代码描述的是"谁是谁的子级",</br>
    /// <br>不能被 <see cref="Common.UserInterfaces.Forms.Form"/> 这类重写改写层级, 否则代码与生成物描述的树不是同一棵.</br>
    /// </summary>
    public bool Attach(Div div, bool doInit = false) => AttachDirect(div, doInit);

    /// <summary>
    /// 于子级序列的指定位置插入子元素 (层级编辑器重排用).
    /// </summary>
    public virtual bool Insert(int index, Div div, bool doInit = false)
    {
      if (div is null || ReferenceEquals(div, this) || Disposed)
        return false;
      div._parent = this;
      div._module = _module;
      div._root = _root;
      div.Layout.Invalidate();
      index = Math.Clamp(index, 0, Children.Count);
      Events.Insert(index, div.Events);
      Children.Insert(index, div);
      _structureVersion++;
      if (doInit || (InitializationCompleted && div.InitializationCompleted is false))
        div.DoInitialize();
      return true;
    }

    /// <summary>
    /// 移除子元素.
    /// </summary>
    /// <param name="div">需要移除的划分元素.</param>
    /// <returns>若移除成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public virtual bool Remove(Div div)
    {
      if (div is null)
        return false;
      div._parent = null;
      div._root = null;
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

    /// <summary>
    /// 把元素提到同级序列末尾 (最上层).
    /// <br>[!] 一切都在它<b>实际的父级</b>上完成: <see cref="Remove"/> 与挂载必须成对作用在同一个列表上,
    /// 否则 <c>Form</c> 这类把子级转发进 <c>View</c> 的容器会出现"从 Form 删、往 View 加"从而复制一份.</br>
    /// </summary>
    public void SetTop(Div division)
    {
      if (division is null || ReferenceEquals(division, this))
        return;
      Div owner = division.Parent;
      if (owner is null)
        return;
      owner.Remove(division);
      owner.Attach(division);
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

    /// <summary>鼠标于本元素坐标系中的位置; 界面未挂载时为 <see cref="Vector2.Zero"/> (编辑器在挂载前读它不该炸).</summary>
    public Vector2 MousePos => Module is null ? Vector2.Zero : Module.UICamera.ConvertToWorld(MouseResponder.Position);

    public Vector2 RelativeMousePos => MousePos - Layout.ScreenLocation;

    public Vector2 RelativeRenderMousePos => MousePos - Layout.RenderTargetLocation;

    public event Action OnDispose;

    public bool Disposed = false;

    public virtual void Dispose()
    {
      if (Disposed)
        return;
      //自摘除: 释放后残留在父级 Children 里的元素会被继续 Update/Render (事件节点已销毁 → NRE)
      Div parent = _parent;
      if (parent is not null && parent.Disposed is false)
      {
        parent.Events?.Remove(Events);
        if (parent.Children.Remove(this))
          parent._structureVersion++;
      }
      Disposed = true;
      _parent = null;
      _renderer = null;
      _controller = null;
      _module = null;
      _root = null;
      //先递归子级 (它们还要从本节点的事件节点上注销), 再销毁自己的事件节点.
      //[!] 子级 Dispose 会把自己从 Children 里摘掉, 所以不能"按下标遍历" —— 从尾部取并核对是否真的移除了,
      //    既不会跳元素, 也不会在被覆盖的 Dispose 忘了自摘除时留下没释放的子级.
      List<Div> children = Children;
      while (children.Count > 0)
      {
        Div child = children[children.Count - 1];
        child.Dispose();
        if (children.Count > 0 && ReferenceEquals(children[children.Count - 1], child))
        {
          children.RemoveAt(children.Count - 1);
          _structureVersion++;
        }
      }
      children.Clear();
      _script?.UnbindInternal();
      _script = null;
      Events?.Dispose();
      Canvas?.Dispose();
      OnDispose?.Invoke();
    }
  }
}