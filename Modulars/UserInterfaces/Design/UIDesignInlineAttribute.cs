namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 声明该划分元素类型<b>不需要自己的 <c>X.UI.cs</c></b> —— 它的外观与子级由父级那份生成文件代劳.
  /// <br>判据: <b>同一个类的两个实例长得不一样</b>. 典型是"参数化原子件": 图标按钮 (每个按钮的贴图不同)、
  /// 物品格 (每个格子显示不同物品)、输入槽 (标签文字不同)、<c>Label</c> 这类文本件.</br>
  /// <br>[!] <b>结构一律由生成代码给出</b> —— 有 <c>X.UI.cs</c> 的类型不能在 <c>DivInit</c> 里 <c>new</c> 子节点,
  /// 否则会与生成结构各建一遍 (树里出现两份). 原子件没有自己的文件, 所以它的结构写在 <c>DivInit</c> 里仍是唯一一份;
  /// 但"每个实例不同的值"必须通过 <see cref="Div.ExportDesignValues"/> 交给父级写回.</br>
  /// <br>[!] 标错方向的代价:</br>
  /// <br>  · 该标没标 → 父级不再写它的内容, 那些"每个实例都不同"的贴图/文字会丢 (看得见);</br>
  /// <br>  · 不该标却标了 → 父级把它的子级抄进自己文件, 而它自己 <c>DivInit</c> 又建一遍 (重复节点, 也看得见).</br>
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class UIDesignInlineAttribute : Attribute
  {
  }
}
