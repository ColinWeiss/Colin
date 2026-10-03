namespace Colin.Core.IO
{
  public sealed class TagByte : Tag
  {
    public byte Value;
    public TagByte() { }
    public TagByte(byte value) => Value = value;
    public override TagType Type => TagType.Byte;
    public static implicit operator TagByte(byte value) => new TagByte(value);
  }
}
