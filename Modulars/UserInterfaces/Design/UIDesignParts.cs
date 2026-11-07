using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Design
{
  /// <summary>一类可替换的部件 (渲染器 / 控制器): 显示名 + 类型 + 工厂.</summary>
  public sealed class UIDesignPart<T> where T : class
  {
    public string Kind;
    public Type PartType;
    public Func<T> Factory;
  }

  /// <summary>
  /// 编辑器"换渲染器 / 换控制器"用的部件目录.
  /// <br>这里是<b>手写清单</b>: 生成器认识哪些类型, 编辑器就能选哪些 —— 两边必须一一对应</br>
  /// <br>(生成器的发射器在 <see cref="DivCodeWriter"/>, 新加一种就要在两边各补一条).</br>
  /// </summary>
  public static class UIDesignParts
  {
    private static List<UIDesignPart<DivRenderer>> _renderers;
    private static List<UIDesignPart<DivController>> _controllers;

    /// <summary>全部可替换渲染器.</summary>
    public static IReadOnlyList<UIDesignPart<DivRenderer>> Renderers => _renderers ??= new List<UIDesignPart<DivRenderer>>
    {
      Part<DivRenderer>("Pixel", typeof(DivPixelRenderer), () => new DivPixelRenderer()),
      Part<DivRenderer>("Texture", typeof(DivTextureRenderer), () => new DivTextureRenderer()),
      Part<DivRenderer>("Ninecut", typeof(DivNinecutRenderer), () => new DivNinecutRenderer()),
      Part<DivRenderer>("Font", typeof(DivFontRenderer), () => new DivFontRenderer()),
      Part<DivRenderer>("Rounded", typeof(DivRoundedRenderer), () => new DivRoundedRenderer()),
      Part<DivRenderer>("SdfIcon", typeof(DivSdfIconRenderer), () => new DivSdfIconRenderer()),
    };

    /// <summary>全部可替换控制器 (行为性控制器不在此列).</summary>
    public static IReadOnlyList<UIDesignPart<DivController>> Controllers => _controllers ??= new List<UIDesignPart<DivController>>
    {
      Part<DivController>("LinearMenu", typeof(LinearMenuController), () => new LinearMenuController()),
      Part<DivController>("Gradient", typeof(DivGradientController), () => new DivGradientController()),
    };

    /// <summary>按实例找登记项; 未登记返回 null.</summary>
    public static UIDesignPart<DivRenderer> FindRenderer(DivRenderer renderer)
    {
      if (renderer is null)
        return null;
      IReadOnlyList<UIDesignPart<DivRenderer>> parts = Renderers;
      for (int index = 0; index < parts.Count; index++)
      {
        if (parts[index].PartType == renderer.GetType())
          return parts[index];
      }
      return null;
    }

    /// <summary>按显示名找渲染器登记项; 未知返回 null.</summary>
    public static UIDesignPart<DivRenderer> GetRenderer(string kind)
    {
      IReadOnlyList<UIDesignPart<DivRenderer>> parts = Renderers;
      for (int index = 0; index < parts.Count; index++)
      {
        if (parts[index].Kind == kind)
          return parts[index];
      }
      return null;
    }

    /// <summary>按实例找控制器登记项; 未登记返回 null.</summary>
    public static UIDesignPart<DivController> FindController(DivController controller)
    {
      if (controller is null)
        return null;
      IReadOnlyList<UIDesignPart<DivController>> parts = Controllers;
      for (int index = 0; index < parts.Count; index++)
      {
        if (parts[index].PartType == controller.GetType())
          return parts[index];
      }
      return null;
    }

    /// <summary>按显示名找控制器登记项; 未知返回 null.</summary>
    public static UIDesignPart<DivController> GetController(string kind)
    {
      IReadOnlyList<UIDesignPart<DivController>> parts = Controllers;
      for (int index = 0; index < parts.Count; index++)
      {
        if (parts[index].Kind == kind)
          return parts[index];
      }
      return null;
    }

    private static UIDesignPart<T> Part<T>(string kind, Type partType, Func<T> factory) where T : class
      => new UIDesignPart<T> { Kind = kind, PartType = partType, Factory = factory };
  }
}
