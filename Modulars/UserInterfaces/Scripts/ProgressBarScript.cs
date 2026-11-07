namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 进度条行为: 底条 + 按比例派生的填充条.
  /// <br>节点形状: 子级 <c>"Fill"</c> 是填充条; Fill 的尺寸逐帧派生, 因此标记为
  /// <see cref="DivLayoutOwnership.Scripted"/> 锁定编辑器手改.</br>
  /// </summary>
  public sealed class ProgressBarScript : DivScript
  {
    /// <summary>填充条子节点的约定名字.</summary>
    public const string FillNodeName = "Fill";

    /// <summary>填充百分比 (0..1); 默认满格, 让作者态在编辑器里可见.</summary>
    public float Percentage = 1f;

    public Direction Direction = Direction.Horizontal;

    public Direction Toward = Direction.Right;

    public Point FillOffset;

    private Div _fill;

    /// <summary>填充条节点.</summary>
    public Div Fill => _fill ??= Owner?.GetChild(FillNodeName);

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      _fill = owner.GetChild(FillNodeName);
      if (_fill is null)
      {
        _fill = new Div(FillNodeName);
        owner.Attach(_fill);
      }
      _fill.LayoutScripted = true;
      //初值同步: 没有 OnUpdate 的场合 (编辑器载入当帧) 也能看到合理的长宽.
      if (Direction == Direction.Vertical && _fill.Layout.Height == 0f)
        _fill.Layout.Height = (int)(Percentage * owner.Layout.Height);
      if (Direction == Direction.Horizontal && _fill.Layout.Width == 0f)
        _fill.Layout.Width = (int)(Percentage * owner.Layout.Width);
    }

    public override void ExportValues(List<UIDesignValue> values)
    {
      values.Add(UIDesignValue.Field(nameof(Percentage), Percentage));
      values.Add(UIDesignValue.Field(nameof(Direction), Direction));
      values.Add(UIDesignValue.Field(nameof(Toward), Toward));
      values.Add(UIDesignValue.Field(nameof(FillOffset), FillOffset));
      base.ExportValues(values);
    }

    protected override void OnUpdate(GameTime time)
    {
      Div owner = Owner;
      if (owner is null || _fill is null)
        return;
      if (Direction == Direction.Vertical)
      {
        if (Toward == Direction.Down)
          _fill.Layout.Height = (int)(Percentage * owner.Layout.Height);
        else if (Toward == Direction.Up)
        {
          _fill.Layout.Top = owner.Layout.Height - (int)(Percentage * owner.Layout.Height) + FillOffset.Y;
          _fill.Layout.Height = (int)(Percentage * owner.Layout.Height);
        }
      }
      else if (Direction == Direction.Horizontal)
      {
        if (Toward == Direction.Right)
          _fill.Layout.Width = (int)(Percentage * owner.Layout.Width);
        else if (Toward == Direction.Left)
        {
          _fill.Layout.Left = owner.Layout.Width - (int)(Percentage * owner.Layout.Width) + FillOffset.X;
          _fill.Layout.Width = (int)(Percentage * owner.Layout.Width);
        }
      }
    }
  }
}
