namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 实体系统喵
  /// </summary>
  public class Entitiesystem
  {
    internal Ecs _ecs;
    public Ecs Ecs => _ecs;

    /// <summary>
    /// 执行系统初始化.
    /// <br>该方法将于系统被加入列表中时执行.</br>
    /// </summary>
    public virtual void DoInitialize() { }
    public virtual void Reset() { }

    public virtual void Start() { }
    public virtual void DoUpdate() { }
    public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }
  }
}