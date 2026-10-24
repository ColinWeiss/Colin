namespace Colin.Core.Modulars.UserInterfaces.Documents
{
  using Colin.Core.Modulars.UserInterfaces.Prefabs;

  /// <summary>
  /// 界面文档的类型注册表: 文档节点类型键 ↔ Div 工厂.
  /// <br>引擎预制件默认全注册; 游戏与模组的手写 Div 子类按 WPF 自定义控件的方式调 <see cref="Register"/> 登记后,</br>
  /// <br>即可作为文档节点类型被保存与还原; 有无参构造的子类未登记时按全类型名自动注册兜底.</br>
  /// </summary>
  public static class UIDocumentRegistry
  {
    private static readonly Dictionary<string, Func<Div>> _factories = new Dictionary<string, Func<Div>>();
    private static readonly Dictionary<Type, string> _keys = new Dictionary<Type, string>();
    private static bool _defaultsEnsured;

    /// <summary>
    /// 全部已注册的类型键与工厂; 编辑器的元素添加清单由此枚举.
    /// </summary>
    public static IReadOnlyDictionary<string, Func<Div>> Factories => _factories;

    /// <summary>
    /// 登记一个界面类型; 重复键覆盖.
    /// </summary>
    public static void Register(string key, Type type, Func<Div> factory)
    {
      _factories[key] = factory;
      if (_keys.ContainsKey(type) is false)
        _keys[type] = key;
    }

    /// <summary>
    /// 以无参构造登记一个界面类型.
    /// </summary>
    public static void Register<T>(string key) where T : Div, new()
      => Register(key, typeof(T), () => new T());

    /// <summary>
    /// 按类型键构造一个未初始化的划分元素; 未知键返回 null 并记录错误.
    /// </summary>
    public static Div Create(string key)
    {
      EnsureDefaults();
      if (_factories.TryGetValue(key, out Func<Div> factory))
        return factory();
      Console.Log(ConsoleTextType.Error, "UIDocument", string.Concat("未注册的界面类型: ", key ?? "null"));
      return null;
    }

    /// <summary>
    /// 查询类型对应的文档键; 未登记但有无参构造的 Div 子类自动按全类型名登记兜底.
    /// <br>既未登记也没有无参构造时返回 null —— 该类型须显式 <see cref="Register"/> 后才可文档化.</br>
    /// </summary>
    public static string GetKey(Type type)
    {
      EnsureDefaults();
      if (_keys.TryGetValue(type, out string key))
        return key;
      if (type.IsSubclassOf(typeof(Div)) && type.GetConstructor(Type.EmptyTypes) is not null)
      {
        string autoKey = type.FullName;
        Register(autoKey, type, () => (Div)Activator.CreateInstance(type));
        return autoKey;
      }
      return null;
    }

    /// <summary>
    /// 确保引擎预制件已登记; 首次使用时自动触发.
    /// </summary>
    public static void EnsureDefaults()
    {
      if (_defaultsEnsured)
        return;
      _defaultsEnsured = true;
      Register("Div", typeof(Div), () => new Div("Div"));
      Register("DivRoot", typeof(DivRoot), () => new DivRoot("Root"));
      Register("Label", typeof(Label), () => new Label("Label"));
      Register("Button", typeof(Button), () => new Button("Button"));
      Register("InputTextBox", typeof(InputTextBox), () => new InputTextBox("InputTextBox"));
      Register("Slider", typeof(Slider), () => new Slider("Slider"));
      Register("ProgressBar", typeof(ProgressBar), () => new ProgressBar("ProgressBar"));
      Register("DivList", typeof(DivList), () => new DivList("DivList"));
      Register("ScrollView", typeof(ScrollView), () => new ScrollView("ScrollView"));
    }
  }
}
