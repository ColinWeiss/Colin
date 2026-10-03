namespace Colin.Core.IO
{
  public sealed class TagDouble : Tag
  {
    public double Value;
    public TagDouble() { }
    public TagDouble(double value) => Value = value;
    public override TagType Type => TagType.Double;
    public static implicit operator TagDouble(double value) => new TagDouble(value);
  }
}
