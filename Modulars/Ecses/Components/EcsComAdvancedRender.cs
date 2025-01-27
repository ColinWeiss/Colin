namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 高级渲染组件. 是一类特殊的Script, 在Render函数中实现渲染.
  /// <br>需要手动使用Batch. 如果需要使用合批, 使用EcsComDeferredRender. </br>
  /// </summary>
  public abstract class EcsComAdvancedRender : EcsComScript
  {
    public List<Sprite> Sprites = new();
    public SpriteEffects SpriteEffects;
    public Sprite Sprite
    {
      get => Sprites.Count > 0 ? Sprites[0] : null;
      set
      {
        if (Sprites.Count <= 0)
        {
          Sprites.Add(value);
        }
        Sprites[0] = value;
      }
    }
    public bool Visible = true;
    public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }
  }
}