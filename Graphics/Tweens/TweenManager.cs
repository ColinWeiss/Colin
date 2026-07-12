using System.Collections.Generic;

namespace Colin.Core.Graphics.Tweens
{
  public static class TweenManager
  {
    private static readonly List<ITween> _active = new List<ITween>();
    private static readonly List<ITween> _pending = new List<ITween>();

    internal static void Register(ITween tween)
    {
      _pending.Add(tween);
    }

    public static void Update()
    {
      if (_pending.Count > 0)
      {
        _active.AddRange(_pending);
        _pending.Clear();
      }

      for (int i = _active.Count - 1; i >= 0; i--)
      {
        _active[i].Update();
        if (_active[i].IsComplete)
          _active.RemoveAt(i);
      }
    }
  }
}
