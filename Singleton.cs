namespace Colin.Core
{
  /// <summary>
  /// 用于为继承了 <see cref="ISingleton"/> 接口的类提供单例对象.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Singleton<T> where T : ISingleton, new()
  {
    private static T _instance;
    public static T Instance
    {
      get
      {
        if (_instance == null)
          _instance = new T();
        return _instance;
      }
    }
  }
}