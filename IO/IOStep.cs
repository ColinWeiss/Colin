namespace Colin.Core.IO
{
  /// <summary>
  /// 指示一个可进行文件保存/读取的对象.
  /// </summary>
  public interface IOStep
  {
    StoreBox SaveStep();
    void LoadStep(StoreBox box);
  }
}