namespace Colin.Core.IO
{
  public sealed class TagShort : Tag
  {
    public short Value;
    public TagShort() { }
    public TagShort(short value) => Value = value;
    public override TagType Type => TagType.Short;
    public static implicit operator TagShort(short value) => new TagShort(value);
  }
}
