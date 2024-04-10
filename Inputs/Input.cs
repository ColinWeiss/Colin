namespace Colin.Core.Inputs
{
  public class Input
  {
    public static List<Keys> NumberKeys = new List<Keys>
        {
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
            Keys.D0,
        };

    public static bool LegalInput(string text)
    {
      foreach (var item in text)
      {
        if (IsChinese(item) || char.IsLetterOrDigit(item) || text == " " || text == "/" || text == ".")
          continue;
        else
          return false;
      }
      return true;
    }
    public static bool IsChinese(char theChar)
    {
      if (theChar >= 0x4e00 && theChar <= 0x9fbb)
        return true;
      return false;
    }
  }
}