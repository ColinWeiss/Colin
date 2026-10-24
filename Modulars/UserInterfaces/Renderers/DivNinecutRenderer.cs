namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivNinecutRenderer : DivRenderer
  {
    private Sprite _sprite;
    public Sprite Sprite => _sprite;
    public Point Cut;
    public override void OnDivInitialize() { }
    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      if (_sprite is null)
        return;
      batch.DrawNineCut(
          _sprite.Source,
          Div.Design.Color,
          Div.Layout.RenderTargetLocation + div.Layout.Anchor,
          Div.Layout.Size,
          Cut,
          _sprite.Depth);
    }
    public DivNinecutRenderer Bind(Sprite sprite)
    {
      _sprite = sprite;
      EnsureCut();
      return this;
    }
    public DivNinecutRenderer Bind(Texture2D texture)
    {
      _sprite = new Sprite(texture);
      EnsureCut();
      return this;
    }
    public DivNinecutRenderer Bind(string path)
    {
      Bind(Sprite.TryGet(path)); //寻不到时静默留空, 渲染跳过
      return this;
    }
    public DivNinecutRenderer Bind(params string[] path)
    {
      Bind(Sprite.TryGet(path));
      return this;
    }
    /// <summary>切边未设 ((0,0)) 时按贴图短边 1/3 自动推导, 避免零切边退化成整图拉伸; 贴图小到切不开两刀时保持 0.</summary>
    private void EnsureCut()
    {
      if (Cut != Point.Zero || _sprite?.Source is null)
        return;
      int min = Math.Min(_sprite.Source.Width, _sprite.Source.Height);
      int cut = min / 3;
      if (cut >= 1 && cut * 2 < min)
        Cut = new Point(cut);
    }
    public void SetCut(int x, int y)
    {
      Cut = new Point(x, y);
    }
    public void SetCut(int size)
    {
      Cut = new Point(size);
    }
  }
}