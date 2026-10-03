namespace Colin.Core.IO
{
  public sealed class TagLong : Tag
  {
    public long Value;
    public TagLong() { }
    public TagLong(long value) => Value = value;
    public override TagType Type => TagType.Long;
    public static implicit operator TagLong(long value) => new TagLong(value);
  }
}
