using Colin.Core.IO;

namespace Colin.Core.Modulars.UserInterfaces.Documents
{
  /// <summary>
  /// 声明划分元素自带文档序列化能力 —— WPF 式"用户自定义预制件"的扩展点.
  /// <br>实现它的 Div 子类视作自定义控件: 文档保存/读取时引擎会在标准槽位 (Layout/Style/Renderer...) 之外追加调用这里,</br>
  /// <br>控件自身的专有字段由此进出一节 <see cref="TagCompound"/>, 引擎不需要知道它的存在.</br>
  /// </summary>
  public interface IUIDocumentCustomizable
  {
    /// <summary>
    /// 把自定义数据写进专有节; 传出的 Compound 为空则不写入文档.
    /// </summary>
    void WriteDocumentCustom(TagCompound tag);

    /// <summary>
    /// 从专有节还原自定义数据; 于标准槽位全部还原之后调用.
    /// </summary>
    void ReadDocumentCustom(TagCompound tag);
  }
}
