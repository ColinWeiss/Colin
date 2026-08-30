using Microsoft.Xna.Framework.Input;
using System.Reflection;

namespace Colin.Core
{
  /// <summary>
  /// 从设备获取信息.
  /// </summary>
  public class CoreInfo
  {
    public static Random Rand => new Random();

    public static string[] StartupParameter;

    /// <summary>
    ///用于初始化和控制图形设备的显示.
    /// </summary>
    internal static GraphicsDeviceManager Graphics { get; set; }

    /// <summary>
    /// 纹理批管道.
    /// </summary>
    internal static SpriteBatch Batch { get; set; }

    /// <summary>
    /// 游戏刻缓存.
    /// </summary>
    public static GameTime GameTimeCache { get; set; }

    public static Core Core { get; set; }

    /// <summary>
    /// 指示当前程序设置.
    /// </summary>
    public static Config Config { get; set; }

    private static string _engineName = string.Empty;
    /// <summary>
    /// 指示当前程序集的名称.
    /// </summary>
    public static string EngineName
    {
      get
      {
        if (_engineName == string.Empty)
        {
          _engineName = Assembly.GetExecutingAssembly().FullName.Split(',').First();
          return _engineName;
        }
        else
          return _engineName;
      }
      set
      {
        _engineName = value;
      }
    }

    public static int ScreenWidth => Graphics.IsFullScreen ? ViewWidth : Graphics.PreferredBackBufferWidth;

    public static int ScreenHeight => Graphics.IsFullScreen ? ViewHeight : Graphics.PreferredBackBufferHeight;

    public static Point ScreenSize => new Point(ScreenWidth, ScreenHeight);

    public static Vector2 ScreenSizeF => new Vector2(ScreenWidth, ScreenHeight);

    /// <summary>
    /// 视图分辨率宽度.
    /// </summary>
    public static int ViewWidth
    {
      get
      {
        return Graphics.GraphicsDevice.Viewport.Width;
      }
    }

    /// <summary>
    /// 视图分辨率高度.
    /// </summary>
    public static int ViewHeight
    {
      get
      {
        return Graphics.GraphicsDevice.Viewport.Height;
      }
    }

    /// <summary>
    /// 视图分辨率.
    /// </summary>
    public static Vector2 ViewSizeF
    {
      get
      {
        return new Vector2(ViewWidth, ViewHeight);
      }
    }

    /// <summary>
    /// 视图分辨率.
    /// </summary>
    public static Point ViewSize
    {
      get
      {
        return new Point(ViewWidth, ViewHeight);
      }
    }

    /// <summary>
    /// 视图分辨率中心坐标.
    /// </summary>
    public static Vector2 ViewCenter
    {
      get
      {
        return new Vector2(ViewWidth / 2, ViewHeight / 2);
      }
    }

    /// <summary>
    /// 分辨率矩形.
    /// </summary>
    public static Rectangle ViewRectangle
    {
      get
      {
        return new Rectangle(0, 0, ViewWidth, ViewHeight);
      }
    }

    /// <summary>
    /// 当前鼠标信息.
    /// </summary>
    public static MouseState MouseState { get; private set; } = new MouseState();

    /// <summary>
    /// 上一帧鼠标信息.
    /// </summary>
    public static MouseState MouseStateLast { get; private set; } = new MouseState();

    /// <summary>
    /// 输入法处理.
    /// </summary>
    public static IMEHandler IMEHandler;

    /// <summary>
    /// 指示当前程序是否处于调试模式.
    /// </summary>
    public static bool Debug = false;

    /// <summary>
    /// 指示引擎是否运行于嵌入宿主 (如 Avalonia 控件) 之中;
    /// 此时画面由宿主呈现, 独立游戏窗口相关行为 (如全屏切换) 应当被屏蔽.
    /// <br>原生运行模式下本质无作用.</br>
    /// </summary>
    public static bool IsEmbedded = false;

    /// <summary>
    /// 嵌入模式下引擎请求退出; 宿主应当关闭宿主窗口以结束程序.
    /// <br>原生运行模式下本质无作用.</br>
    /// </summary>
    public static event Action? EmbeddedExitRequested;

    internal static void RaiseEmbeddedExitRequested() => EmbeddedExitRequested?.Invoke();

    internal static void Init(Core engine)
    {
      Core = engine;
      IMEHandler = new WinFormsIMEHandler(Core, true);
#if DEBUG
      Debug = true;
#endif
    }

    /// <summary>
    /// 从设备获取信息.
    /// </summary>
    /// <param name="gameTime">游戏刻.</param>
    internal static void GetInformationFromDevice(GameTime gameTime)
    {
      GameTimeCache = gameTime;
      Input.UpdateMouseState();
      MouseStateLast = Input.MouseStateLast;
      MouseState = Input.MouseState;
    }
  }
}
