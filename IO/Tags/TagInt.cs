namespace Colin.Core.IO
{
  public sealed class TagInt : Tag
  {
    public int Value;
    public TagInt() { }
    public TagInt(int value) => Value = value;
    public override TagType Type => TagType.Int;
    public static implicit operator TagInt(int value) => new TagInt(value);
  }
}
