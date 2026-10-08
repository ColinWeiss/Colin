using Colin.Core.Modulars.Ecses.Systems;

namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 物块交互组件.
  /// <br>它将被 <see cref="EcsTileCollisionSystem"/> 解析.</br>
  /// </summary>
  public class EcsComTileInteract : IEcsCom, IResetable
  {
    public bool IsOnSlope { get; set; }

    public Vector2 SlopeNormal { get; set; }

    /// <summary>
    /// 指示实体当前所处的层.
    /// </summary>
    public int Layer;

    /// <summary>
    /// 指示是否无视物块碰撞的值.
    /// </summary>
    public bool IgnoreTile;

    /// <summary>
    /// 指示碰撞是否启用点状计算.
    /// <br>启用后, 实体只以碰撞盒底部中点这一个点参与物块碰撞, 适用于掉落物/弹幕等无需严格碰撞的实体.</br>
    /// <br>该值默认为 <see langword="true"/>; 角色/生物等需要严格矩形碰撞的实体应显式设为 <see langword="false"/>.</br>
    /// </summary>
    public bool PointLike = true;

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

    public bool SlopeCollision;

    public bool IsCollision => CollisionLeft || CollisionRight || CollisionBottom || CollisionTop;

    public bool ResetEnable { get; set; } = true;

    public bool PreviousCollisionLeft;

    public bool PreviousCollisionRight;

    public bool PreviousCollisionTop;

    public bool PreviousCollisionBottom;

    public bool PreviousSlopeCollision;

    /// <summary>
    /// 指示基础碰撞盒.
    /// <br>其中, X、Y 用作针对 <see cref="Transform2D.Translation"/> 的偏移.</br>
    /// <br>Width、Height用作针对 <see cref="EcsComTransform.Size"/> 的增减.</br>
    /// </summary>
    public RectangleF Hitbox;

    public void DoInitialize()
    {
      IgnoreTile = false;
      PointLike = true;
    }
    public void Reset()
    {
      IgnoreTile = false;
    }
  }
}