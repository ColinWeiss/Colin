namespace Colin.Core.IO
{
  /// <summary>
  /// 一串同类型的值, 顺序就是身份.
  /// <br>适合"个数不定、每个都长一样"的数据, 比如背包里的一列物品、一串实体槽位.</br>
  /// </summary>
  public sealed class TagSeq : Tag
  {
    /// <summary>元素类型; 建表时不知道就先空着, 塞进第一个元素后自动补上.</summary>
    public TagType ElementType;
    public readonly List<Tag> Items = new List<Tag>();
    public TagSeq() { }
    public TagSeq(TagType elementType) => ElementType = elementType;
    public override TagType Type => TagType.List;
    public int Count => Items.Count;
    public Tag this[int index] => Items[index];
    public void Add(Tag item)
    {
      if (ElementType == TagType.End)
        ElementType = item.Type;
      Items.Add(item);
    }
    public TagCompound GetCompound(int index) => Items[index] as TagCompound;
    public int GetInt(int index, int fallback = 0) => Items[index] is TagInt value ? value.Value : fallback;
    public string GetString(int index, string fallback = "") => Items[index] is TagString value ? value.Value : fallback;
  }
}
