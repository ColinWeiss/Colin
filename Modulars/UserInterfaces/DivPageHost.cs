namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 页面/子树宿主: 保证"代码声明需要的那棵子树"在界面上存在.
  /// <br>节点都是裸 <see cref="Div"/>, 所以"这是什么页面"由<b>根上挂的脚本类型</b>回答: 按类型找, 缺失就按工厂重建,
  /// 再让页面自己跑一遍脚本绑定.</br>
  /// </summary>
  public static class DivPageHost
  {
    /// <summary>确保 <paramref name="root"/> 下存在挂着脚本 <typeparamref name="T"/> 的元素; 已存在则直接复用.</summary>
    public static Div Ensure<T>(Div root, Func<Div> factory) where T : DivScript
      => Ensure(root, typeof(T), factory);

    /// <summary>确保 <paramref name="root"/> 下存在挂着指定脚本的元素; 缺失时用工厂建一个并初始化.</summary>
    public static Div Ensure(Div root, Type pageScriptType, Func<Div> factory)
    {
      if (root is null || root.Disposed || pageScriptType is null || factory is null)
        return null;
      Div existing = FindByScript(root, pageScriptType);
      if (existing is not null)
        return existing;
      Div page = factory();
      if (page is null)
        return null;
      if (HasScript(page, pageScriptType) is false)
      {
        Console.Log(ConsoleTextType.Warning, "DivPageHost",
          string.Concat("工厂建出来的页面没有挂 ", pageScriptType.Name, ", 已忽略"));
        return null;
      }
      root.AttachDirect(page, doInit: true);
      page.RebindScriptTree();
      UIPageScripts.Run(page);
      Console.Log(ConsoleTextType.Normal, "DivPageHost",
        string.Concat("界面上没有 ", pageScriptType.Name, ", 已按代码重建"));
      return page;
    }

    /// <summary>
    /// 页面标识匹配: 传脚本类型 → 按挂的脚本认; 传 Div 子类 (过渡期的旧页面类) → 按节点类型认.
    /// </summary>
    private static bool HasScript(Div div, Type scriptType)
    {
      if (div is null || scriptType is null)
        return false;
      if (typeof(Div).IsAssignableFrom(scriptType))
        return scriptType.IsInstanceOfType(div);
      return div.Script is not null && scriptType.IsInstanceOfType(div.Script);
    }

    private static Div FindByScript(Div node, Type scriptType)
    {
      if (node is null || node.Disposed)
        return null;
      if (HasScript(node, scriptType))
        return node;
      List<Div> children = node.Children;
      for (int index = 0; index < children.Count; index++)
      {
        Div found = FindByScript(children[index], scriptType);
        if (found is not null)
          return found;
      }
      return null;
    }
  }
}
