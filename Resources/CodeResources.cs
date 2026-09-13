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
        if (!item.IsAbstract && _codeResourceTypes.Contains(item) && item.GetInterfaces().Contains(typeof(ICodeResource)))
        {
          Type resources = typeof(CodeResources<>);
          Type resource = resources.MakeGenericType(item);
          resource.GetMethod("Load").Invoke(Activator.CreateInstance(resource), null);
        }
      }
    }
  }
  public class CodeResources<T0> where T0 : ICodeResource
  {
    public static Dictionary<Type, T0> Resources = new Dictionary<Type, T0>();
    public static Dictionary<string, int> serToHashs = new Dictionary<string, int>();
    private static Dictionary<int, string> hashToSers = new Dictionary<int, string>();
    private static Dictionary<string, Type> serToResourceTypes = new Dictionary<string, Type>();

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

    public void Load()
    {
      Resources.Clear();
      serToHashs.Clear();
      foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (!item.IsAbstract && item.IsSubclassOf(typeof(T0)))
        {
          Resources.Add(item, (T0)Activator.CreateInstance(item));
          serToResourceTypes.Add(item.FullName, item);
          serToHashs.Add(item.FullName, item.FullName.GetMsnHashCode());
        }
      }
      hashToSers.Clear();
      foreach (var item in serToHashs)
        hashToSers.Add(item.Value, item.Key);
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
      // 合并语义: 当前程序集的类型以注册表为准, 表里多出来的历史名字补进来, 老存档才能照常解析
      // 全部在临时状态里准备妥当再原子替换, 中途任何一步失败都不会把注册表掏空
      foreach (var item in loaded)
        serToHashs.TryAdd(item.Key, item.Value);
      Dictionary<int, string> reversed = new Dictionary<int, string>();
      foreach (var item in serToHashs)
      {
        if (reversed.ContainsKey(item.Value) is false)
          reversed.Add(item.Value, item.Key);
      }
      hashToSers = reversed;
    }
  }
}