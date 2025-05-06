using Colin.Core.Modulars.Tiles;
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
        Console.WriteLine("Error", "为代码资产列表注册类型失败: " + typeof(T).Name);
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

    public static T1 Get<T0, T1>() where T0 : ICodeResource where T1 : T0
    {
      return CodeResources<T0>.Get<T1>();
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
      return (T0)Activator.CreateInstance(typeof(T0));
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
      hashToSers.Clear();
      using (FileStream fileStream = new FileStream(path, FileMode.Open))
      {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        serToHashs = (Dictionary<string, int>)JsonSerializer.Deserialize(fileStream, serToHashs.GetType());
        foreach (var item in serToHashs)
          hashToSers.Add(item.Value, item.Key);
      }
    }
  }
}