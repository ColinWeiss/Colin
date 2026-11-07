namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 标记"界面上应当唯一"的元素类型 (每页一个): 代码按类型找它时, 若树上出现多个实例, 会在日志里点名而不是静默取第一个.
  /// <br>用于 <c>ChestView</c> / <c>Shortcut</c> / 页面根这类"只能有一个"的东西; 像 <c>ItemSlot</c> 这种复数类型不要标.</br>
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true)]
  public sealed class UIUniqueAttribute : Attribute
  {
  }
}
