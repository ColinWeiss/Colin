namespace Colin.Core.IO
{
  public sealed class TagFloat : Tag
  {
    public float Value;
    public TagFloat() { }
    public TagFloat(float value) => Value = value;
    public override TagType Type => TagType.Float;
    public static implicit operator TagFloat(float value) => new TagFloat(value);
  }
}
