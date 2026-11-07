using Colin.Core.Events;
using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 页面级脚本: 给"没有生成字段可用"的外部程序集 (模组) 留的逻辑入口.
  /// <br>页面初始化完成后由 <see cref="DivPageHost"/> 调 <see cref="UIPageScripts.Run"/>, 脚本按名字路径找节点并挂事件.</br>
  /// <br>[!] 能改页面类的时候优先用页面自己的 <c>DivScript</c> 与生成字段 —— 编译期检查永远比字符串路径可靠.</br>
  /// <br>[!] <see cref="Unbind"/> 会在每次 <see cref="Bind"/> 之前调用; 用基类提供的 OnClick/OnKey 会自动登记解绑,
  /// 手写 <c>Events.X += handler</c> 时请自己配对 <c>-=</c>.</br>
  /// <br>[!] 脚本实例是<b>单例</b>: <see cref="State{T}"/> 里的数据跨重绑保留 (但别往里放 Div 引用).</br>
  /// </summary>
  public interface IUIPageScript
  {
    /// <summary>适用页面的名字 (null/空 = 所有页面); 与页面的 <see cref="Div.Name"/> 比较.</summary>
    string PageName { get; }

    /// <summary>解绑上一轮挂的处理器; 保证重复绑定不叠加.</summary>
    void Unbind();

    /// <summary>把逻辑挂到页面上.</summary>
    void Bind(Div page);
  }

  /// <summary>
  /// 脚本基类: 提供节点路径查找 + 自动登记解绑的事件挂钩 + 跨重绑保留的私有状态.
  /// <br>示例:</br>
  /// <code>
  /// public sealed class MyScript : UIPageScript
  /// {
  ///     public override string PageName => "Gameplay";
  ///     public override void Bind(Div page)
  ///     {
  ///         OnClick&lt;Button&gt;(page, "View/StartButton", (button, e) => { e.IsCapture = true; ... });
  ///         OnKey(page, Keys.F10, (_, e) => Dump(page, 0));
  ///         int opens = State("opens", () => 0);   // 跨重绑保留
  ///     }
  /// }
  /// </code>
  /// </summary>
  public abstract class UIPageScript : IUIPageScript
  {
    private readonly List<Action> _unhooks = new List<Action>();
    private readonly Dictionary<string, object> _state = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>适用页面名; 默认 null = 所有页面.</summary>
    public virtual string PageName => null;

    /// <summary>把逻辑挂到页面上; 每次重绑都会重来一遍.</summary>
    public abstract void Bind(Div page);

    /// <summary>解绑上一轮挂的处理器 (由 <see cref="UIPageScripts.Run"/> 自动调用).</summary>
    public void Unbind()
    {
      for (int index = 0; index < _unhooks.Count; index++)
        _unhooks[index]();
      _unhooks.Clear();
      UIPageScripts.ClearBindings(this);
    }

    /// <summary>
    /// 脚本私有状态 (跨重绑保留): 首次取时用 <paramref name="create"/> 建值.
    /// <br>[!] 只放数据, 不要放 Div —— 重绑后节点可能已经是新实例.</br>
    /// </summary>
    protected T State<T>(string key, Func<T> create)
    {
      if (string.IsNullOrEmpty(key) || create is null)
        return default;
      if (_state.TryGetValue(key, out object value) && value is T typed)
        return typed;
      T fresh = create();
      _state[key] = fresh;
      return fresh;
    }

    /// <summary>写入脚本私有状态 (跨重绑保留).</summary>
    protected void State<T>(string key, T value)
    {
      if (string.IsNullOrEmpty(key))
        return;
      _state[key] = value;
    }

    /// <summary>按路径取节点 (相对页面; <c>"/"</c> 开头从界面根开始). 取不到时落一条警告并返回 null, 不会抛.</summary>
    protected T Node<T>(Div page, string path) where T : Div
    {
      T node = Resolve<T>(page, path);
      UIPageScripts.Record(this, node, path, "Find");
      return node;
    }

    private T Resolve<T>(Div page, string path) where T : Div
    {
      Div node = page?.Find(path);
      if (node is null)
      {
        Console.Log(ConsoleTextType.Warning, "UIScript", string.Concat(DescribePage(), " 找不到节点 ", path));
        return null;
      }
      if (node is not T typed)
      {
        Console.Log(ConsoleTextType.Warning, "UIScript",
          string.Concat(DescribePage(), " 节点 ", path, " 是 ", node.GetType().Name, ", 期望 ", typeof(T).Name));
        return null;
      }
      return typed;
    }

    private string DescribePage() => string.IsNullOrEmpty(PageName) ? "(全局脚本)" : string.Concat("[", PageName, "]");

    /// <summary>给节点挂左键点击; 返回值方便链式/继续操作.</summary>
    protected T OnClick<T>(Div page, string path, Action<T, LeftClickedArgs> handler) where T : Div
    {
      T node = Resolve<T>(page, path);
      UIPageScripts.Record(this, node, path, "Click");
      if (node is null || handler is null)
        return node;
      EventHandler<LeftClickedArgs> hook = (sender, args) => handler(node, args);
      node.Events.LeftClicked += hook;
      _unhooks.Add(() => node.Events.LeftClicked -= hook);
      return node;
    }

    /// <summary>给节点挂左键点击 (不关心事件参数时用).</summary>
    protected T OnClick<T>(Div page, string path, Action<T> handler) where T : Div
      => OnClick<T>(page, path, (node, _) => handler(node));

    /// <summary>给节点挂"按某个特定键"; 只在事件冒泡到这里且键位匹配时触发.</summary>
    protected T OnKey<T>(Div page, string path, Keys key, Action<T, KeysClickedArgs> handler) where T : Div
    {
      T node = Resolve<T>(page, path);
      UIPageScripts.Record(this, node, path, string.Concat("Key:", key));
      if (node is null || handler is null)
        return node;
      EventHandler<KeysClickedArgs> hook = (sender, args) =>
      {
        if (args.Keys == key)
          handler(node, args);
      };
      node.Events.KeysClicked += hook;
      _unhooks.Add(() => node.Events.KeysClicked -= hook);
      return node;
    }

    /// <summary>页面级热键 (不需要挂到某个节点上).</summary>
    protected void OnKey(Div page, Keys key, Action<Div, KeysClickedArgs> handler)
    {
      if (page is null || handler is null)
        return;
      UIPageScripts.Record(this, page, string.Empty, string.Concat("PageKey:", key));
      EventHandler<KeysClickedArgs> hook = (sender, args) =>
      {
        if (args.Keys == key)
          handler(page, args);
      };
      page.Events.KeysClicked += hook;
      _unhooks.Add(() => page.Events.KeysClicked -= hook);
    }

    /// <summary>悬停开始 (元素由不可交互变为悬停时触发一次).</summary>
    protected T OnHover<T>(Div page, string path, Action<T> handler) where T : Div
    {
      T node = Resolve<T>(page, path);
      UIPageScripts.Record(this, node, path, "Hover");
      if (node is null || handler is null)
        return node;
      EventHandler<MouseHoverArgs> hook = (sender, args) => handler(node);
      node.Events.HoverBegan += hook;
      _unhooks.Add(() => node.Events.HoverBegan -= hook);
      return node;
    }

    /// <summary>手动登记一次解绑动作 (手写事件时配套使用).</summary>
    protected void OnUnbind(Action action)
    {
      if (action is not null)
        _unhooks.Add(action);
    }
  }

  /// <summary>
  /// 页面脚本注册表: 逻辑与页面类解耦 —— 新增玩法逻辑只要写一个 <see cref="UIPageScript"/> 并注册,
  /// 不用改页面的加载/绑定代码.
  /// <br>同时记录每个脚本"绑到了哪个节点" (<see cref="Bindings"/>), 供编辑器显示与断链诊断.</br>
  /// </summary>
  public static class UIPageScripts
  {
    /// <summary>一条脚本绑定点: 哪段脚本、用什么路径、绑到哪个节点、什么事件.</summary>
    public readonly struct UIPageScriptBinding
    {
      public readonly Div Node;
      public readonly string Path;
      public readonly string Script;
      public readonly string Kind;
      /// <summary>绑定它的脚本实例; 解绑按引用匹配 (同名脚本类型不该互相清账).</summary>
      public readonly object Owner;

      public UIPageScriptBinding(Div node, string path, string script, string kind, object owner)
      {
        Node = node;
        Path = path ?? string.Empty;
        Script = script ?? string.Empty;
        Kind = kind ?? string.Empty;
        Owner = owner;
      }

      /// <summary>路径是否解析到了节点 (false = 断链).</summary>
      public bool Resolved => Node is not null;
    }

    private static readonly List<IUIPageScript> _scripts = new List<IUIPageScript>();
    private static readonly List<UIPageScriptBinding> _bindings = new List<UIPageScriptBinding>();

    /// <summary>已注册脚本 (诊断用).</summary>
    public static IReadOnlyList<IUIPageScript> Scripts => _scripts;

    /// <summary>最近一次 <see cref="Run"/> 记录的绑定点 (编辑器显示用).</summary>
    public static IReadOnlyList<UIPageScriptBinding> Bindings => _bindings;

    /// <summary>注册脚本; 同类型重复注册以最后一次为准 (避免 ModuleInitializer 跑两遍就双份).</summary>
    public static void Register(IUIPageScript script)
    {
      if (script is null)
        return;
      Type type = script.GetType();
      for (int index = 0; index < _scripts.Count; index++)
      {
        if (_scripts[index].GetType() == type)
        {
          _scripts[index] = script;
          return;
        }
      }
      _scripts.Add(script);
    }

    /// <summary>注册脚本 (无参构造).</summary>
    public static void Register<T>() where T : IUIPageScript, new()
      => Register(new T());

    /// <summary>清空 (测试/热重载用).</summary>
    public static void Clear()
    {
      _scripts.Clear();
      _bindings.Clear();
    }

    internal static void Record(UIPageScript script, Div node, string path, string kind)
    {
      if (script is null)
        return;
      _bindings.Add(new UIPageScriptBinding(node, path, script.GetType().Name, kind, script));
    }

    /// <summary>某个脚本解绑时, 顺手清掉它上一轮的绑定点记录.</summary>
    internal static void ClearBindings(UIPageScript script)
    {
      if (script is null)
        return;
      for (int index = _bindings.Count - 1; index >= 0; index--)
        if (ReferenceEquals(_bindings[index].Owner, script))
          _bindings.RemoveAt(index);
    }

    /// <summary>
    /// 跑一遍适用于该页面的脚本: 先 <see cref="IUIPageScript.Unbind"/> 再 <see cref="IUIPageScript.Bind"/>.
    /// <br>单个脚本抛异常只落日志, 不影响其它脚本与页面本身.</br>
    /// </summary>
    /// <returns>实际执行的脚本数.</returns>
    public static int Run(Div page)
    {
      if (page is null || page.Disposed)
        return 0;
      int count = 0;
      for (int index = 0; index < _scripts.Count; index++)
      {
        IUIPageScript script = _scripts[index];
        string target = script.PageName;
        if (string.IsNullOrEmpty(target) is false && page.Name != target)
          continue;
        try
        {
          script.Unbind();
          script.Bind(page);
          count++;
        }
        catch (Exception exception)
        {
          Console.Log(ConsoleTextType.Error, "UIScript",
            string.Concat("[", page.Name, "] 脚本 ", script.GetType().Name, " 失败: ", exception.Message));
        }
      }
      return count;
    }
  }
}
