namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// <see cref="UIDesignValue"/> 承载的值类型; 决定代码生成器该写成哪种字面量.
  /// </summary>
  public enum UIDesignValueKind
  {
    String,
    Float,
    Int,
    Bool,
    Enum,
    Vector2,
    Point,
    Color,
    /// <summary>原样写入的表达式 (如另一节点的句柄字段名); 生成器不做引用检查.</summary>
    Symbol,
  }

  /// <summary>
  /// 一条"这个实例的作者态参数": 目标成员名 + 值 (+ 是赋值还是单参方法调用).
  /// <br>由 <see cref="Div.ExportDesignValues"/> 产出, 代码生成器把它写进父级那份 <c>X.UI.cs</c> 的
  /// <c>OnStructureReady</c> 里 (此刻子单元自己的结构已建好, 设值不会落空).</br>
  /// <example>
  /// <code>
  /// public override void ExportDesignValues(List&lt;UIDesignValue&gt; values)
  /// {
  ///   values.Add(UIDesignValue.Call("SetText", Caption));
  ///   values.Add(UIDesignValue.Field("Percentage", Percentage));
  /// }
  /// </code>
  /// </example>
  /// </summary>
  public readonly struct UIDesignValue
  {
    /// <summary>目标成员名 (字段/属性名, 或 <see cref="IsCall"/> 为真时的方法名).</summary>
    public readonly string Member;

    public readonly UIDesignValueKind Kind;

    public readonly object Value;

    /// <summary>是否写成单参方法调用 (<c>Member(value)</c>) 而非赋值 (<c>Member = value</c>).</summary>
    public readonly bool IsCall;

    private UIDesignValue(string member, UIDesignValueKind kind, object value, bool isCall)
    {
      Member = member;
      Kind = kind;
      Value = value;
      IsCall = isCall;
    }

    public static UIDesignValue Field(string member, string value)
      => new UIDesignValue(member, UIDesignValueKind.String, value, false);

    public static UIDesignValue Field(string member, bool value)
      => new UIDesignValue(member, UIDesignValueKind.Bool, value, false);

    public static UIDesignValue Field(string member, int value)
      => new UIDesignValue(member, UIDesignValueKind.Int, value, false);

    public static UIDesignValue Field(string member, float value)
      => new UIDesignValue(member, UIDesignValueKind.Float, value, false);

    public static UIDesignValue Field(string member, Enum value)
      => new UIDesignValue(member, UIDesignValueKind.Enum, value, false);

    public static UIDesignValue Field(string member, Vector2 value)
      => new UIDesignValue(member, UIDesignValueKind.Vector2, value, false);

    public static UIDesignValue Field(string member, Point value)
      => new UIDesignValue(member, UIDesignValueKind.Point, value, false);

    public static UIDesignValue Field(string member, Color value)
      => new UIDesignValue(member, UIDesignValueKind.Color, value, false);

    public static UIDesignValue Call(string member, string value)
      => new UIDesignValue(member, UIDesignValueKind.String, value, true);

    public static UIDesignValue Call(string member, bool value)
      => new UIDesignValue(member, UIDesignValueKind.Bool, value, true);

    public static UIDesignValue Call(string member, int value)
      => new UIDesignValue(member, UIDesignValueKind.Int, value, true);

    public static UIDesignValue Call(string member, float value)
      => new UIDesignValue(member, UIDesignValueKind.Float, value, true);

    /// <summary>写一个原样表达式 (用于跨单元接线: 目标是同一个方法体里另一个节点的字段名).</summary>
    public static UIDesignValue Symbol(string member, string expression)
      => new UIDesignValue(member, UIDesignValueKind.Symbol, expression, false);

    public override string ToString()
      => string.Concat(Member, IsCall ? "(" : "=", Value?.ToString() ?? "null");
  }
}
