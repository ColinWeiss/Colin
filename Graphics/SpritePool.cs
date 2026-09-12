using Colin.Core.Common.Debugs;
using System.Collections.Concurrent;

namespace Colin.Core.Graphics
{
  /// <summary>
  /// 纹理缓存, 单例.
  /// </summary>
  public class SpritePool : ConcurrentDictionary<string, Sprite>, IGameComponent, IUpdateable
  {
    public event EventHandler<EventArgs> EnabledChanged;
    public event EventHandler<EventArgs> UpdateOrderChanged;
    public bool Enabled { get; } = true;
    public int UpdateOrder { get; }

    private static SpritePool _instance = new SpritePool();
    public static SpritePool Instance => _instance;
    public static float DepthSteps = 10000000f;
    public void Initialize() { }

    /// <summary>
    /// 注册进池子并按注册顺序发放深度号, 深度决定贴图的绘制层级.
    /// <br>返回是否真的注册成功, 已经有同名贴图在池子里就返回 false 不动深度.</br>
    /// </summary>
    public bool Add(string key, Sprite value)
    {
      // 原子占位, 抢到位的才领深度号, 后台线程也会进来注册, 不能用先查后加的两步写法
      if (TryAdd(key, value))
      {
        value.Depth = Count / DepthSteps;
        return true;
      }
      return false;
    }
    public void Update(GameTime gameTime)
    {
      {
        Sprite _sprite;
        for (int count = 0; count < Values.Count; count++)
        {
          _sprite = Values.ElementAt(count);
          if (_sprite.AutoUpdateSharedFrame && _sprite.SharedFrame.FrameMax > 1
              && _sprite.SharedFrame.IsLoop && _sprite.SharedFrame.IsPlay)
            _sprite.SharedFrame.UpdateFrame();
        }
      }
    }
    private SpritePool() { }
  }
}