using Colin.Core.IO;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Documents
{
  /// <summary>
  /// 渲染器/控制器的文档序列化单元: 类型键 + 工厂 + 参数读写.
  /// </summary>
  public sealed class UIDocumentPart<T> where T : class
  {
    public readonly string Kind;
    public readonly Type PartType;
    public readonly Func<T> Factory;
    public readonly Action<T, TagCompound> Write;
    public readonly Action<T, TagCompound> Read;

    public UIDocumentPart(string kind, Type partType, Func<T> factory, Action<T, TagCompound> write, Action<T, TagCompound> read)
    {
      Kind = kind;
      PartType = partType;
      Factory = factory;
      Write = write;
      Read = read;
    }
  }

  /// <summary>
  /// 渲染器的文档序列化登记表; 引擎自带渲染器默认全注册, 自定义渲染器同法登记.
  /// </summary>
  public static class UIDocumentRendererParts
  {
    private static readonly Dictionary<string, UIDocumentPart<DivRenderer>> _parts = new Dictionary<string, UIDocumentPart<DivRenderer>>();
    private static readonly Dictionary<Type, UIDocumentPart<DivRenderer>> _byType = new Dictionary<Type, UIDocumentPart<DivRenderer>>();
    private static bool _defaultsEnsured;

    public static IReadOnlyDictionary<string, UIDocumentPart<DivRenderer>> Parts => _parts;

    public static void Register<K>(string kind, Func<K> factory, Action<K, TagCompound> write, Action<K, TagCompound> read) where K : DivRenderer
    {
      UIDocumentPart<DivRenderer> part = new UIDocumentPart<DivRenderer>(
        kind, typeof(K), () => (DivRenderer)(object)factory(),
        (renderer, tag) => write((K)(object)renderer, tag),
        (renderer, tag) => read((K)(object)renderer, tag));
      _parts[kind] = part;
      _byType[typeof(K)] = part;
    }

    public static UIDocumentPart<DivRenderer> Get(string kind)
    {
      EnsureDefaults();
      if (_parts.TryGetValue(kind, out UIDocumentPart<DivRenderer> part))
        return part;
      Console.Log(ConsoleTextType.Error, "UIDocument", string.Concat("未注册的渲染器类型: ", kind ?? "null"));
      return null;
    }

    public static UIDocumentPart<DivRenderer> Find(DivRenderer renderer)
    {
      EnsureDefaults();
      return _byType.TryGetValue(renderer.GetType(), out UIDocumentPart<DivRenderer> part) ? part : null;
    }

    private static void EnsureDefaults()
    {
      if (_defaultsEnsured)
        return;
      _defaultsEnsured = true;
      Register("Pixel", () => new DivPixelRenderer(), (DivPixelRenderer _, TagCompound __) => { }, (DivPixelRenderer _, TagCompound __) => { });
      Register("Texture", () => new DivTextureRenderer(),
        (DivTextureRenderer r, TagCompound t) =>
        {
          t["Path"] = r.Sprite?.RawPath ?? string.Empty;
          t["Adaptive"] = r.Adaptive;
        },
        (DivTextureRenderer r, TagCompound t) =>
        {
          string path = t.GetString("Path");
          if (string.IsNullOrEmpty(path) is false)
            r.Bind(path);
          r.Adaptive = t.GetBool("Adaptive");
        });
      Register("Ninecut", () => new DivNinecutRenderer(),
        (DivNinecutRenderer r, TagCompound t) =>
        {
          t["Path"] = r.Sprite?.RawPath ?? string.Empty;
          t["CutX"] = r.Cut.X;
          t["CutY"] = r.Cut.Y;
        },
        (DivNinecutRenderer r, TagCompound t) =>
        {
          string path = t.GetString("Path");
          if (string.IsNullOrEmpty(path) is false)
            r.Bind(path);
          r.SetCut(t.GetInt("CutX"), t.GetInt("CutY"));
        });
      Register("Font", () => new DivFontRenderer(),
        (DivFontRenderer r, TagCompound t) =>
        {
          t["Text"] = r.Text ?? string.Empty;
          t["FontPath"] = r.FontPath ?? string.Empty;
          t["FontSize"] = r.FontSize;
          t["Shadow"] = r.TextShadow;
        },
        (DivFontRenderer r, TagCompound t) =>
        {
          string path = t.GetString("FontPath");
          if (string.IsNullOrEmpty(path) is false)
            r.BindFont(path, t.GetInt("FontSize", 16));
          r.TextShadow = t.GetBool("Shadow", true);
          r.Text = t.GetString("Text");
        });
      Register("Rounded", () => new DivRoundedRenderer(),
        (DivRoundedRenderer r, TagCompound t) =>
        {
          t["Radius"] = r.Radius;
          t["BorderWidth"] = r.BorderWidth;
          t["BorderColor"] = UIDocument.PackColor(r.BorderColor);
        },
        (DivRoundedRenderer r, TagCompound t) =>
        {
          r.Radius = t.GetFloat("Radius");
          r.BorderWidth = t.GetFloat("BorderWidth");
          r.BorderColor = UIDocument.UnpackColor(t.GetInt("BorderColor"));
        });
      Register("SdfIcon", () => new DivSdfIconRenderer(),
        (DivSdfIconRenderer r, TagCompound t) =>
        {
          t["IconKind"] = (int)r.Kind;
          t["Morph"] = r.Morph;
        },
        (DivSdfIconRenderer r, TagCompound t) =>
        {
          r.Bind((SdfIconKind)t.GetInt("IconKind"));
          r.SetMorph(t.GetFloat("Morph"));
        });
    }
  }

  /// <summary>
  /// 控制器的文档序列化登记表; 行为性控制器 (事件挂接类) 不进文档, 只有参数性控制器可登记.
  /// </summary>
  public static class UIDocumentControllerParts
  {
    private static readonly Dictionary<string, UIDocumentPart<DivController>> _parts = new Dictionary<string, UIDocumentPart<DivController>>();
    private static readonly Dictionary<Type, UIDocumentPart<DivController>> _byType = new Dictionary<Type, UIDocumentPart<DivController>>();
    private static bool _defaultsEnsured;

    public static IReadOnlyDictionary<string, UIDocumentPart<DivController>> Parts => _parts;

    public static void Register<K>(string kind, Func<K> factory, Action<K, TagCompound> write, Action<K, TagCompound> read) where K : DivController
    {
      UIDocumentPart<DivController> part = new UIDocumentPart<DivController>(
        kind, typeof(K), () => (DivController)(object)factory(),
        (controller, tag) => write((K)(object)controller, tag),
        (controller, tag) => read((K)(object)controller, tag));
      _parts[kind] = part;
      _byType[typeof(K)] = part;
    }

    public static UIDocumentPart<DivController> Get(string kind)
    {
      EnsureDefaults();
      if (_parts.TryGetValue(kind, out UIDocumentPart<DivController> part))
        return part;
      Console.Log(ConsoleTextType.Error, "UIDocument", string.Concat("未注册的控制器类型: ", kind ?? "null"));
      return null;
    }

    public static UIDocumentPart<DivController> Find(DivController controller)
    {
      EnsureDefaults();
      return _byType.TryGetValue(controller.GetType(), out UIDocumentPart<DivController> part) ? part : null;
    }

    private static void EnsureDefaults()
    {
      if (_defaultsEnsured)
        return;
      _defaultsEnsured = true;
      Register("LinearMenu", () => new Controllers.LinearMenuController(),
        (Controllers.LinearMenuController c, TagCompound t) =>
        {
          t["Direction"] = (int)c.Direction;
          t["Alignment"] = (int)c.Alignment;
          t["Toward"] = (int)c.Toward;
          t["Interval"] = c.DivInterval;
          t["ScrollX"] = c.Scroll.X;
          t["ScrollY"] = c.Scroll.Y;
          t["AutoSetSize"] = c.AutoSetSize;
        },
        (Controllers.LinearMenuController c, TagCompound t) =>
        {
          c.Direction = (Direction)t.GetInt("Direction", (int)Direction.Vertical);
          c.Alignment = (Direction)t.GetInt("Alignment", (int)Direction.Center);
          c.Toward = (Direction)t.GetInt("Toward", (int)Direction.Down);
          c.DivInterval = t.GetInt("Interval");
          c.Scroll = new Vector2(t.GetFloat("ScrollX"), t.GetFloat("ScrollY"));
          c.AutoSetSize = t.GetBool("AutoSetSize");
        });
      Register("Gradient", () => new Controllers.DivGradientController(),
        (Controllers.DivGradientController c, TagCompound t) =>
        {
          t["OpenColor"] = c.OpenColor.Duration;
          t["CloseColor"] = c.CloseColor.Duration;
          t["OpenScale"] = c.OpenScale.Duration;
          t["CloseScale"] = c.CloseScale.Duration;
        },
        (Controllers.DivGradientController c, TagCompound t) =>
        {
          c.OpenColor.SetDuration(t.GetFloat("OpenColor", c.OpenColor.Duration));
          c.CloseColor.SetDuration(t.GetFloat("CloseColor", c.CloseColor.Duration));
          c.OpenScale.SetDuration(t.GetFloat("OpenScale", c.OpenScale.Duration));
          c.CloseScale.SetDuration(t.GetFloat("CloseScale", c.CloseScale.Duration));
        });
    }
  }
}
