using Colin.Core.Modulars.Ecses.Systems;

namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 物理组件.
  /// <br>它将被 <see cref="EcsTileCollisionSystem"/> 解析.</br>
  /// </summary>
  public class EcsComPhysic : ISectionCom, IResetable
  {
    /// <summary>
    /// 指示是否忽略重力的值.
    /// </summary>
    public bool IgnoreGravity = false;

    /// <summary>
    /// 指示切片当前所处的层.
    /// </summary>
    public int Layer;

    /// <summary>
    /// 指示是否无视物块碰撞的值.
    /// </summary>
    public Entrance<bool> IgnoreTile;

    /// <summary>
    /// 指示各个方向上的空气阻力.
    /// </summary>
    public Entrance<Vector2> AirResistance;

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

    /// <summary>
    /// 用于 <see cref="EcsTileCollisionSystem"/> 计算的值之一.
    /// </summary>
    public float PreviousBottom;

    /// <summary>
    /// 用于 <see cref="EcsTileCollisionSystem"/> 计算的值之一.
    /// </summary>
    public float PreviousTop;

    /// <summary>
    /// 用于 <see cref="EcsTileCollisionSystem"/> 计算的值之一.
    /// </summary>
    public float PreviousLeft;

    /// <summary>
    /// 用于 <see cref="EcsTileCollisionSystem"/> 计算的值之一.
    /// </summary>
    public float PreviousRight;

    /// <summary>
    /// 指示该切片上一帧触发了碰撞.
    /// </summary>
    public bool PreviousCollisionBottom;

    /// <summary>
    /// 用于 <see cref="EcsTileCollisionSystem"/> 计算的值之一.
    /// </summary>
    public Vector2 PreviousPosition;

    /// <summary>
    /// 指示基础碰撞盒.
    /// <br>其中, X、Y 用作针对 <see cref="Transform2D.Translation"/> 的偏移.</br>
    /// <br>Width、Height用作针对 <see cref="EcsComTransform.Size"/> 的增减.</br>
    /// </summary>
    public RectangleF Hitbox;

    public void DoInitialize()
    {
      IgnoreTile = false;
      AirResistance = Vector2.Zero;
      UniGravitySpeedAttTime = 0.24f;
    }
    public void Reset()
    {
      IgnoreTile.Reset();
      AirResistance.Reset();
      UniGravitySpeedAttTime.Reset();
    }
  }
}