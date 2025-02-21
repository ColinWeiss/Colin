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
    public void Add(string key, Sprite value)
    {
      if (!ContainsKey(key))
      {
        value.Depth = Count / DepthSteps;
        TryAdd(key, value);
      }
    }
    public void Update(GameTime gameTime)
    {
      using (DebugProfiler.Tag("SpritePool"))
      {
        Sprite _sprite;
        for (int count = 0; count < Values.Count; count++)
        {
          _sprite = Values.ElementAt(count);
          if (_sprite.AutoUpdateFrame && _sprite.Frame.FrameMax > 1
              && _sprite.Frame.IsLoop && _sprite.Frame.IsPlay)
            _sprite.Frame.UpdateFrame();
        }
      }
    }
  }
}