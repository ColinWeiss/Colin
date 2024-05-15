namespace Colin.Core.Modulars
{
  public interface IGameContent<T>
  {
    /// <summary>
    /// 以该对象的类型实例化一个实例对象.
    /// </summary>
    /// <returns></returns>
    public T CreateInstance()
    {
      return (T)Activator.CreateInstance(GetType());
    }
  }
}