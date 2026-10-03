namespace Colin.Core.IO
{
  public sealed class TagString : Tag
  {
    public string Value;
    public TagString() { }
    public TagString(string value) => Value = value;
    public override TagType Type => TagType.String;
    public static implicit operator TagString(string value) => new TagString(value);
  }
}
