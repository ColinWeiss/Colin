namespace Particle.Editor
{
  /// <summary>编辑器命令接口 (命令模式): 一切可撤销的编辑操作都封装为实现.</summary>
  public interface IEditorCommand
  {
    /// <summary>命令描述 (撤销列表显示).</summary>
    string Label { get; }

    /// <summary>执行 (首次应用).</summary>
    void Do();

    /// <summary>撤销.</summary>
    void Undo();
  }

  /// <summary>委托式简单命令: 适用于单值修改 (滑条拖动完成后的提交).</summary>
  public sealed class DelegateCommand : IEditorCommand
  {
    private readonly Action _apply;
    private readonly Action _revert;

    public string Label { get; }

    public DelegateCommand(string label, Action apply, Action revert)
    {
      Label = label;
      _apply = apply;
      _revert = revert;
    }

    public void Do() => _apply();

    public void Undo() => _revert();
  }

  /// <summary>
  /// 编辑器命令历史: 统一管理撤销/重做 (命令模式), 默认保留 128 步.
  /// </summary>
  public sealed class EditorCommandHistory
  {
    /// <summary>历史容量上限.</summary>
    public const int MaxHistory = 128;

    private readonly Stack<IEditorCommand> _undo = new Stack<IEditorCommand>();
    private readonly Stack<IEditorCommand> _redo = new Stack<IEditorCommand>();

    /// <summary>最近一次命令描述 (诊断).</summary>
    public string LastLabel { get; private set; }

    /// <summary>能否撤销.</summary>
    public bool CanUndo => _undo.Count > 0;

    /// <summary>能否重做.</summary>
    public bool CanRedo => _redo.Count > 0;

    /// <summary>执行命令并压入撤销栈 (清空重做栈).</summary>
    public void Execute(IEditorCommand command)
    {
      command.Do();
      _undo.Push(command);
      LastLabel = command.Label;
      if (_undo.Count > MaxHistory)
      {
        // 移除最旧条目: 通过辅助栈重建.
        Stack<IEditorCommand> buffer = new Stack<IEditorCommand>(_undo.Count);
        while (_undo.Count > 1)
          buffer.Push(_undo.Pop());
        _undo.Clear();
        while (buffer.Count > 0)
          _undo.Push(buffer.Pop());
      }
      _redo.Clear();
    }

    /// <summary>撤销最近一条命令.</summary>
    public void Undo()
    {
      if (!CanUndo)
        return;
      IEditorCommand command = _undo.Pop();
      command.Undo();
      _redo.Push(command);
      LastLabel = command.Label;
    }

    /// <summary>重做最近被撤销的命令.</summary>
    public void Redo()
    {
      if (!CanRedo)
        return;
      IEditorCommand command = _redo.Pop();
      command.Do();
      _undo.Push(command);
      LastLabel = command.Label;
    }

    /// <summary>清空历史 (加载新配置时调用).</summary>
    public void Clear()
    {
      _undo.Clear();
      _redo.Clear();
      LastLabel = null;
    }
  }
}
