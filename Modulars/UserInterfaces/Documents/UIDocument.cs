using Colin.Core.IO;

namespace Colin.Core.Modulars.UserInterfaces.Documents
{
  /// <summary>
  /// 界面文档: Div 子树与 NBT 的双向转换.
  /// <br>文档只存外观与结构 (名称/类型/布局/样式/交互/渲染器/参数性控制器/自定义节), 行为 (事件挂接、逻辑) 留给代码按名取节点挂接 —— HTML/JS 分工.</br>
  /// <br>手写的 Div 子类以注册表类型 (或无参构造自动登记) 的身份进文档, 即 WPF 式用户自定义预制件.</br>
  /// </summary>
  public static class UIDocument
  {
    public const int FormatVersion = 1;

    /// <summary>
    /// 把 Div 子树写成文档根.
    /// </summary>
    public static TagCompound Write(Div root)
    {
      TagCompound doc = new TagCompound();
      doc["Version"] = FormatVersion;
      doc["Root"] = WriteNode(root);
      return doc;
    }

    /// <summary>
    /// 从文档根还原 Div 子树; 元素未经 <see cref="Div.DoInitialize"/>, 由调用方注册进界面时以 doInit 触发.
    /// </summary>
    public static Div Read(TagCompound doc)
    {
      TagCompound root = doc?.GetCompound("Root");
      if (root is null)
      {
        Console.Log(ConsoleTextType.Error, "UIDocument", "文档缺少 Root 节");
        return null;
      }
      Div div = ReadNode(root);
      //锚点目标以元素名跨节点引用, 须待整树还原后统一解析
      ResolveAnchorTargets(div, div);
      return div;
    }

    /// <summary>
    /// 保存为 gzip 压缩的 .uibt 文件.
    /// </summary>
    public static void Save(Div root, string path)
      => TagIO.Write(path, Write(root), true);

    /// <summary>
    /// 从 .uibt 文件加载 Div 子树.
    /// </summary>
    public static Div Load(string path)
      => Read(TagIO.Read(path));

    internal static TagCompound WriteNode(Div div)
    {
      Type type = div.GetType();
      string key = UIDocumentRegistry.GetKey(type);
      if (key is null)
      {
        Console.Log(ConsoleTextType.Error, "UIDocument",
          string.Concat("类型 ", type.FullName, " 既未注册也没有无参构造, 已按基类 Div 降级保存; 请为它调 UIDocumentRegistry.Register"));
        key = "Div";
      }
      TagCompound tag = new TagCompound();
      tag["Type"] = key;
      tag["Name"] = div.Name;
      if (div.LayoutOwnership != DivLayoutOwnership.Auto)
        tag["Ownership"] = (int)div.LayoutOwnership;

      DivLayout layout = div.Layout;
      TagCompound layoutTag = new TagCompound();
      layoutTag["Left"] = layout.Left;
      layoutTag["Top"] = layout.Top;
      layoutTag["Width"] = layout.Width;
      layoutTag["Height"] = layout.Height;
      layoutTag["AnchorX"] = layout.AnchorX;
      layoutTag["AnchorY"] = layout.AnchorY;
      layoutTag["ScaleX"] = layout.ScaleX;
      layoutTag["ScaleY"] = layout.ScaleY;
      layoutTag["PadL"] = layout.PaddingLeft;
      layoutTag["PadT"] = layout.PaddingTop;
      layoutTag["PadR"] = layout.PaddingRight;
      layoutTag["PadB"] = layout.PaddingBottom;
      if (layout.Rotation != 0f)
        layoutTag["Rotation"] = layout.Rotation;
      if (layout.ScissorEnable)
      {
        layoutTag["ScissorEnable"] = true;
        layoutTag["ScissorL"] = layout.ScissorLeft;
        layoutTag["ScissorT"] = layout.ScissorTop;
        layoutTag["ScissorW"] = layout.ScissorWidth;
        layoutTag["ScissorH"] = layout.ScissorHeight;
      }
      tag["Layout"] = layoutTag;

      if (HasStyle(div.Style))
        tag["Style"] = WriteStyle(div.Style);

      InteractStyle interact = div.Interact;
      if (interact.IsInteractive is false || interact.IsSelectable || interact.IsDraggable || interact.DragLimit != Rectangle.Empty)
      {
        TagCompound interactTag = new TagCompound();
        interactTag["Interactive"] = interact.IsInteractive;
        interactTag["Selectable"] = interact.IsSelectable;
        interactTag["Draggable"] = interact.IsDraggable;
        interactTag["LimitX"] = interact.DragLimit.X;
        interactTag["LimitY"] = interact.DragLimit.Y;
        interactTag["LimitW"] = interact.DragLimit.Width;
        interactTag["LimitH"] = interact.DragLimit.Height;
        tag["Interact"] = interactTag;
      }

      if (div.Design.Color != Color.White)
        tag["Color"] = PackColor(div.Design.Color);

      if (div.Renderer is not null)
      {
        UIDocumentPart<DivRenderer> part = UIDocumentRendererParts.Find(div.Renderer);
        if (part is not null)
        {
          TagCompound rendererTag = new TagCompound();
          rendererTag["Kind"] = part.Kind;
          part.Write(div.Renderer, rendererTag);
          tag["Renderer"] = rendererTag;
        }
        else
          Console.Log(ConsoleTextType.Warning, "UIDocument",
            string.Concat("元素 ", div.Name, " 的渲染器 ", div.Renderer.GetType().Name, " 未注册文档序列化, 渲染器将丢失"));
      }

      if (div.Controller is not null)
      {
        UIDocumentPart<DivController> part = UIDocumentControllerParts.Find(div.Controller);
        if (part is not null)
        {
          TagCompound controllerTag = new TagCompound();
          controllerTag["Kind"] = part.Kind;
          part.Write(div.Controller, controllerTag);
          tag["Controller"] = controllerTag;
        }
      }

      if (div is IUIDocumentCustomizable customizable)
      {
        TagCompound custom = new TagCompound();
        customizable.WriteDocumentCustom(custom);
        if (custom.Count > 0)
          tag["Custom"] = custom;
      }

      if (div.Children.Count > 0)
      {
        TagSeq children = new TagSeq(TagType.Compound);
        List<Div> nodes = div.Children;
        for (int index = 0; index < nodes.Count; index++)
          children.Add(WriteNode(nodes[index]));
        tag["Children"] = children;
      }
      return tag;
    }

    internal static Div ReadNode(TagCompound tag)
    {
      Div div = UIDocumentRegistry.Create(tag.GetString("Type", "Div"));
      if (div is null)
        return null;
      div.Rename(tag.GetString("Name", div.Name));
      div.LayoutOwnership = (DivLayoutOwnership)Math.Clamp(tag.GetInt("Ownership"), 0, 2);

      ReadLayout(div, tag.GetCompound("Layout"));

      TagCompound styleTag = tag.GetCompound("Style");
      if (styleTag is not null)
        div.Style = ReadStyle(styleTag);

      TagCompound interactTag = tag.GetCompound("Interact");
      if (interactTag is not null)
      {
        InteractStyle interact = div.Interact;
        interact.IsInteractive = interactTag.GetBool("Interactive", true);
        interact.IsSelectable = interactTag.GetBool("Selectable");
        interact.IsDraggable = interactTag.GetBool("Draggable");
        interact.DragLimit = new Rectangle(
          interactTag.GetInt("LimitX"), interactTag.GetInt("LimitY"),
          interactTag.GetInt("LimitW"), interactTag.GetInt("LimitH"));
        div.Interact = interact;
      }

      if (tag.Has("Color"))
        div.Design.Color = UnpackColor(tag.GetInt("Color"));

      TagCompound rendererTag = tag.GetCompound("Renderer");
      if (rendererTag is not null)
      {
        UIDocumentPart<DivRenderer> part = UIDocumentRendererParts.Get(rendererTag.GetString("Kind"));
        if (part is not null)
        {
          DivRenderer renderer = part.Factory();
          if (renderer is not null)
          {
            div.BindRenderer(renderer);
            part.Read(renderer, rendererTag);
          }
        }
      }

      TagCompound controllerTag = tag.GetCompound("Controller");
      if (controllerTag is not null)
      {
        UIDocumentPart<DivController> part = UIDocumentControllerParts.Get(controllerTag.GetString("Kind"));
        if (part is not null)
        {
          DivController controller = part.Factory();
          if (controller is not null)
          {
            div.BindController(controller);
            part.Read(controller, controllerTag);
          }
        }
      }

      if (div is IUIDocumentCustomizable customizable && tag.GetCompound("Custom") is TagCompound custom)
        customizable.ReadDocumentCustom(custom);

      TagSeq children = tag.GetSeq("Children");
      if (children is not null)
      {
        for (int index = 0; index < children.Count; index++)
        {
          if (children.Items[index] is TagCompound childTag)
          {
            Div child = ReadNode(childTag);
            if (child is not null)
              div.Register(child);
          }
        }
      }
      return div;
    }

    private static void ReadLayout(Div div, TagCompound layout)
    {
      if (layout is null)
        return;
      div.Layout.SetLocation(layout.GetFloat("Left"), layout.GetFloat("Top"));
      div.Layout.SetSize(layout.GetFloat("Width"), layout.GetFloat("Height"));
      div.Layout.Anchor = new Vector2(layout.GetFloat("AnchorX"), layout.GetFloat("AnchorY"));
      div.Layout.Scale = new Vector2(layout.GetFloat("ScaleX", 1f), layout.GetFloat("ScaleY", 1f));
      div.Layout.PaddingLeft = layout.GetFloat("PadL");
      div.Layout.PaddingTop = layout.GetFloat("PadT");
      div.Layout.PaddingRight = layout.GetFloat("PadR");
      div.Layout.PaddingBottom = layout.GetFloat("PadB");
      if (layout.Has("Rotation"))
        div.Layout.Rotation = layout.GetFloat("Rotation");
      if (layout.GetBool("ScissorEnable"))
      {
        div.Layout.ScissorEnable = true;
        div.Layout.ScissorLeft = layout.GetInt("ScissorL");
        div.Layout.ScissorTop = layout.GetInt("ScissorT");
        div.Layout.ScissorWidth = layout.GetInt("ScissorW");
        div.Layout.ScissorHeight = layout.GetInt("ScissorH");
      }
    }

    internal static bool HasStyle(DivLayoutStyle style)
      => style.Horizontal != DivAxisAnchor.None
        || style.Vertical != DivAxisAnchor.None
        || style.WidthMode != DivSizeMode.Fixed
        || style.HeightMode != DivSizeMode.Fixed;

    internal static TagCompound WriteStyle(DivLayoutStyle style)
    {
      TagCompound tag = new TagCompound();
      tag["H"] = (int)style.Horizontal;
      tag["V"] = (int)style.Vertical;
      tag["BeginX"] = style.OffsetBegin.X;
      tag["BeginY"] = style.OffsetBegin.Y;
      tag["EndX"] = style.OffsetEnd.X;
      tag["EndY"] = style.OffsetEnd.Y;
      tag["WMode"] = (int)style.WidthMode;
      tag["HMode"] = (int)style.HeightMode;
      tag["PW"] = style.PercentWidth;
      tag["PH"] = style.PercentHeight;
      tag["MatchX"] = style.MatchOffset.X;
      tag["MatchY"] = style.MatchOffset.Y;
      tag["ExtraX"] = style.AutoExtra.X;
      tag["ExtraY"] = style.AutoExtra.Y;
      tag["PercentX"] = style.PercentPoint.X;
      tag["PercentY"] = style.PercentPoint.Y;
      if (style.AnchorTarget is not null)
        tag["Target"] = style.AnchorTarget.Name;
      return tag;
    }

    internal static DivLayoutStyle ReadStyle(TagCompound tag)
      => new DivLayoutStyle
      {
        Horizontal = (DivAxisAnchor)tag.GetInt("H"),
        Vertical = (DivAxisAnchor)tag.GetInt("V"),
        OffsetBegin = new Vector2(tag.GetFloat("BeginX"), tag.GetFloat("BeginY")),
        OffsetEnd = new Vector2(tag.GetFloat("EndX"), tag.GetFloat("EndY")),
        WidthMode = (DivSizeMode)tag.GetInt("WMode"),
        HeightMode = (DivSizeMode)tag.GetInt("HMode"),
        PercentWidth = tag.GetFloat("PW"),
        PercentHeight = tag.GetFloat("PH"),
        MatchOffset = new Vector2(tag.GetFloat("MatchX"), tag.GetFloat("MatchY")),
        AutoExtra = new Vector2(tag.GetFloat("ExtraX"), tag.GetFloat("ExtraY")),
        PercentPoint = new Vector2(tag.GetFloat("PercentX"), tag.GetFloat("PercentY")),
        AnchorTargetName = tag.GetString("Target"),
      };

    private static void ResolveAnchorTargets(Div root, Div div)
    {
      if (div is null)
        return;
      string name = div.Style.AnchorTargetName;
      if (string.IsNullOrEmpty(name) is false && div.Style.AnchorTarget is null)
      {
        Div target = FindByName(root, name);
        if (target is not null)
          div.Style.AnchorTarget = target;
        else
          Console.Log(ConsoleTextType.Warning, "UIDocument",
            string.Concat("元素 ", div.Name, " 的锚点目标 ", name, " 不存在于文档中, 匹配尺寸将保持原值"));
      }
      List<Div> children = div.Children;
      for (int count = 0; count < children.Count; count++)
        ResolveAnchorTargets(root, children[count]);
    }

    private static Div FindByName(Div div, string name)
    {
      if (div.Name == name)
        return div;
      List<Div> children = div.Children;
      for (int count = 0; count < children.Count; count++)
      {
        Div found = FindByName(children[count], name);
        if (found is not null)
          return found;
      }
      return null;
    }

    internal static int PackColor(Color color)
      => color.R | (color.G << 8) | (color.B << 16) | (color.A << 24);

    internal static Color UnpackColor(int packed)
      => new Color(packed & 0xFF, (packed >> 8) & 0xFF, (packed >> 16) & 0xFF, (packed >> 24) & 0xFF);
  }
}
