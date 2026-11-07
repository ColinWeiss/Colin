using System.Reflection;

namespace Colin.Core.Modulars.UserInterfaces.Design
{
  /// <summary>一个可添加的界面类型: 键 + 类型 + 工厂.</summary>
  public sealed class UIDesignType
  {
    public string Key;
    public Type Type;
    public Func<Div> Factory;
  }

  /// <summary>
  /// 编辑器"添加子级"用的界面类型目录 —— 靠<b>反射自动发现</b>, 不需要任何登记表.
  /// <br>收录条件: 非抽象、非泛型、非页面根 (<see cref="DivRoot"/> 不能嵌套)、能用名字构造出来.</br>
  /// <br>[!] 名字即键: 生成代码写 <c>new T("名字")</c>, 所以"能用名字构造"是硬条件.</br>
  /// </summary>
  public static class UIDesignTypes
  {
    private static List<UIDesignType> _all;
    private static readonly HashSet<string> _ambiguous = new HashSet<string>(StringComparer.Ordinal);
    private static int _assemblyCount = -1;

    /// <summary>全部可添加类型 (按键排序).</summary>
    public static IReadOnlyList<UIDesignType> All
    {
      get
      {
        //程序集数量变了就重扫: 编辑器/热重载会晚到地加载插件, 缓存一辈子会让新类型永远不出现在列表里.
        int count = AppDomain.CurrentDomain.GetAssemblies().Length;
        if (_all is null || count != _assemblyCount)
        {
          _all = Collect();
          _assemblyCount = count;
        }
        return _all;
      }
    }

    /// <summary>丢弃缓存 (手工触发重扫).</summary>
    public static void Invalidate()
    {
      _all = null;
      _assemblyCount = -1;
    }

    /// <summary>可添加类型的键 (供下拉框).</summary>
    public static List<string> Keys()
    {
      List<string> keys = new List<string>(All.Count);
      for (int index = 0; index < All.Count; index++)
        keys.Add(All[index].Key);
      return keys;
    }

    /// <summary>按键取类型; 未知键返回 null.</summary>
    public static UIDesignType Find(string key)
    {
      if (string.IsNullOrEmpty(key))
        return null;
      IReadOnlyList<UIDesignType> all = All;
      for (int index = 0; index < all.Count; index++)
      {
        if (all[index].Key == key)
          return all[index];
      }
      return null;
    }

    /// <summary>按类型取键 (类型不存在于目录里时返回类型短名, 仅用于显示).</summary>
    public static string KeyOf(Type type)
    {
      if (type is null)
        return "null";
      IReadOnlyList<UIDesignType> all = All;
      for (int index = 0; index < all.Count; index++)
      {
        if (all[index].Type == type)
          return all[index].Key;
      }
      return type.Name;
    }

    /// <summary>按类型新建一个未初始化的元素; 不支持的类型返回 null.</summary>
    public static Div Create(Type type, string name)
    {
      if (type is null || type.IsAbstract || typeof(Div).IsAssignableFrom(type) is false)
        return null;
      //页面根不能当子级嵌套: 造出来只会是一棵假的界面
      if (typeof(DivRoot).IsAssignableFrom(type))
        return null;
      ConstructorInfo[] constructors = type.GetConstructors();
      for (int index = 0; index < constructors.Length; index++)
      {
        ParameterInfo[] parameters = constructors[index].GetParameters();
        if (parameters.Length == 0)
        {
          Div instance = (Div)constructors[index].Invoke(null);
          if (string.IsNullOrEmpty(name) is false)
            instance.Rename(name);
          return instance;
        }
        if (parameters[0].ParameterType != typeof(string) || RestOptional(parameters) is false)
          continue;
        object[] arguments = new object[parameters.Length];
        arguments[0] = string.IsNullOrEmpty(name) ? type.Name : name;
        for (int cursor = 1; cursor < parameters.Length; cursor++)
          arguments[cursor] = parameters[cursor].HasDefaultValue ? parameters[cursor].DefaultValue : null;
        return (Div)constructors[index].Invoke(arguments);
      }
      return null;
    }

    private static bool RestOptional(ParameterInfo[] parameters)
    {
      for (int index = 1; index < parameters.Length; index++)
      {
        if (parameters[index].IsOptional is false)
          return false;
      }
      return true;
    }

    private static List<UIDesignType> Collect()
    {
      List<UIDesignType> result = new List<UIDesignType>(128);
      Dictionary<string, Type> byKey = new Dictionary<string, Type>(StringComparer.Ordinal);
      _ambiguous.Clear();
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
      {
        Assembly assembly = assemblies[assemblyIndex];
        if (assembly.IsDynamic)
          continue;
        Type[] types;
        try
        {
          types = assembly.GetTypes();
        }
        catch
        {
          //反射加载失败的程序集 (缺依赖) 直接跳过: 编辑器不该因为一个坏插件打不开
          continue;
        }
        for (int index = 0; index < types.Length; index++)
        {
          Type type = types[index];
          if (type.IsAbstract || type.IsGenericTypeDefinition || type.IsNested)
            continue;
          if (typeof(Div).IsAssignableFrom(type) is false)
            continue;
          if (typeof(DivRoot).IsAssignableFrom(type))
            continue;
          if (CanConstruct(type) is false)
            continue;
          string key = type.Name;
          if (byKey.TryGetValue(key, out Type existing) && existing != type)
          {
            //跨命名空间撞名: 都排除, 免得编辑器里点一下出来的是另一个
            _ambiguous.Add(key);
            continue;
          }
          byKey[key] = type;
          result.Add(new UIDesignType
          {
            Key = key,
            Type = type,
            Factory = () => Create(type, key) ?? new Div(key),
          });
        }
      }
      if (_ambiguous.Count > 0)
        result.RemoveAll(entry => _ambiguous.Contains(entry.Key));
      result.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));
      return result;
    }

    private static bool CanConstruct(Type type)
    {
      ConstructorInfo[] constructors = type.GetConstructors();
      for (int index = 0; index < constructors.Length; index++)
      {
        ParameterInfo[] parameters = constructors[index].GetParameters();
        if (parameters.Length == 0)
          return true;
        if (parameters[0].ParameterType == typeof(string) && RestOptional(parameters))
          return true;
      }
      return false;
    }
  }
}
