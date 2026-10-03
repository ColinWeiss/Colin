namespace Colin.Core.IO
{
  public sealed class TagBool : Tag
  {
    public bool Value;
    public TagBool() { }
    public TagBool(bool value) => Value = value;
    public override TagType Type => TagType.Bool;
    public static implicit operator TagBool(bool value) => new TagBool(value);
  }
}
