using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
  public class Input
  {
    /// <summary>
    /// 当前鼠标状态; 坐标为游戏坐标系 (backbuffer 像素).
    /// </summary>
    public static MouseState MouseState { get; internal set; } = new MouseState();

    /// <summary>
    /// 上一帧的鼠标状态.
    /// </summary>
    public static MouseState MouseStateLast { get; internal set; } = new MouseState();

    /// <summary>
    /// 鼠标状态来源; 嵌入宿主 (如 Avalonia) 时由宿主设置,
    /// 产出游戏坐标系下的状态; 为 null 时回退到 MonoGame 原生轮询.
    /// </summary>
    public static Func<MouseState>? MouseStateSource { get; set; }

    /// <summary>
    /// 推进一帧鼠标状态; 每帧只应调用一次 (目前由 <see cref="CoreInfo"/> 在游戏刻开头调用).
    /// </summary>
    public static void UpdateMouseState()
    {
      MouseStateLast = MouseState;
      MouseState = MouseStateSource?.Invoke() ?? Mouse.GetState();
    }

    private static List<Keys> _numbers = new List<Keys>
    {
       Keys.D0,
       Keys.D1,
       Keys.D2,
       Keys.D3,
       Keys.D4,
       Keys.D5,
       Keys.D6,
       Keys.D7,
       Keys.D8,
       Keys.D9,
       Keys.NumPad0,
       Keys.NumPad1,
       Keys.NumPad2,
       Keys.NumPad3,
       Keys.NumPad4,
       Keys.NumPad5,
       Keys.NumPad6,
       Keys.NumPad7,
       Keys.NumPad8,
       Keys.NumPad9
    };

    public static bool IsNumberKey(Keys keys)
    {
      return _numbers.Contains(keys);
    }

    public static int GetNumber(Keys keys)
    {
      int index = _numbers.FindIndex(a => a == keys);
      if (index == -1)
        return -1;
      return index % 10;
    }

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