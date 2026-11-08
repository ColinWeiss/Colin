namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 锚定关系的合法性规则 (编辑器的连线、代码导出、以及运行时都按这一份判断).
  /// <br>锚定 = "我的布局依赖目标的矩形", 所以任何会让依赖成环的组合都必须拒绝:</br>
  /// <br>· 锚定到自己;</br>
  /// <br>· 锚定到自己的子级 (子级的位置又由我推出来);</br>
  /// <br>· 锚定到自己的父级 (我的位置由父级推, 父级位置又要由我推);</br>
  /// <br>· 目标已经直接/间接锚定到我 (或我的子树) —— 含"两个控件互相锚".</br>
  /// </summary>
  public static class DivAnchorRules
  {
    /// <summary>
    /// 能否把 <paramref name="self"/> 的锚定目标设为 <paramref name="target"/>.
    /// </summary>
    /// <param name="reason">不允许时的原因 (可直接显示给作者).</param>
    public static bool CanAnchor(Div self, Div target, out string reason)
    {
      reason = null;
      if (self is null || target is null || self.Disposed || target.Disposed)
      {
        reason = "节点无效或已释放";
        return false;
      }
      if (ReferenceEquals(self, target))
      {
        reason = "不能把锚定点到自己";
        return false;
      }
      if (IsDescendantOf(target, self))
      {
        reason = "不能锚定到自己的子级 (子级位置由我推出, 会形成循环)";
        return false;
      }
      if (IsDescendantOf(self, target))
      {
        reason = "不能锚定到自己的父级 (我的位置由父级推出, 会形成循环)";
        return false;
      }
      // 目标已经(直接或间接)依赖我 / 我的子树 → 再加我→目标就成环 (两个控件互相锚也在这一类)
      Div node = target.Style.AnchorTarget;
      for (int guard = 0; node is not null && guard < 64; guard++)
      {
        if (ReferenceEquals(node, self) || IsDescendantOf(node, self))
        {
          reason = "会形成锚定环 (目标已经直接或间接锚定到当前节点)";
          return false;
        }
        node = node.Style.AnchorTarget;
      }
      return true;
    }

    /// <summary>
    /// <paramref name="node"/> 是否是 <paramref name="root"/> 的后代 (不含 root 自己).
    /// </summary>
    public static bool IsDescendantOf(Div node, Div root)
    {
      if (node is null || root is null)
        return false;
      for (Div parent = node.Parent; parent is not null; parent = parent.Parent)
        if (ReferenceEquals(parent, root))
          return true;
      return false;
    }
  }
}