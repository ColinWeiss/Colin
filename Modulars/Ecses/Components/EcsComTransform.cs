namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// [默认组件]
  /// <br>提供关于位置/速度/旋转/缩放的字段.</br>
  /// </summary>
  public class EcsComTransform : Transform2D, IEcsComIO
  {
    /// <summary>
    /// 指示实体的大小.
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// 指示实体的速度.
    /// </summary>
    public Vector2 Vel;

    /// <summary>
    /// 指示实体的位置.
    /// </summary>
    public Vector2 Pos
    {
      get => Translation;
      set => Translation = value;
    }

    public Vector2 DeltaVelocity => Vel * Time.DeltaTime;

    /// <summary>
    /// 获取 <see cref="Size"/> 的一半.
    /// </summary>
    public Vector2 Half => Size / 2;

    public RectangleF LocalBound => new RectangleF(Translation - Half, Size);

    public EcsComTransform SetSize(Vector2 size)
    {
      Size = size;
      return this;
    }
    public EcsComTransform SetSize(float width, float height) => SetSize(new Vector2(width, height));

    /// <summary>
    /// 指示实体当前是否正在进行横向移动.
    /// </summary>
    public bool OnHorizontalMove => Math.Abs(Vel.X) > 0;
    /// <summary>
    /// 指示实体当前是否正在进行纵向移动.
    /// </summary>
    public bool OnLongitudinalMove => Math.Abs(Vel.Y) > 0;
    /// <summary>
    /// 指示实体当前是否正在移动.
    /// </summary>
    public bool OnMove => OnHorizontalMove || OnLongitudinalMove;

    private Direction _horizontalDirection;
    /// <summary>
    /// 获取实体当前横向方向的值.
    /// <br><see cref="Direction.Left"/> 和 <see cref="Direction.Right"/>.</br>
    /// </summary>
    public Direction HorizontalDirection
    {
      get
      {
        if (Vel.X > 0)
          _horizontalDirection = Direction.Right;
        if (Vel.X < 0)
          _horizontalDirection = Direction.Left;
        return _horizontalDirection;
      }
    }

    private Direction _longitudinalDirection;
    /// <summary>
    /// 获取实体当前纵向方向的值.
    /// <br><see cref="Direction.Up"/> 和 <see cref="Direction.Down"/>.</br>
    /// </summary>
    public Direction LongitudinalDirection
    {
      get
      {
        if (Vel.Y > 0)
          _longitudinalDirection = Direction.Down;
        else if (Vel.Y < 0)
          _longitudinalDirection = Direction.Up;
        return _longitudinalDirection;
      }
    }
    public void DoInitialize()
    {
      Scale = Vector2.One;
    }

    public void SaveStep(BinaryWriter writer)
    {
      writer.Write(Translation.X);
      writer.Write(Translation.Y);
      writer.Write(Vel.X);
      writer.Write(Vel.Y);
      writer.Write(Size.X);
      writer.Write(Size.Y);
    }

    public void LoadStep(BinaryReader reader)
    {
      Translation.X = reader.ReadSingle();
      Translation.Y = reader.ReadSingle();
      Vel.X = reader.ReadSingle();
      Vel.Y = reader.ReadSingle();
      Size.X = reader.ReadSingle();
      Size.Y = reader.ReadSingle();
    }
  }
}