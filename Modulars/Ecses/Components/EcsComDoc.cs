namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 切片文档.
  /// </summary>
  public class EcsComDoc : ISectionBindableCom
  {
    public Section Section { get; set; }

    private string identifier;

    /// <summary>
    /// 指示切片的标识符.
    /// <br>切片判断相等的依据之一.</br>
    /// </summary>
    public string Identifier
    {
      get
      {
        identifier ??= Section.GetType().Name;
        return identifier;
      }
    }

    public string Description = "";
    /// <summary>
    /// 指示切片标签.
    /// </summary>
    public HashSet<string> Tags = new HashSet<string>();

    public void DoInitialize() { }

    public bool Equals(ISectionCom other)
    {
      bool result = false;
      if (other is EcsComDoc doc)
      {
        result = Identifier.Equals(doc.Identifier);
        return result && Tags.SetEquals(doc.Tags);
      }
      return result;
    }
  }
}