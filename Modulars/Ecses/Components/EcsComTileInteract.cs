using Colin.Core.Modulars.Ecses.Systems;

namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 物块交互组件.
  /// <br>它将被 <see cref="EcsTileCollisionSystem"/> 解析.</br>
  /// </summary>
  public class EcsComTileInteract : IEntityCom, IResetable
  {
    /// <summary>
    /// 指示是否忽略重力的值.
    /// </summary>
    public bool IgnoreGravity = false;

    /// <summary>
    /// 指示实体当前所处的层.
    /// </summary>
    public int Layer;

    /// <summary>
    /// 指示是否无视物块碰撞的值.
    /// </summary>
    public bool IgnoreTile;

    /// <summary>
    /// 指示达到最大重力速度的时间.
    /// </summary>
    public Entrance<float> UniGravitySpeedAttTime;

    /// <summary>
    /// 指示基础碰撞盒是否拥有左侧碰撞状态.
    /// </summary>
    public bool CollisionLeft;

    /// <summary>
    /// 指示基础碰撞盒是否拥有右侧碰撞状态.
    /// </summary>
    public bool CollisionRight;

    /// <summary>
    /// 指示基础碰撞盒是否拥有底部碰撞状态.
    /// </summary>
    public bool CollisionBottom;

    /// <summary>
    /// 指示基础碰撞盒是否拥有顶部碰撞状态.
    /// </summary>
    public bool CollisionTop;

    public bool IsCollision => CollisionLeft || CollisionRight || CollisionBottom || CollisionTop;

    public bool ResetEnable { get; set; } = true;

    public bool PreviousCollisionLeft;

    public bool PreviousCollisionRight;

    public bool PreviousCollisionTop;

    public bool PreviousCollisionBottom;

    /// <summary>
    /// 指示基础碰撞盒.
    /// <br>其中, X、Y 用作针对 <see cref="Transform2D.Translation"/> 的偏移.</br>
    /// <br>Width、Height用作针对 <see cref="EcsComTransform.Size"/> 的增减.</br>
    /// </summary>
    public RectangleF Hitbox;

    public void DoInitialize()
    {
      IgnoreTile = false;
      UniGravitySpeedAttTime = new Entrance<float>(0.24f);
    }
    public void Reset()
    {
      IgnoreTile = false;
      UniGravitySpeedAttTime.Reset();
    }
  }
}