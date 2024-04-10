using System.Reflection;

namespace Colin.Core.Resources
{
  public class CodeResourceManager
  {
    private static List<Type> _codeResourceTypes = new List<Type>();
    /// <summary>
    /// 注册代码资产类型.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterCodeResourceType<T>()
    {
      if (_codeResourceTypes.Contains(typeof(T)) || typeof(T).IsNotPublic || typeof(T).IsGenericType || typeof(T).IsEnum || typeof(T).IsValueType)
        EngineConsole.WriteLine(ConsoleTextType.Error, "为代码资产列表注册类型失败: " + typeof(T).Name);
      else
        _codeResourceTypes.Add(typeof(T));
    }
    internal static void LoadCodeResource()
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
}
