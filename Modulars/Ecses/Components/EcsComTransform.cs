using Colin.Core.Modulars.Ecses.Systems;

namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// [默认组件]
    /// <br>提供关于位置/速度/旋转/缩放的字段.</br>
    /// <br>它将被 <see cref="EcsPhysicSystem"/> 解析.</br>
    /// </summary>
    public class EcsComTransform : ISectionComponent
    {
        public Vector2 Size;
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 MaxVelocity; //最大速度限制.
        public Vector2 Scale = Vector2.One;
        public float Rotation;

        /// <summary>
        /// 获取 <see cref="Size"/> 的一半.
        /// </summary>
        public Vector2 Half => Size / 2;

        /// <summary>
        /// 获取 <see cref="Position"/> + <see cref="Half"/>.
        /// </summary>
        public Vector2 Center => Position + Half;

        /// <summary>
        /// 获取由 <see cref="Position"/> 和 <see cref="Size"/> 计算得出的默认矩形.
        /// </summary>
        public RectangleF LocalBound => new RectangleF(Position, Size);

        public EcsComTransform SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }
        public EcsComTransform SetSize(float width, float height) => SetSize(new Vector2(width, height));

        /// <summary>
        /// 指示切片当前是否正在进行横向移动.
        /// </summary>
        public bool OnHorizontalMove => Math.Abs(Velocity.X) > 0;
        /// <summary>
        /// 指示切片当前是否正在进行纵向移动.
        /// </summary>
        public bool OnLongitudinalMove => Math.Abs(Velocity.Y) > 0;
        /// <summary>
        /// 指示切片当前是否正在移动.
        /// </summary>
        public bool OnMove => OnHorizontalMove || OnLongitudinalMove;


        /// <summary>
        /// 获取切片当前横向方向的值.
        /// <br><see cref="Direction.Left"/> 和 <see cref="Direction.Right"/>.</br>
        /// </summary>
        public Direction HorizontalDirection { get; set; }
        private Direction _longitudinalDirection;
        /// <summary>
        /// 获取切片当前纵向方向的值.
        /// <br<see cref="Direction.Up"/> 和 <see cref="Direction.Down"/>.</br>
        /// </summary>
        public Direction LongitudinalDirection
        {
            get
            {
                if (Velocity.Y > 0)
                    _longitudinalDirection = Direction.Down;
                else if (Velocity.Y < 0)
                    _longitudinalDirection = Direction.Up;
                return _longitudinalDirection;
            }
        }
        public void DoInitialize() { }
    }
}