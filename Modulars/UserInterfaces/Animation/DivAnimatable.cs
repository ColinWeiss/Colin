namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>轨道值类型, 决定分量数与插值语义.</summary>
  public enum TrackValueType
  {
    Float,
    Vector2,
    Color,
    Bool,
  }

  /// <summary>
  /// 可动画属性登记表: 属性路径 (C# 命名, 如 "Layout.Left") ↔ Div 读写器.
  /// <br>轨道 / Auto-Key / 打帧按钮共用这一份路径真相; 扩充可动画属性在此加一行.</br>
  /// </summary>
  public static class DivAnimatable
  {
    /// <summary>单个可动画属性的描述.</summary>
    public sealed class Property
    {
      public string Path;
      public string Label;
      public TrackValueType Type;
      public Func<Div, float[]> Get;
      public Action<Div, float[]> Set;
    }

    public static readonly List<Property> Properties = new List<Property>
    {
      new Property { Path = "Layout.Left", Label = "位置.X", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.Left }, Set = (div, value) => div.Layout.Left = value[0] },
      new Property { Path = "Layout.Top", Label = "位置.Y", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.Top }, Set = (div, value) => div.Layout.Top = value[0] },
      new Property { Path = "Layout.Width", Label = "宽度", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.Width }, Set = (div, value) => div.Layout.Width = value[0] },
      new Property { Path = "Layout.Height", Label = "高度", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.Height }, Set = (div, value) => div.Layout.Height = value[0] },
      new Property { Path = "Layout.Rotation", Label = "旋转", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.Rotation }, Set = (div, value) => div.Layout.Rotation = value[0] },
      new Property { Path = "Layout.ScaleX", Label = "缩放.X", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.ScaleX }, Set = (div, value) => div.Layout.ScaleX = value[0] },
      new Property { Path = "Layout.ScaleY", Label = "缩放.Y", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.ScaleY }, Set = (div, value) => div.Layout.ScaleY = value[0] },
      new Property { Path = "Layout.AnchorX", Label = "轴点.X", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.AnchorX }, Set = (div, value) => div.Layout.AnchorX = value[0] },
      new Property { Path = "Layout.AnchorY", Label = "轴点.Y", Type = TrackValueType.Float,
        Get = div => new float[] { div.Layout.AnchorY }, Set = (div, value) => div.Layout.AnchorY = value[0] },
      new Property { Path = "Design.Color", Label = "颜色", Type = TrackValueType.Color,
        Get = div => new float[] { div.Design.Color.R / 255f, div.Design.Color.G / 255f, div.Design.Color.B / 255f, div.Design.Color.A / 255f },
        Set = (div, value) => div.Design.Color = new Color(
          (byte)(Math.Clamp(value[0], 0f, 1f) * 255f + 0.5f), (byte)(Math.Clamp(value[1], 0f, 1f) * 255f + 0.5f),
          (byte)(Math.Clamp(value[2], 0f, 1f) * 255f + 0.5f), (byte)(Math.Clamp(value[3], 0f, 1f) * 255f + 0.5f)) },
      new Property { Path = "IsVisible", Label = "可见性", Type = TrackValueType.Bool,
        Get = div => new float[] { div.IsVisible ? 1f : 0f }, Set = (div, value) => div.IsVisible = value[0] >= 0.5f },
    };

    private static readonly Dictionary<string, Property> _lookup = CreateLookup();

    private static Dictionary<string, Property> CreateLookup()
    {
      Dictionary<string, Property> lookup = new Dictionary<string, Property>();
      for (int index = 0; index < Properties.Count; index++)
        lookup[Properties[index].Path] = Properties[index];
      return lookup;
    }

    public static bool TryGet(string path, out Property property)
      => _lookup.TryGetValue(path, out property);

    /// <summary>读取目标节点指定属性的当前分量值; 属性路径未知时返回 null.</summary>
    public static float[] Read(Div div, string path)
      => _lookup.TryGetValue(path, out Property property) ? property.Get(div) : null;

    /// <summary>把分量值写入目标节点指定属性; 属性路径未知时忽略.</summary>
    public static void Write(Div div, string path, float[] value)
    {
      if (_lookup.TryGetValue(path, out Property property))
        property.Set(div, value);
    }
  }

  /// <summary>按稳定 Id 查找节点 (动画轨道绑定 / 播放寻址用).</summary>
  public static class DivFinder
  {
    public static Div FindById(this UserInterface ui, string id)
    {
      if (ui?.Root is null || string.IsNullOrEmpty(id))
        return null;
      return Search(ui.Root, id);
    }

    private static Div Search(Div div, string id)
    {
      if (div.Id == id)
        return div;
      List<Div> children = div.Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div found = Search(children[index], id);
        if (found is not null)
          return found;
      }
      return null;
    }
  }
}
