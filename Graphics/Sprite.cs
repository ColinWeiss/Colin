namespace Colin.Core.Graphics
{
  using System.Collections.Concurrent;

  /// <summary>
  /// 标识一张Sprite.
  /// <br>两种模式: 传入 <see cref="Texture2D"/> 为快照模式 (程序化纹理, 持有实例);
  /// 传入 Leemo 虚拟路径为路径视图模式 —— <see cref="Source"/> 每次取用都现查 Leemo 缓存,
  /// 热重载换入新实例后自动跟随, 帧格宽高亦随新纹理重算.</br>
  /// </summary>
  public class Sprite
  {
    // 路径视图的实例缓存, 同一张图全局只建一个 Sprite, 逐格 new 的开销就省下来了
    private static readonly ConcurrentDictionary<string, Sprite> _pathViews = new ConcurrentDictionary<string, Sprite>();

    private Texture2D _source;
    private readonly string _virtualPath;      // 非空 = 路径视图模式 (Leemo 管线, 热重载实时跟随).
    private readonly int _frameMax;
    private readonly Direction _direction;
    private readonly bool _sliceFrames;

    /// <summary>
    /// 源纹理. 路径视图模式下每次取用现查 Leemo 缓存 —— 热重载后立取新图.
    /// </summary>
    public Texture2D Source => ResolveSource();

    private Texture2D ResolveSource()
    {
      if (_virtualPath is not null
          && Assets.Manager.TryGet<Texture2D>(_virtualPath, out Texture2D current)
          && ReferenceEquals(current, _source) is false)
      {
        _source = current;
        ApplyFrameSize(current);
      }
      return _source;
    }

    /// <summary>按构造时确定的分帧方式, 依据纹理尺寸重算帧格宽高 (热重载换图后调用).</summary>
    private void ApplyFrameSize(Texture2D texture)
    {
      SharedFrame.Width = texture.Width;
      SharedFrame.Height = texture.Height;
      if (_sliceFrames)
      {
        if (_direction is Direction.Vertical)
          SharedFrame.Height = texture.Height / _frameMax;
        if (_direction is Direction.Horizontal)
          SharedFrame.Width = texture.Width / _frameMax;
      }
    }

    /// <summary>
    /// 大小的一半.
    /// </summary>
    public Vector2 Half => new Vector2(Source.Width / 2, Source.Height / 2);

    /// <summary>
    /// 大小.
    /// </summary>
    public Vector2 SizeF => new Vector2(Source.Width, Source.Height);

    /// <summary>
    /// 大小.
    /// </summary>
    public Point Size => new Point(Source.Width, Source.Height);

    public int Width => Source.Width;

    public int Height => Source.Height;

    /// <summary>
    /// 指示该 Sprite 的共享帧格刷新是否交由自动化运行程序.
    /// <br>路径视图模式下实例是全局共享的, 改这个字段会影响所有用这张图的地方.</br>
    /// </summary>
    public bool AutoUpdateSharedFrame = true;

    /// <summary>
    /// 该纹理内置的帧格选取.
    /// <br>路径视图模式下它是共享状态, 同一张图的所有使用者读写的是同一份.</br>
    /// </summary>
    public Frame SharedFrame;

    /// <summary>
    /// 纹理批绘制参数.
    /// </summary>
    public float Depth { get; internal set; }

    public string Name => Source.Name;

    private void AddThisToGraphicCoreSpritePool()
    {
      // 注册进池子领深度号, 深度按注册顺序发, 决定贴图的绘制层级
      // 后台线程也可能建 Sprite, Add 内部是原子占位, 抢不到就说明同名贴图已注册, 跟它对齐深度
      if (SpritePool.Instance.Add(Source.Name, this) is false
          && SpritePool.Instance.TryGetValue(Source.Name, out Sprite registered))
        Depth = registered.Depth;
    }

    public static void New(Texture2D texture2D)
    {
      new Sprite(texture2D);
    }

    public Sprite(Texture2D texture)
    {
      _source = texture;
      _direction = Direction.Vertical;
      SharedFrame.Direction = Direction.Vertical;
      ApplyFrameSize(texture);
      AddThisToGraphicCoreSpritePool();
    }

    public Sprite(Texture2D texture, int frameMax = 1, bool isLoop = true, bool isPlay = true, Direction direction = Direction.Vertical)
    {
      _source = texture;
      _frameMax = frameMax;
      _direction = direction;
      _sliceFrames = true;
      SharedFrame.IsLoop = isLoop;
      SharedFrame.IsPlay = isPlay;
      SharedFrame.Direction = direction;
      ApplyFrameSize(texture);
      AddThisToGraphicCoreSpritePool();
    }

    /// <summary>
    /// 路径视图模式: 绑定 Leemo 根相对路径 (如 "Textures/Pixel.png").
    /// 构造时未加载则立即经管线加载; 其后 <see cref="Source"/> 随缓存实时更新 (热重载免重建).
    /// </summary>
    public Sprite(string virtualPath)
    {
      _virtualPath = virtualPath;
      _source = Assets.Manager.Load<Texture2D>(virtualPath);
      _direction = Direction.Vertical;
      SharedFrame.Direction = Direction.Vertical;
      ApplyFrameSize(_source);
      AddThisToGraphicCoreSpritePool();
    }

    public static Sprite Get(Texture2D texture)
    {
      if (SpritePool.Instance.TryGetValue(texture.Name, out Sprite sprite))
        return sprite;
      else
        return new Sprite(texture);
    }

    /// <summary>
    /// 按无扩展名的 Textures 相对路径取 Sprite (如 "Gameplays/Items/Backpacks/Backpack");
    /// 返回路径视图 Sprite, 热重载实时跟随.
    /// <br>同一路径全局只建一个实例并缓存, 注意 SharedFrame 是共享状态.</br>
    /// </summary>
    public static Sprite Get(string path)
      => _pathViews.GetOrAdd(
          "Textures/" + path.Replace('\\', '/') + ".png",
          static virtualPath => new Sprite(virtualPath));
    public static Sprite Get(params string[] paths) => Get(Path.Combine(paths));
  }
}
