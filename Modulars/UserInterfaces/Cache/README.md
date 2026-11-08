# Cache —— 新建 UI 的暂存区

UI 编辑器导出代码时：
- **已经存在同名 builder 类** → 直接把 `XxxUI.UI.cs` 写在它旁边（按命名空间/文件位置打）；
- **还没有类的全新 UI** → 先落在这里：

```
Colin/Modulars/UserInterfaces/Cache/<类名>.UI.cs
```

这是**暂存区，不是归宿**。导出后请自己把文件拖到你想要的命名空间目录（并确认
`DeltaMachine.Core.projitems` 里那条 `<Compile Include>` 跟着改路径 —— 编辑器导出时
会自动登记，但**挪文件不会自动改登记**，留下失效路径会直接编译失败）。

## 导出的东西怎么用

生成的是**纯结构 builder**，没有任何运行时入口。要在游戏里生效，必须自己接上：

```csharp
// 页面工厂 / 父级 builder 里组合
ShortcutUI.Handles handles = new ShortcutUI.Handles();
Div shortcut = ShortcutUI.Create(handles, "Shortcut");
parent.Attach(shortcut);
```

或者把它当"子面板"塞进已有页面：

```csharp
ShortcutUI.Build(parent, handles);   // 内容建到 parent 下
```

没有被调用的 `XxxUI.UI.cs` 只是死代码 —— **改它不会影响游戏**。

## 两条注意

1. **脚本自持结构的节点不会被导出**。`OwnsStructure => true` 的脚本（FormScript、
   ShortcutScript、ItemSlotScript…）在 `OnBind` 里现建内部件，导出只写 `BindScript`；
   重新导出后这些内部件仍然不在生成文件里，这是对的（不然运行时会建两套）。
2. **被脚本逐帧接管的布局改不动**。`LayoutOwnership = Scripted` / 脚本 `OnUpdate` 里
   写 `Layout.Left/Top/Width/Height` 的节点（选中框、滚动条、列表内容、Form 的 View/边框…），
   作者值会在运行期被覆盖；要改这类位置，改脚本，不要改导出值。