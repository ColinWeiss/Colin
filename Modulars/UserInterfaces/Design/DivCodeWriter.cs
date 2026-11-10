using System.Globalization;
using System.Text;
using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Renderers;
using Microsoft.Xna.Framework;

namespace Colin.Core.Modulars.UserInterfaces.Design
{
  /// <summary>
  /// 导出代码时收集到的问题与提示.
  /// </summary>
  public sealed class DivCodeWriteReport
  {
    /// <summary>会让生成结果与现场不一致的问题 (未知渲染器、未知脚本等).</summary>
    public readonly List<string> Issues = new List<string>();

    /// <summary>不影响生成的提醒.</summary>
    public readonly List<string> Notes = new List<string>();

    /// <summary>本次生成的节点数.</summary>
    public int Nodes;

    /// <summary>本次生成里绑定的脚本数.</summary>
    public int Scripts;

    /// <summary>是否没有阻断性问题.</summary>
    public bool Ok => Issues.Count == 0;

    internal void Issue(string text)
    {
      if (Issues.Count < 128)
        Issues.Add(text);
    }

    internal void Note(string text)
    {
      if (Notes.Count < 128)
        Notes.Add(text);
    }
  }

  /// <summary>
  /// 代码导出参数.
  /// </summary>
  public sealed class DivCodeWriterOptions
  {
    /// <summary>目标静态类所在命名空间.</summary>
    public string Namespace;

    /// <summary>目标静态类名 (不含命名空间); 为空则取根节点名 + "UI".</summary>
    public string ClassName;

    /// <summary>建结构的方法名.</summary>
    public string BuildMethodName = "Build";

    /// <summary>建实例的工厂方法名.</summary>
    public string CreateMethodName = "Create";

    /// <summary>句柄表类名 (null/空 = 不生成句柄表).</summary>
    public string HandlesClassName = "Handles";

    /// <summary>写进文件头的来源说明.</summary>
    public string SourceNote;
  }

  /// <summary>
  /// 把一棵活在内存里的子树写成 C# <b>builder</b>:
  /// <code>
  /// public static class XxxUI
  /// {
  ///   public sealed class Handles { public Div Panel; ... }
  ///   public static Div Create(Handles handles, string name = "Xxx");
  ///   public static void Build(Div parent, Handles handles);
  /// }
  /// </code>
  /// <br>[!] 节点一律是 <see cref="Div"/> (不再按类区分): builder 只负责"建节点 + 摆布局 + 装组件 + 绑脚本".</br>
  /// <br>[!] 只发射作者态: 状态机脚本的现场值走 <see cref="DivScript.AuthorColor"/> / <see cref="DivScript.ExportValues"/>.</br>
  /// </summary>
  public static class DivCodeWriter
  {
    private const int IndentUnit = 2;

    /// <summary>生成一个 subtree builder 的完整 C# 文件内容.</summary>
    public static string Emit(Div root, DivCodeWriterOptions options, DivCodeWriteReport report)
    {
      if (root is null)
        throw new ArgumentNullException(nameof(root));
      options ??= new DivCodeWriterOptions();
      report ??= new DivCodeWriteReport();
      string @namespace = options.Namespace ?? root.GetType().Namespace ?? string.Empty;
      string className = options.ClassName;
      if (string.IsNullOrEmpty(className))
        className = string.Concat(ToIdentifier(root.Name), "UI");
      string buildName = string.IsNullOrEmpty(options.BuildMethodName) ? "Build" : options.BuildMethodName;
      string createName = string.IsNullOrEmpty(options.CreateMethodName) ? "Create" : options.CreateMethodName;
      string handlesName = options.HandlesClassName;
      if (string.IsNullOrEmpty(@namespace))
      {
        @namespace = "Global";
        report.Issue("根节点没有命名空间; 生成代码落到 namespace Global, 请手工调整");
      }

      List<Node> nodes = new List<Node>();
      Dictionary<Div, string> localOf = new Dictionary<Div, string>();
      HashSet<string> usedLocals = new HashSet<string>(StringComparer.Ordinal);
      HashSet<string> usedHandles = new HashSet<string>(StringComparer.Ordinal);
      Collect(root, null, nodes, localOf, usedLocals, usedHandles, report.Notes);
      report.Nodes = nodes.Count;
      //根节点不新建: 它就是 Build(parent) 要填内容的那个节点 —— 否则 Create 里会多套一层同名节点.
      if (nodes.Count > 0)
        nodes[0].Local = "parent";

      StringBuilder sb = new StringBuilder(8192);
      EmitHeader(sb, root, className, options, report);
      sb.Append('\n');
      sb.Append("namespace ").Append(@namespace).Append('\n');
      sb.Append("{\n");
      Line(sb, 1, "/// <summary>结构 + 组件 + 脚本; 由 UI 编辑器导出, 请勿手改 (再次导出会整个覆盖).</summary>");
      Line(sb, 1, string.Concat("public static class ", className));
      sb.Append(Indent(1)).Append("{\n");

      if (string.IsNullOrEmpty(handlesName) is false)
      {
        Line(sb, 2, "/// <summary>节点句柄表: 名字即字段名, 改名字/删节点都会在编译期报错.</summary>");
        Line(sb, 2, string.Concat("public sealed class ", handlesName));
        sb.Append(Indent(2)).Append("{\n");
        for (int index = 0; index < nodes.Count; index++)
          Line(sb, 3, string.Concat("public ", TypeName(typeof(Div)), " ", nodes[index].Handle, ";"));
        sb.Append(Indent(2)).Append("}\n");
        sb.Append('\n');
      }

      string handlesParam = string.IsNullOrEmpty(handlesName) ? string.Empty : string.Concat(handlesName, " handles");
      string handlesArg = string.IsNullOrEmpty(handlesName) ? string.Empty : ", handles";

      // —— Create ——
      Line(sb, 2, "/// <summary>建一个实例并填好内容.</summary>");
      Line(sb, 2, string.Concat("public static ", TypeName(typeof(Div)), " ", createName, "(",
        string.IsNullOrEmpty(handlesParam) ? string.Empty : string.Concat(handlesParam, ", "),
        "string name = ", Literal(root.Name), ")"));
      sb.Append(Indent(2)).Append("{\n");
      Line(sb, 3, string.Concat(TypeName(typeof(Div)), " root = new ", TypeName(typeof(Div)), "(name);"));
      if (root.IsRoot)
        Line(sb, 3, "root.MarkAsRoot();");
      if (root.IsCanvas)
        Line(sb, 3, "root.MarkAsCanvas();");
      Line(sb, 3, string.Concat(buildName, "(root", handlesArg, ");"));
      Line(sb, 3, "return root;");
      sb.Append(Indent(2)).Append("}\n");
      sb.Append('\n');

      // —— Build ——
      Line(sb, 2, "/// <summary>把内容建到 <paramref name=\"parent\"/> 下 (供父级 builder 组合调用).</summary>");
      Line(sb, 2, string.Concat("public static void ", buildName, "(", TypeName(typeof(Div)), " parent",
        string.IsNullOrEmpty(handlesName) ? string.Empty : string.Concat(", ", handlesName, " handles"), ")"));
      sb.Append(Indent(2)).Append("{\n");

      Line(sb, 3, "// —— 建节点 —— //");
      for (int index = 1; index < nodes.Count; index++)
      {
        Node node = nodes[index];
        string owner = node.Parent is null ? "parent" : node.Parent.Local;
        Line(sb, 3, string.Concat(TypeName(typeof(Div)), " ", node.Local, " = new ", TypeName(typeof(Div)), "(", Literal(node.Div.Name), ");"));
        if (node.Div.IsRoot)
          Line(sb, 3, string.Concat(node.Local, ".MarkAsRoot();"));
        if (node.Div.IsCanvas)
          Line(sb, 3, string.Concat(node.Local, ".MarkAsCanvas();"));
        Line(sb, 3, string.Concat(owner, ".Attach(", node.Local, ");"));
      }

      sb.Append('\n');
      Line(sb, 3, "// —— 摆放 / 样式 / 外观 / 组件 / 脚本 —— //");
      int rendererSerial = 0;
      int controllerSerial = 0;
      int scriptSerial = 0;
      for (int index = 0; index < nodes.Count; index++)
      {
        Node node = nodes[index];
        EmitNodeValues(sb, node.Div, node.Local, report, ref rendererSerial, ref controllerSerial, ref scriptSerial);
      }

      sb.Append('\n');
      Line(sb, 3, "// —— 跨节点引用 —— //");
      for (int index = 0; index < nodes.Count; index++)
      {
        Node node = nodes[index];
        if (node.Div.Style.AnchorTarget is Div target && localOf.TryGetValue(target, out string targetLocal))
          Line(sb, 3, string.Concat(node.Local, ".Style.AnchorTarget = ", targetLocal, ";"));
      }

      if (string.IsNullOrEmpty(handlesName) is false)
      {
        sb.Append('\n');
        Line(sb, 3, "// —— 句柄 —— //");
        Line(sb, 3, "if (handles is not null)");
        sb.Append(Indent(3)).Append("{\n");
        for (int index = 0; index < nodes.Count; index++)
          Line(sb, 4, string.Concat("handles.", nodes[index].Handle, " = ", nodes[index].Local, ";"));
        sb.Append(Indent(3)).Append("}\n");
      }

      sb.Append(Indent(2)).Append("}\n");
      sb.Append(Indent(1)).Append("}\n");
      sb.Append("}\n");
      string code = sb.ToString();
      VerifyGenerated(code, report);
      return code;
    }

    private sealed class Node
    {
      public Div Div;
      public Node Parent;
      public string Local;
      public string Handle;
    }

    private static void Collect(Div div, Node parent, List<Node> nodes, Dictionary<Div, string> localOf,
      HashSet<string> usedLocals, HashSet<string> usedHandles, List<string> notes)
    {
      string local = UniqueName("_ui", div.Name, usedLocals);
      string handle = UniqueName(string.Empty, div.Name, usedHandles);
      Node node = new Node { Div = div, Parent = parent, Local = local, Handle = handle };
      nodes.Add(node);
      localOf[div] = local;
      //运行态生成的节点 (槽位/图标框…): 既不导出它, 也不导出它的子树.
      if (div.ExcludeFromExport)
      {
        notes?.Add(string.Concat("节点 ", div.Name, " 是运行态生成的: 导出跳过 (由脚本在绑定期重建)"));
        return;
      }
      //脚本自持结构: 它的子级由 OnBind 现建, 生成代码只写 BindScript —— 否则会建出两套.
      if (div.Script is { OwnsStructure: true })
      {
        notes?.Add(string.Concat("节点 ", div.Name, " 的脚本 ", div.Script.GetType().Name,
          " 自持结构: 其子级不出现在生成代码里, 由脚本在绑定期重建"));
        return;
      }
      List<Div> children = div.Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div child = children[index];
        if (child is null)
          continue;
        Collect(child, node, nodes, localOf, usedLocals, usedHandles, notes);
      }
    }

    private static string UniqueName(string prefix, string name, HashSet<string> used)
    {
      StringBuilder sb = new StringBuilder(prefix);
      bool upper = true;
      for (int index = 0; index < (name?.Length ?? 0); index++)
      {
        char c = name[index];
        if (char.IsLetterOrDigit(c))
        {
          sb.Append(upper ? char.ToUpperInvariant(c) : c);
          upper = false;
        }
        else
          upper = true;
      }
      if (sb.Length <= prefix.Length)
        sb.Append("Node");
      if (char.IsDigit(sb[prefix.Length]))
        sb.Append('_');
      string root = sb.ToString();
      string candidate = root;
      int serial = 2;
      while (used.Contains(candidate))
        candidate = string.Concat(root, serial++);
      used.Add(candidate);
      return candidate;
    }

    private static string ToIdentifier(string name)
    {
      StringBuilder sb = new StringBuilder(name?.Length ?? 0);
      bool upper = true;
      for (int index = 0; index < (name?.Length ?? 0); index++)
      {
        char c = name[index];
        if (char.IsLetterOrDigit(c))
        {
          sb.Append(upper ? char.ToUpperInvariant(c) : c);
          upper = false;
        }
        else
          upper = true;
      }
      return sb.Length == 0 ? "Interface" : sb.ToString();
    }

    /// <summary>出厂自检: 同一个临时变量名只能声明一次.</summary>
    private static void VerifyGenerated(string code, DivCodeWriteReport report)
    {
      string[] prefixes = { "__r", "__c", "__s" };
      for (int prefixIndex = 0; prefixIndex < prefixes.Length; prefixIndex++)
      {
        string prefix = prefixes[prefixIndex];
        for (int serial = 0; serial < 512; serial++)
        {
          string declaration = string.Concat(" ", prefix, serial.ToString(CultureInfo.InvariantCulture), " = new ");
          int count = 0;
          int index = 0;
          while ((index = code.IndexOf(declaration, index, StringComparison.Ordinal)) >= 0)
          {
            count++;
            index += declaration.Length;
          }
          if (count > 1)
            report.Issue(string.Concat("生成器内部错误: 临时变量 ", prefix, serial.ToString(CultureInfo.InvariantCulture),
              " 声明了 ", count.ToString(CultureInfo.InvariantCulture), " 次 (编译会报 CS0128)"));
        }
      }
    }

    private static void EmitHeader(StringBuilder sb, Div root, string className, DivCodeWriterOptions options, DivCodeWriteReport report)
    {
      sb.Append("// <auto-generated>\n");
      sb.Append("//     此文件由 UI 编辑器导出, 请勿手改 —— 再次导出会整个覆盖.\n");
      sb.Append("//\n");
      sb.Append("//     这里只有: 结构 / 布局 / 样式 / 渲染器 / 控制器 / 脚本绑定与脚本参数.\n");
      sb.Append("//     行为写在脚本类里 (DivScript 子类), 本文件只负责把它挂上去.\n");
      sb.Append("//\n");
      sb.Append("//     单元: ").Append(className).Append("  ←  ").Append(root.Name).Append('\n');
      sb.Append("//     节点数: ").Append(report.Nodes.ToString(CultureInfo.InvariantCulture));
      if (report.Scripts > 0)
        sb.Append(", 脚本 ").Append(report.Scripts.ToString(CultureInfo.InvariantCulture));
      sb.Append('\n');
      if (string.IsNullOrEmpty(options.SourceNote) is false)
        sb.Append("//     ").Append(options.SourceNote.Replace("\r", " ").Replace("\n", " ")).Append('\n');
      sb.Append("// </auto-generated>\n");
    }

    private static void EmitNodeValues(StringBuilder sb, Div div, string p, DivCodeWriteReport report,
      ref int rendererSerial, ref int controllerSerial, ref int scriptSerial)
    {
      Line(sb, 3, string.Concat("// ", div.Name));
      if (div.IsVisible is false)
        Line(sb, 3, string.Concat(p, ".IsVisible = false;"));
      if (div.LayoutOwnership != DivLayoutOwnership.Auto)
        Line(sb, 3, string.Concat(p, ".LayoutOwnership = ", EnumLiteral(typeof(DivLayoutOwnership), div.LayoutOwnership), ";"));

      //尺寸自持的脚本 (FormScript 之类) 由脚本在绑定期按自身规格现算, 生成代码不写宽高.
      bool ownsSize = div.Script is { OwnsSize: true };
      if (ownsSize)
        report.Note(string.Concat("节点 ", div.Name, " 的脚本 ", div.Script.GetType().Name, " 自持尺寸: 不导出宽高"));
      EmitLayout(sb, div.Layout, div.Style, p, ownsSize);
      EmitStyle(sb, div.Style, p, div, report);
      EmitInteract(sb, div.Interact, p);
      Color authorColor = div.AuthorColor;
      if (authorColor != Color.White)
        Line(sb, 3, string.Concat(p, ".Design.Color = ", ColorLiteral(authorColor), ";"));
      if (div.Renderer is not null)
        EmitRenderer(sb, div, p, report, ref rendererSerial);
      if (div.Controller is not null)
        EmitController(sb, div, p, report, ref controllerSerial);
      if (div.Script is not null)
        EmitScript(sb, div, p, report, ref scriptSerial);
      else if (string.IsNullOrWhiteSpace(div.PendingScriptType) is false)
        EmitPendingScript(sb, div, p, report);
    }

    private static void EmitLayout(StringBuilder sb, DivLayout layout, DivLayoutStyle style, string p, bool ownsSize)
    {
      bool derivedLeft = style.Horizontal != DivAxisAnchor.None;
      bool derivedTop = style.Vertical != DivAxisAnchor.None;
      bool derivedWidth = style.WidthMode != DivSizeMode.Fixed;
      bool derivedHeight = style.HeightMode != DivSizeMode.Fixed;

      if (derivedLeft && derivedTop is false)
        Line(sb, 3, string.Concat(p, ".Layout.Top = ", F(layout.Top), ";"));
      else if (derivedTop && derivedLeft is false)
        Line(sb, 3, string.Concat(p, ".Layout.Left = ", F(layout.Left), ";"));
      else if (derivedLeft is false && derivedTop is false && (layout.Left != 0f || layout.Top != 0f))
        Line(sb, 3, string.Concat(p, ".Layout.SetLocation(", F(layout.Left), ", ", F(layout.Top), ");"));

      if (ownsSize)
      {
        //尺寸归脚本: 不写宽高 (写了也会被 OnBind 里的现算覆盖, 只会让生成文件看起来和运行期不一致).
      }
      else if (derivedWidth && derivedHeight is false)
        Line(sb, 3, string.Concat(p, ".Layout.Height = ", F(layout.Height), ";"));
      else if (derivedHeight && derivedWidth is false)
        Line(sb, 3, string.Concat(p, ".Layout.Width = ", F(layout.Width), ";"));
      else if (derivedWidth is false && derivedHeight is false && (layout.Width != 0f || layout.Height != 0f))
        Line(sb, 3, string.Concat(p, ".Layout.SetSize(", F(layout.Width), ", ", F(layout.Height), ");"));
      if (layout.AnchorX != 0f)
        Line(sb, 3, string.Concat(p, ".Layout.AnchorX = ", F(layout.AnchorX), ";"));
      if (layout.AnchorY != 0f)
        Line(sb, 3, string.Concat(p, ".Layout.AnchorY = ", F(layout.AnchorY), ";"));
      if (layout.Scale != Vector2.One)
        Line(sb, 3, string.Concat(p, ".Layout.Scale = ", VectorLiteral(layout.Scale), ";"));
      if (layout.PaddingLeft != 0f || layout.PaddingTop != 0f || layout.PaddingRight != 0f || layout.PaddingBottom != 0f)
      {
        Line(sb, 3, string.Concat(p, ".Layout.PaddingLeft = ", F(layout.PaddingLeft), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.PaddingTop = ", F(layout.PaddingTop), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.PaddingRight = ", F(layout.PaddingRight), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.PaddingBottom = ", F(layout.PaddingBottom), ";"));
      }
      if (layout.Rotation != 0f)
        Line(sb, 3, string.Concat(p, ".Layout.Rotation = ", F(layout.Rotation), ";"));
      if (layout.ScissorEnable)
      {
        Line(sb, 3, string.Concat(p, ".Layout.ScissorEnable = true;"));
        Line(sb, 3, string.Concat(p, ".Layout.ScissorLeft = ", layout.ScissorLeft.ToString(CultureInfo.InvariantCulture), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.ScissorTop = ", layout.ScissorTop.ToString(CultureInfo.InvariantCulture), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.ScissorWidth = ", layout.ScissorWidth.ToString(CultureInfo.InvariantCulture), ";"));
        Line(sb, 3, string.Concat(p, ".Layout.ScissorHeight = ", layout.ScissorHeight.ToString(CultureInfo.InvariantCulture), ";"));
      }
    }

    private static void EmitStyle(StringBuilder sb, DivLayoutStyle style, string p, Div div, DivCodeWriteReport report)
    {
      if (style.IsAuthored is false)
        return;
      //Before/After 的语义是"贴着锚点目标": 没有目标时该轴根本不会定位, 生成出来就是个僵尸样式.
      if (style.AnchorTarget is Div anchorTarget
          && DivAnchorRules.CanAnchor(div, anchorTarget, out string anchorReason) is false)
        report.Issue(string.Concat("节点 ", div.Name, " 的锚定目标不合法: ", anchorReason));

      if (style.AnchorTarget is null)
      {
        if (style.Horizontal is DivAxisAnchor.Before or DivAxisAnchor.After)
          report.Issue(string.Concat("节点 ", div.Name, " 的横向锚定是 ", style.Horizontal.ToString(),
            ", 但 Style.AnchorTarget 为空: 该轴永远不会被定位"));
        if (style.Vertical is DivAxisAnchor.Before or DivAxisAnchor.After)
          report.Issue(string.Concat("节点 ", div.Name, " 的纵向锚定是 ", style.Vertical.ToString(),
            ", 但 Style.AnchorTarget 为空: 该轴永远不会被定位"));
      }
      if (style.Horizontal != DivAxisAnchor.None)
        Line(sb, 3, string.Concat(p, ".Style.Horizontal = ", EnumLiteral(typeof(DivAxisAnchor), style.Horizontal), ";"));
      if (style.Vertical != DivAxisAnchor.None)
        Line(sb, 3, string.Concat(p, ".Style.Vertical = ", EnumLiteral(typeof(DivAxisAnchor), style.Vertical), ";"));
      if (style.WidthMode != DivSizeMode.Fixed)
        Line(sb, 3, string.Concat(p, ".Style.WidthMode = ", EnumLiteral(typeof(DivSizeMode), style.WidthMode), ";"));
      if (style.HeightMode != DivSizeMode.Fixed)
        Line(sb, 3, string.Concat(p, ".Style.HeightMode = ", EnumLiteral(typeof(DivSizeMode), style.HeightMode), ";"));
      if (style.OffsetBegin != Vector2.Zero)
        Line(sb, 3, string.Concat(p, ".Style.OffsetBegin = ", VectorLiteral(style.OffsetBegin), ";"));
      if (style.OffsetEnd != Vector2.Zero)
        Line(sb, 3, string.Concat(p, ".Style.OffsetEnd = ", VectorLiteral(style.OffsetEnd), ";"));
      if (style.PercentWidth != 0f)
        Line(sb, 3, string.Concat(p, ".Style.PercentWidth = ", F(style.PercentWidth), ";"));
      if (style.PercentHeight != 0f)
        Line(sb, 3, string.Concat(p, ".Style.PercentHeight = ", F(style.PercentHeight), ";"));
      if (style.PercentPoint != Vector2.Zero)
        Line(sb, 3, string.Concat(p, ".Style.PercentPoint = ", VectorLiteral(style.PercentPoint), ";"));
      if (style.MatchOffset != Vector2.Zero)
        Line(sb, 3, string.Concat(p, ".Style.MatchOffset = ", VectorLiteral(style.MatchOffset), ";"));
      if (style.AutoExtra != Vector2.Zero)
        Line(sb, 3, string.Concat(p, ".Style.AutoExtra = ", VectorLiteral(style.AutoExtra), ";"));
    }

    private static void EmitInteract(StringBuilder sb, InteractStyle interact, string p)
    {
      bool authored = interact.IsInteractive is false || interact.IsSelectable || interact.IsDraggable || interact.DragLimit != Rectangle.Empty;
      if (authored is false)
        return;
      Line(sb, 3, string.Concat(p, ".Interact.IsInteractive = ", Bool(interact.IsInteractive), ";"));
      Line(sb, 3, string.Concat(p, ".Interact.IsSelectable = ", Bool(interact.IsSelectable), ";"));
      Line(sb, 3, string.Concat(p, ".Interact.IsDraggable = ", Bool(interact.IsDraggable), ";"));
      if (interact.DragLimit != Rectangle.Empty)
        Line(sb, 3, string.Concat(p, ".Interact.DragLimit = ", RectangleLiteral(interact.DragLimit), ";"));
    }

    private static void EmitRenderer(StringBuilder sb, Div div, string p, DivCodeWriteReport report, ref int serial)
    {
      DivRenderer renderer = div.Renderer;
      string v = string.Concat("__r", serial++.ToString(CultureInfo.InvariantCulture));
      string typeName = TypeName(renderer.GetType());
      List<string> statements = new List<string>();
      switch (renderer)
      {
        case DivPixelRenderer:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          break;
        case DivNinecutRenderer ninecut:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          AddBind(statements, v, ninecut.Sprite?.RawPath);
          if (ninecut.Cut.X != 0 || ninecut.Cut.Y != 0)
            statements.Add(string.Concat(v, ".SetCut(", ninecut.Cut.X.ToString(CultureInfo.InvariantCulture), ", ", ninecut.Cut.Y.ToString(CultureInfo.InvariantCulture), ");"));
          break;
        case DivTextureRenderer texture:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          AddBind(statements, v, texture.Sprite?.RawPath);
          break;
        case DivFontRenderer font:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          if (string.IsNullOrEmpty(font.FontPath) is false)
            statements.Add(string.Concat(v, ".BindFont(", Literal(NormalizeAssetPath(font.FontPath)), ", ", font.FontSize.ToString(CultureInfo.InvariantCulture), ");"));
          if (string.IsNullOrEmpty(font.Text) is false)
            statements.Add(string.Concat(v, ".Text = ", Literal(font.Text), ";"));
          if (font.TextShadow is false)
            statements.Add(string.Concat(v, ".TextShadow = false;"));
          if (font.FontSystemEffect != FontStashSharp.FontSystemEffect.None)
            statements.Add(string.Concat(v, ".FontSystemEffect = ", EnumLiteral(font.FontSystemEffect.GetType(), font.FontSystemEffect), ";"));
          break;
        case DivRoundedRenderer rounded:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          if (rounded.Radius != 0f)
            statements.Add(string.Concat(v, ".Radius = ", F(rounded.Radius), ";"));
          if (rounded.BorderWidth != 0f)
            statements.Add(string.Concat(v, ".BorderWidth = ", F(rounded.BorderWidth), ";"));
          if (rounded.BorderColor != Color.White)
            statements.Add(string.Concat(v, ".BorderColor = ", ColorLiteral(rounded.BorderColor), ";"));
          break;
        case DivSdfIconRenderer icon:
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          statements.Add(string.Concat(v, ".Bind(", EnumLiteral(icon.Kind.GetType(), icon.Kind), ");"));
          if (icon.Morph != 0f)
            statements.Add(string.Concat(v, ".SetMorph(", F(icon.Morph), ");"));
          break;
        default:
          if (renderer.GetType().GetConstructor(Type.EmptyTypes) is null)
          {
            report.Issue(string.Concat("渲染器 ", renderer.GetType().Name, " 需要构造参数, 生成代码写不出来 (节点 ", div.Name, ")"));
            Line(sb, 3, string.Concat("// 渲染器 ", renderer.GetType().Name, " 需要构造参数 —— 请在脚本里手动绑定"));
            return;
          }
          statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
          if (UIDesignParts.FindRenderer(renderer) is not null)
            report.Issue(string.Concat("渲染器 ", renderer.GetType().Name, " 已登记在 UIDesignParts, 但生成器没有对应发射器 —— 参数会丢"));
          else
            report.Note(string.Concat("渲染器 ", renderer.GetType().Name, " 生成器只写了类型 (参数请在脚本里补)"));
          break;
      }
      if (renderer.Adaptive)
        statements.Add(string.Concat(v, ".Adaptive = true;"));
      if (renderer.UseTranslucent is false)
        statements.Add(string.Concat(v, ".UseTranslucent = false;"));
      statements.Add(string.Concat(p, ".BindRenderer(", v, ");"));
      for (int index = 0; index < statements.Count; index++)
        Line(sb, 3, statements[index]);
    }

    private static void AddBind(List<string> statements, string variable, string path)
    {
      string normalized = NormalizeAssetPath(path);
      if (string.IsNullOrEmpty(normalized))
        return;
      statements.Add(string.Concat(variable, ".Bind(", Literal(normalized), ");"));
    }

    private static void EmitController(StringBuilder sb, Div div, string p, DivCodeWriteReport report, ref int serial)
    {
      DivController controller = div.Controller;
      string v = string.Concat("__c", serial++.ToString(CultureInfo.InvariantCulture));
      string typeName = TypeName(controller.GetType());
      List<string> statements = new List<string>();
      //[!] BindController 必须先执行: OnBinded 才会创建控制器自己的成员 (Gradient 的 tween 就在那儿).
      List<string> configuration = new List<string>();
      statements.Add(string.Concat(typeName, " ", v, " = new ", typeName, "();"));
      statements.Add(string.Concat(p, ".BindController(", v, ");"));
      switch (controller)
      {
        case LinearMenuController menu:
          configuration.Add(string.Concat(v, ".Direction = ", EnumLiteral(menu.Direction.GetType(), menu.Direction), ";"));
          configuration.Add(string.Concat(v, ".Alignment = ", EnumLiteral(menu.Alignment.GetType(), menu.Alignment), ";"));
          configuration.Add(string.Concat(v, ".Toward = ", EnumLiteral(menu.Toward.GetType(), menu.Toward), ";"));
          if (menu.DivInterval != 0)
            configuration.Add(string.Concat(v, ".DivInterval = ", menu.DivInterval.ToString(CultureInfo.InvariantCulture), ";"));
          if (menu.Scroll != Vector2.Zero)
            configuration.Add(string.Concat(v, ".Scroll = ", VectorLiteral(menu.Scroll), ";"));
          if (menu.AutoSetSize)
            configuration.Add(string.Concat(v, ".AutoSetSize = true;"));
          break;
        case DivGradientController gradient:
          configuration.Add(string.Concat(v, ".OpenColor.SetDuration(", F(gradient.OpenColor.Duration), ");"));
          configuration.Add(string.Concat(v, ".CloseColor.SetDuration(", F(gradient.CloseColor.Duration), ");"));
          configuration.Add(string.Concat(v, ".OpenScale.SetDuration(", F(gradient.OpenScale.Duration), ");"));
          configuration.Add(string.Concat(v, ".CloseScale.SetDuration(", F(gradient.CloseScale.Duration), ");"));
          break;
        default:
          if (controller.GetType().GetConstructor(Type.EmptyTypes) is null)
          {
            report.Issue(string.Concat("控制器 ", controller.GetType().Name, " 需要构造参数, 生成代码写不出来 (节点 ", div.Name, ")"));
            Line(sb, 3, string.Concat("// 控制器 ", controller.GetType().Name, " 需要构造参数 —— 请在脚本里手动绑定"));
            return;
          }
          if (UIDesignParts.FindController(controller) is not null)
            report.Issue(string.Concat("控制器 ", controller.GetType().Name, " 已登记在 UIDesignParts, 但生成器没有对应发射器 —— 参数会丢"));
          else
            report.Note(string.Concat("控制器 ", controller.GetType().Name, " 生成器只写了类型 (参数请在脚本里补)"));
          break;
      }
      statements.AddRange(configuration);
      for (int index = 0; index < statements.Count; index++)
        Line(sb, 3, statements[index]);
    }

    /// <summary>脚本: 绑定 + 作者态参数. 参数在绑定之后写, 保证脚本的 OnBind 已经跑过 (它可能补建内部件).</summary>
    /// <summary>已创建但尚未编译的脚本: 只写按类型名的预绑定 (没有实例, 导不出参数).</summary>
    private static void EmitPendingScript(StringBuilder sb, Div div, string p, DivCodeWriteReport report)
    {
      string typeName = div.PendingScriptType.Trim();
      if (IsQualifiedTypeName(typeName) is false)
      {
        report.Issue(string.Concat("节点 ", div.Name, " 的待编译脚本类型名不合法: ", typeName));
        return;
      }
      string qualified = typeName.StartsWith("global::", StringComparison.Ordinal) ? typeName : string.Concat("global::", typeName);
      Line(sb, 3, string.Concat(p, ".BindScript(new ", qualified, "());"));
      report.Scripts++;
      Line(sb, 3, string.Concat("// ↑ 预绑定: ", typeName, " 尚未编译; 编译后如需导出参数, 再导出一次"));
      report.Notes.Add(string.Concat("节点 ", div.Name, " 预绑定未编译脚本 ", typeName, " (只写 BindScript, 不导参数)"));
    }

    /// <summary>形如 A.B.C 的类型全名 (允许前缀 global::); 不校验类型是否真的存在.</summary>
    private static bool IsQualifiedTypeName(string value)
    {
      string text = value.StartsWith("global::", StringComparison.Ordinal) ? value.Substring("global::".Length) : value;
      if (string.IsNullOrEmpty(text))
        return false;
      string[] parts = text.Split('.');
      for (int index = 0; index < parts.Length; index++)
      {
        string part = parts[index];
        if (string.IsNullOrEmpty(part))
          return false;
        if (char.IsLetter(part[0]) is false && part[0] != '_')
          return false;
        for (int c = 1; c < part.Length; c++)
          if (char.IsLetterOrDigit(part[c]) is false && part[c] != '_')
            return false;
      }
      return true;
    }
    private static void EmitScript(StringBuilder sb, Div div, string p, DivCodeWriteReport report, ref int serial)
    {
      DivScript script = div.Script;
      Type type = script.GetType();
      if (type.GetConstructor(Type.EmptyTypes) is null)
      {
        report.Issue(string.Concat("脚本 ", type.Name, " 没有无参构造, 生成代码写不出来 (节点 ", div.Name, ")"));
        Line(sb, 3, string.Concat("// 脚本 ", type.Name, " 需要构造参数 —— 请在脚本里手动绑定"));
        return;
      }
      report.Scripts++;
      string v = string.Concat("__s", serial++.ToString(CultureInfo.InvariantCulture));
      Line(sb, 3, string.Concat(TypeName(type), " ", v, " = new ", TypeName(type), "();"));
      Line(sb, 3, string.Concat(p, ".BindScript(", v, ");"));
      List<UIDesignValue> values = new List<UIDesignValue>();
      script.ExportValues(values);
      for (int index = 0; index < values.Count; index++)
        EmitDesignValue(sb, div, v, values[index], report);
    }

    private static void EmitDesignValue(StringBuilder sb, Div div, string p, UIDesignValue value, DivCodeWriteReport report)
    {
      if (string.IsNullOrEmpty(value.Member))
      {
        report.Issue(string.Concat("节点 ", div.Name, " 的脚本参数缺少成员名"));
        return;
      }
      string literal = ValueLiteral(value, div, report);
      if (literal is null)
        return;
      Line(sb, 3, value.IsCall
        ? string.Concat(p, ".", value.Member, "(", literal, ");")
        : string.Concat(p, ".", value.Member, " = ", literal, ";"));
    }

    private static string ValueLiteral(UIDesignValue value, Div div, DivCodeWriteReport report)
    {
      switch (value.Kind)
      {
        case UIDesignValueKind.String:
          return Literal(value.Value as string);
        case UIDesignValueKind.Float:
          return F(Convert.ToSingle(value.Value, CultureInfo.InvariantCulture));
        case UIDesignValueKind.Int:
          return Convert.ToInt32(value.Value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        case UIDesignValueKind.Bool:
          return Bool(Convert.ToBoolean(value.Value, CultureInfo.InvariantCulture));
        case UIDesignValueKind.Enum:
          return value.Value is null ? null : EnumLiteral(value.Value.GetType(), value.Value);
        case UIDesignValueKind.Vector2:
          return VectorLiteral((Vector2)value.Value);
        case UIDesignValueKind.Point:
          return PointLiteral((Point)value.Value);
        case UIDesignValueKind.Color:
          return ColorLiteral((Color)value.Value);
        case UIDesignValueKind.Symbol:
          return value.Value as string;
        default:
          report.Issue(string.Concat("节点 ", div.Name, " 的脚本参数 ", value.Member, " 是未知值类型, 已跳过"));
          return null;
      }
    }

    // —— 字面量 ——

    private static string F(float value)
    {
      if (float.IsNaN(value) || float.IsInfinity(value))
        return "0f";
      if (value == 0f)
        return "0f";
      return string.Concat(value.ToString("R", CultureInfo.InvariantCulture), "f");
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private static string EnumLiteral(Type type, object value)
    {
      string typeName = TypeName(type);
      string name = type.IsEnum ? Enum.GetName(type, value) : null;
      if (string.IsNullOrEmpty(name) is false)
        return string.Concat(typeName, ".", name);
      return string.Concat("(", typeName, ")", Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
    }

    private static string VectorLiteral(Vector2 value)
      => string.Concat("new ", TypeName(typeof(Vector2)), "(", F(value.X), ", ", F(value.Y), ")");

    private static string PointLiteral(Point value)
      => string.Concat("new ", TypeName(typeof(Point)), "(",
        value.X.ToString(CultureInfo.InvariantCulture), ", ",
        value.Y.ToString(CultureInfo.InvariantCulture), ")");

    private static string RectangleLiteral(Rectangle value)
      => string.Concat("new ", TypeName(typeof(Rectangle)), "(",
        value.X.ToString(CultureInfo.InvariantCulture), ", ",
        value.Y.ToString(CultureInfo.InvariantCulture), ", ",
        value.Width.ToString(CultureInfo.InvariantCulture), ", ",
        value.Height.ToString(CultureInfo.InvariantCulture), ")");

    private static string ColorLiteral(Color color)
      => string.Concat("new ", TypeName(typeof(Color)), "(",
        color.R.ToString(CultureInfo.InvariantCulture), ", ",
        color.G.ToString(CultureInfo.InvariantCulture), ", ",
        color.B.ToString(CultureInfo.InvariantCulture), ", ",
        color.A.ToString(CultureInfo.InvariantCulture), ")");

    private static string Literal(string value)
    {
      if (value is null)
        return "null";
      StringBuilder sb = new StringBuilder(value.Length + 2);
      sb.Append('"');
      for (int index = 0; index < value.Length; index++)
      {
        char c = value[index];
        switch (c)
        {
          case '\\': sb.Append("\\\\"); break;
          case '"': sb.Append("\\\""); break;
          case '\n': sb.Append("\\n"); break;
          case '\r': sb.Append("\\r"); break;
          case '\t': sb.Append("\\t"); break;
          default:
            if (char.IsControl(c))
              sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
            else
              sb.Append(c);
            break;
        }
      }
      sb.Append('"');
      return sb.ToString();
    }

    internal static string NormalizeAssetPath(string path)
      => string.IsNullOrEmpty(path) ? path : path.Replace('\\', '/');

    private static string TypeName(Type type)
    {
      string full = type.FullName ?? type.Name;
      return string.Concat("global::", full.Replace('+', '.'));
    }

    private static string Indent(int depth) => new string(' ', depth * IndentUnit);

    private static void Line(StringBuilder sb, int depth, string text)
    {
      sb.Append(Indent(depth));
      sb.Append(text);
      sb.Append('\n');
    }
  }
}
