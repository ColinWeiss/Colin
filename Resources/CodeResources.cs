using System.Reflection;
using System.Text.Json;

namespace Colin.Core.Resources
{
  public class CodeResources
  {
    private static List<Type> _codeResourceTypes = new List<Type>();
    /// <summary>
    /// 注册代码资产类型.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Register<T>()
    {
      if (_codeResourceTypes.Contains(typeof(T)) || typeof(T).IsNotPublic || typeof(T).IsGenericType || typeof(T).IsEnum || typeof(T).IsValueType)
        Console.Log(ConsoleTextType.Error, "Resource", "为代码资产列表注册类型失败: " + typeof(T).Name);
      else
        _codeResourceTypes.Add(typeof(T));
    }

    internal static void Load()
    {
      foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (!item.IsAbstract && _codeResourceTypes.Contains(item) && item.GetInterfaces().Contains(typeof(ICodeRes)))
        {
          Type resources = typeof(CodeResources<>);
          Type resource = resources.MakeGenericType(item);
          resource.GetMethod("Load").Invoke(Activator.CreateInstance(resource), null);
        }
      }
    }
  }
  public class CodeResources<T0> where T0 : ICodeRes
  {
    public static Dictionary<Type, T0> Resources = new Dictionary<Type, T0>();
    public static Dictionary<string, int> serToHashs = new Dictionary<string, int>();
    private static Dictionary<int, string> hashToSers = new Dictionary<int, string>();
    private static Dictionary<string, Type> serToResourceTypes = new Dictionary<string, Type>();

    // 外部来源(模组)注册的原型底账:Load 只扫游戏本体程序集,模组类型必须走 Register 进来,
    // 而 Load 会整表重建,所以这里要记账,扫完之后把它们补回表里,注册时序就不用挑了。
    private static readonly List<T0> _externalPrototypes = new List<T0>();

    /// <summary>
    /// 外部注册一个原型实例,模组往游戏里加物品/实体就走这里。
    /// <br>Load 的反射扫描只认游戏本体程序集,模组程序集里的类型它看不见,必须用这个口子挂号;
    /// 注册之后存档、生成、调试面板这些系统就对模组内容和原生内容一视同仁了。</br>
    /// </summary>
    public static void Register(T0 prototype)
    {
      if (prototype is null)
      {
        Console.Log(ConsoleTextType.Error, "Resource", "往代码资产表注册了空原型, 已忽略.");
        return;
      }
      Type type = prototype.GetType();
      if (Resources.ContainsKey(type) || serToResourceTypes.ContainsKey(type.FullName))
      {
        Console.Log(ConsoleTextType.Warning, "Resource", "代码资产表里已有该类型, 跳过注册: " + type.FullName);
        return;
      }
      _externalPrototypes.Add(prototype);
      AddToTable(prototype);
      Console.Log(ConsoleTextType.Remind, "Resource", "外部类型: " + type.FullName);
    }

    /// <summary>把一个外部注册的类型从表里摘掉(模组卸载时用);摘到了返回 true.</summary>
    public static bool Unregister(Type type)
    {
      bool removedFromLedger = false;
      for (int count = _externalPrototypes.Count - 1; count >= 0; count--)
      {
        if (_externalPrototypes[count].GetType() == type)
        {
          _externalPrototypes.RemoveAt(count);
          removedFromLedger = true;
        }
      }
      return RemoveFromTable(type) || removedFromLedger;
    }

    private static void AddToTable(T0 prototype)
    {
      Type type = prototype.GetType();
      int hash = type.FullName.GetMsnHashCode();
      // 统一用索引器赋值而不是 Add:Load 原本不清 serToResourceTypes,
      // 重建表之后补回外部原型时按 Add 走会撞键,索引器写法对重复注册天然幂等。
      Resources[type] = prototype;
      serToResourceTypes[type.FullName] = type;
      serToHashs[type.FullName] = hash;
      hashToSers[hash] = type.FullName;
    }

    private static bool RemoveFromTable(Type type)
    {
      if (Resources.Remove(type) is false)
        return false;
      if (serToHashs.TryGetValue(type.FullName, out int hash))
      {
        serToResourceTypes.Remove(type.FullName);
        serToHashs.Remove(type.FullName);
        hashToSers.Remove(hash);
      }
      return true;
    }

    public static T1 Get<T1>() where T1 : T0 => (T1)Resources.GetValueOrDefault(typeof(T1));
    public static T0 GetFromType(Type type)
    {
      if (Resources.TryGetValue(type, out T0 value))
        return value;
      else return default;
    }
    public static T0 GetFromTypeName(string typeName)
    {
      if (serToResourceTypes.TryGetValue(typeName, out Type type))
        return GetFromType(type);
      else return default;
    }
    public static T0 GetFromHash(int hashValue)
    {
      return GetFromTypeName(GetTypeNameFromHash(hashValue));
    }

    public static string GetTypeNameFromHash(int hashValue)
    {
      if (hashToSers.TryGetValue(hashValue, out string value))
        return value;
      else
        return null;
    }
    public static int? GetHashFromTypeName(string typeName)
    {
      if (serToHashs.TryGetValue(typeName, out int value))
      {
        return value;
      }
      else
        return null;
    }

    public static T1 CreateNewInstance<T1>() where T1 : T0
    {
      return (T1)Activator.CreateInstance(typeof(T1));
    }
    public static T0 CreateNewInstance(T0 t)
    {
      return (T0)Activator.CreateInstance(t.GetType());
    }
    public static T0 CreateNewInstance(int hashValue)
    {
      string typeName = GetTypeNameFromHash(hashValue);
      return (T0)Activator.CreateInstance(GetFromTypeName(typeName).GetType());
    }

    /// <summary>
    /// 按类型名建实例;查不到(模组被卸载等)返回 default 而不是炸,读档端好走占位转换。
    /// <br>存档里现在直接写类型名,这条比按哈希查的更常用。</br>
    /// </summary>
    public static T0 TryCreateFromTypeName(string typeName)
    {
      if (typeName is null)
        return default;
      T0 prototype = GetFromTypeName(typeName);
      if (prototype is null)
        return default;
      return (T0)Activator.CreateInstance(prototype.GetType());
    }

    /// <summary>
    /// 按哈希建实例;查不到(模组被卸载等)返回 default 而不是炸,读档端好走占位转换。
    /// </summary>
    public static T0 TryCreateNewInstance(int hashValue)
    {
      string typeName = GetTypeNameFromHash(hashValue);
      if (typeName is null)
        return default;
      T0 prototype = GetFromTypeName(typeName);
      if (prototype is null)
        return default;
      return (T0)Activator.CreateInstance(prototype.GetType());
    }

    public void Load()
    {
      Resources.Clear();
      serToHashs.Clear();
      foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (!item.IsAbstract && item.IsSubclassOf(typeof(T0)))
        {
          T0 obj = (T0)Activator.CreateInstance(item);
          if (obj is ICodeResPreload pre)
            pre.PreLoad();
          Resources.Add(item, obj);
          serToResourceTypes.Add(item.FullName, item);
          serToHashs.Add(item.FullName, item.FullName.GetMsnHashCode());
        }
      }
      hashToSers.Clear();
      foreach (var item in serToHashs)
        hashToSers.Add(item.Value, item.Key);
      // 本体类型扫完重建后, 把模组注册的原型重新铺回表里, 外部注册就不怕撞上 Load 的时序了
      foreach (T0 prototype in _externalPrototypes)
        AddToTable(prototype);
    }

    public static void SaveTable(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Create))
      {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        JsonSerializer.Serialize(fileStream, serToHashs, serToHashs.GetType(), options);
      }
    }
    public static void LoadTable(string path)
    {
      // 新存档还没有表文件: 保留启动时构建的注册表, 这里绝不能先清后读
      // 以前先 Clear 再开文件, 文件一缺反向表就被掏空, 之后所有哈希查询全返回 null
      if (File.Exists(path) is false)
      {
        Console.Log(ConsoleTextType.Remind, "Resource", string.Concat("代码资产表不存在, 沿用运行时注册表: ", path));
        return;
      }
      Dictionary<string, int> loaded;
      using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        loaded = JsonSerializer.Deserialize<Dictionary<string, int>>(fileStream);
      Dictionary<string, int> merged = new Dictionary<string, int>(serToHashs);
      foreach (var item in loaded)
        merged.TryAdd(item.Key, item.Value);
      Dictionary<int, string> reversed = new Dictionary<int, string>();
      foreach (var item in merged)
      {
        if (reversed.ContainsKey(item.Value) is false)
          reversed.Add(item.Value, item.Key);
      }
      serToHashs = merged;
      hashToSers = reversed;
    }
  }
}