namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 实体文档.
  /// </summary>
  public class EcsComDoc : IEntityBindableCom
  {
    public Entity Entity { get; set; }

    private string identifier;

    /// <summary>
    /// 指示实体的标识符.
    /// <br>实体判断相等的依据之一.</br>
    /// </summary>
    public string Identifier
    {
      get
      {
        identifier ??= Entity.GetType().Name;
        return identifier;
      }
    }

    public string Description = "";
    /// <summary>
    /// 指示实体标签.
    /// </summary>
    public HashSet<string> Tags = new HashSet<string>();

    public void DoInitialize() { }

    public bool Equals(IEntityCom other)
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