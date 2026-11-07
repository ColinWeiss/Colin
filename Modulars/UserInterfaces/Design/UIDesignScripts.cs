using System.Reflection;

namespace Colin.Core.Modulars.UserInterfaces.Design
{
  /// <summary>一个可挂到 Div 上的脚本: 显示名 + 类型 + 工厂.</summary>
  public sealed class UIDesignScriptType
  {
    public string Key;
    public Type Type;
    public Func<DivScript> Factory;
  }

  /// <summary>
  /// 编辑器"给这个 Div 选脚本"用的目录 —— 靠反射自动发现所有可无参构造的 <see cref="DivScript"/> 子类.
  /// <br>节点不按类区分, 所以"这个节点是什么控件"完全由挂的脚本决定.</br>
  /// </summary>
  public static class UIDesignScripts
  {
    private static List<UIDesignScriptType> _all;
    private static int _assemblyCount = -1;

    /// <summary>全部可挂脚本 (按键排序).</summary>
    public static IReadOnlyList<UIDesignScriptType> All
    {
      get
      {
        int count = AppDomain.CurrentDomain.GetAssemblies().Length;
        if (_all is null || count != _assemblyCount)
        {
          _all = Collect();
          _assemblyCount = count;
        }
        return _all;
      }
    }

    /// <summary>丢弃缓存 (插件/热重载后重扫).</summary>
    public static void Invalidate()
    {
      _all = null;
      _assemblyCount = -1;
    }

    /// <summary>可挂脚本的键 (供下拉框): 类型名去掉 Script 后缀.</summary>
    public static List<string> Keys()
    {
      List<string> keys = new List<string>(All.Count);
      for (int index = 0; index < All.Count; index++)
        keys.Add(All[index].Key);
      return keys;
    }

    /// <summary>按键取脚本类型; 未知键返回 null.</summary>
    public static UIDesignScriptType Find(string key)
    {
      if (string.IsNullOrEmpty(key))
        return null;
      IReadOnlyList<UIDesignScriptType> all = All;
      for (int index = 0; index < all.Count; index++)
        if (all[index].Key == key)
          return all[index];
      return null;
    }

    /// <summary>按实例取键 (用于下拉框回显).</summary>
    public static string KeyOf(DivScript script)
    {
      if (script is null)
        return string.Empty;
      return script.GetType().Name.EndsWith("Script", StringComparison.Ordinal)
        ? script.GetType().Name.Substring(0, script.GetType().Name.Length - "Script".Length)
        : script.GetType().Name;
    }

    private static List<UIDesignScriptType> Collect()
    {
      List<UIDesignScriptType> result = new List<UIDesignScriptType>(32);
      HashSet<string> used = new HashSet<string>(StringComparer.Ordinal);
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
          //反射加载失败的程序集 (缺依赖) 直接跳过: 编辑器不该因为一个坏插件打不开.
          continue;
        }
        for (int index = 0; index < types.Length; index++)
        {
          Type type = types[index];
          if (type.IsAbstract || type.IsGenericTypeDefinition || type.IsNested)
            continue;
          if (typeof(DivScript).IsAssignableFrom(type) is false)
            continue;
          if (type.GetConstructor(Type.EmptyTypes) is null)
            continue;
          string key = KeyOfName(type);
          if (used.Add(key) is false)
            continue;
          Type captured = type;
          result.Add(new UIDesignScriptType
          {
            Key = key,
            Type = captured,
            Factory = () => (DivScript)Activator.CreateInstance(captured),
          });
        }
      }
      result.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));
      return result;
    }

    private static string KeyOfName(Type type)
      => type.Name.EndsWith("Script", StringComparison.Ordinal)
        ? type.Name.Substring(0, type.Name.Length - "Script".Length)
        : type.Name;
  }
}
