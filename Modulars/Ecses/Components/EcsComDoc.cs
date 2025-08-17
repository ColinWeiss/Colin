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
    /// 名称.
    /// </summary>
    public string Name;

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

    /// <summary>
    /// 为实体添加标签.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void AddTag(string tag) => Tags.Add(tag);

    /// <summary>
    /// 判断该实体是否具有指定标签.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasTag(string tag) => Tags.Contains(tag);

    public void DoInitialize() 
    {
      Name ??= "未配置名称";
    }

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