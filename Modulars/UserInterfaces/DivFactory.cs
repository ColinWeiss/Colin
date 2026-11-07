using Colin.Core.Modulars.UserInterfaces.Renderers;
using Colin.Core.Modulars.UserInterfaces.Scripts;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// Div 的构造工厂与查询扩展: <b>节点一律是 <see cref="Div"/>, 差别只在挂了什么组件</b>.
  /// <br>这里是"手写代码的便捷入口"; 编辑器导出的是同样形状的 <c>new Div(...)</c> + <c>BindRenderer/BindController/BindScript</c>.</br>
  /// </summary>
  public static class DivFactory
  {
    /// <summary>界面根: 尺寸每帧同步视口.</summary>
    public static Div Root(string name)
    {
      Div div = new Div(name);
      div.MarkAsRoot();
      return div;
    }

    /// <summary>建一个子节点并挂上去.</summary>
    public static Div Node(this Div parent, string name)
    {
      Div div = new Div(name);
      parent?.Attach(div);
      return div;
    }

    /// <summary>建一个画布子节点 (渲染目标).</summary>
    public static Div CanvasNode(this Div parent, string name)
    {
      Div div = parent.Node(name);
      div.MarkAsCanvas();
      return div;
    }

    /// <summary>建一个文本节点: 挂字体渲染器并写入文字.</summary>
    public static Div Label(this Div parent, string name, string text = "")
    {
      Div div = parent.Node(name);
      div.BindRenderer<DivFontRenderer>();
      div.SetText(text);
      return div;
    }

    /// <summary>建一个按钮: 像素底 + <c>"Text"</c> 文本子节点 + <see cref="ButtonScript"/>.</summary>
    public static Div Button(this Div parent, string name, string text = "")
    {
      Div div = parent.Node(name);
      div.Design.Color = UITheme.ButtonNormal;
      Div caption = div.Node(ButtonScript.TextNodeName);
      DivFontRenderer font = caption.BindRenderer<DivFontRenderer>();
      font.BindFont("Fonts/GlowSansMedium.ttf", (int)UITheme.ButtonFontSize);
      caption.Design.Color = UITheme.ButtonText;
      caption.SetText(text);
      div.BindScript(new ButtonScript());
      return div;
    }

    /// <summary>建一个滑动条: 像素底 + <c>"Block"</c> 滑块 + <see cref="SliderScript"/>.</summary>
    public static Div Slider(this Div parent, string name, Direction direction = Direction.Vertical)
    {
      Div div = parent.Node(name);
      div.BindScript(new SliderScript { Direction = direction });
      return div;
    }

    /// <summary>建一个滚动视口: <c>"Content"</c> + <c>"Slider"</c> + <see cref="ScrollViewScript"/>.</summary>
    public static Div ScrollView(this Div parent, string name, Direction direction = Direction.Vertical)
    {
      Div div = parent.Node(name);
      div.BindScript(new ScrollViewScript { Direction = direction });
      return div;
    }

    /// <summary>建一个列表容器: <c>"List"</c> + <see cref="DivListScript"/>.</summary>
    public static Div DivList(this Div parent, string name)
    {
      Div div = parent.Node(name);
      div.BindScript(new DivListScript());
      return div;
    }

    /// <summary>建一个单行输入框: <c>"Text"</c> + <see cref="InputTextBoxScript"/>.</summary>
    public static Div InputBox(this Div parent, string name, int limit = 16)
    {
      Div div = parent.Node(name);
      div.BindScript(new InputTextBoxScript { Limit = limit });
      return div;
    }

    /// <summary>建一个进度条: <c>"Fill"</c> + <see cref="ProgressBarScript"/>.</summary>
    public static Div ProgressBar(this Div parent, string name)
    {
      Div div = parent.Node(name);
      div.BindScript(new ProgressBarScript());
      return div;
    }

    // —— 组件查询 (把旧类型上的字段访问换成"按约定形状取") ——

    /// <summary>取本节点的字体渲染器; 没有就补一个.</summary>
    public static DivFontRenderer FontRenderer(this Div div)
    {
      if (div is null)
        return null;
      if (div.Renderer is DivFontRenderer font)
        return font;
      return div.BindRenderer<DivFontRenderer>();
    }

    /// <summary>取"文字渲染器": 优先本节点, 其次子节点 <c>"Text"</c>.</summary>
    public static DivFontRenderer TextRenderer(this Div div)
    {
      if (div is null)
        return null;
      if (div.Renderer is DivFontRenderer font)
        return font;
      Div text = div.GetChild("Text");
      if (text is null)
        return null;
      if (text.Renderer is DivFontRenderer textFont)
        return textFont;
      return text.BindRenderer<DivFontRenderer>();
    }

    /// <summary>取 <c>"Text"</c> 文本子节点.</summary>
    public static Div TextNode(this Div div) => div?.GetChild("Text");

    /// <summary>写文字 (写到文字渲染器上).</summary>
    public static void SetText(this Div div, string text)
    {
      DivFontRenderer font = div.TextRenderer();
      if (font is not null)
        font.Text = text ?? string.Empty;
    }

    /// <summary>读文字.</summary>
    public static string GetText(this Div div) => div.TextRenderer()?.Text ?? string.Empty;

    /// <summary>取滑动条的滑块节点.</summary>
    public static Div SliderBlock(this Div div)
      => div is null ? null : div.GetScript<SliderScript>()?.Block ?? div.GetChild(SliderScript.BlockNodeName);

    /// <summary>取滚动/列表容器的内容容器 (项挂到它下面).</summary>
    public static Div ContentNode(this Div div)
    {
      if (div is null)
        return null;
      DivScript script = div.Script;
      return script switch
      {
        ScrollViewScript scroll => scroll.Content,
        DivListScript list => list.List,
        _ => div.GetChild("Content") ?? div.GetChild(DivListScript.ListNodeName),
      };
    }

    /// <summary>给一个节点挂脚本 (返回节点, 便于链式).</summary>
    public static Div WithScript<T>(this Div div, T script) where T : DivScript
    {
      div?.BindScript(script);
      return div;
    }
  }
}
