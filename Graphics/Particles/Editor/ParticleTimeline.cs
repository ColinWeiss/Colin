using ImGuiNET;
using Particle.Core;
// 全局别名将 NV2/NV4 绑定至 XNA 类型, 此处以 NV2/NV4 指代 ImGui 的 System.Numerics 版本.
using NV2 = System.Numerics.Vector2;
using NV4 = System.Numerics.Vector4;
using CurveKey = Particle.Core.CurveKey;

namespace Particle.Editor
{
  /// <summary>
  /// 时间轴与曲线编辑控件 (自绘 ImDrawList 实现, 依赖 ImGuiNET):
  /// <br>- <see cref="RateTimeline"/>: 发射率随效果时间的关键帧时间轴;</br>
  /// <br>- <see cref="FloatCurveEditor"/>: 标量曲线 (透明度/尺寸/速度) 编辑器;</br>
  /// <br>- <see cref="ColorCurveEditor"/>: 颜色曲线 (RGB 三通道) 编辑器;</br>
  /// <br>交互: 双击空白添加关键帧, 拖拽移动, 右键删除, 滚轮无缩放 (轻量实现).</br>
  /// </summary>
  public static class ParticleTimeline
  {
    private static readonly uint BgColor = ImGui.GetColorU32(new NV4(0.08f, 0.08f, 0.1f, 1f));
    private static readonly uint GridColor = ImGui.GetColorU32(new NV4(0.25f, 0.25f, 0.3f, 0.6f));
    private static readonly uint CurveColor = ImGui.GetColorU32(new NV4(0.3f, 0.85f, 1f, 1f));
    private static readonly uint KeyColor = ImGui.GetColorU32(new NV4(1f, 0.85f, 0.25f, 1f));
    private static readonly uint PlayheadColor = ImGui.GetColorU32(new NV4(1f, 0.4f, 0.4f, 0.9f));

    private const float KeyHitRadius = 8f;

    // —— 拖拽中的关键帧状态 (同一时间只允许一个控件交互) ——
    private static string _dragOwner;
    private static int _dragKey = -1;
    private static bool _dragging;

    /// <summary>
    /// 发射率时间轴: 绘制速率倍率曲线与关键帧, 返回交互后是否需要通知配置变更.
    /// </summary>
    /// <param name="emitter">发射器配置 (RateCurve 被编辑).</param>
    /// <param name="duration">效果时长 (秒), 修改后写回.</param>
    /// <param name="effectTime">当前播放时间 (播放头只读显示, 秒).</param>
    /// <param name="onChanged">任何修改后的回调 (观察者通知).</param>
    public static void RateTimeline(string id, EmitterConfig emitter, ref float duration, float effectTime, Action onChanged)
    {
      // 局部函数无法捕获 ref 参数, 统一使用局部副本.
      float dur = duration;
      NV2 size = new NV2(ImGui.GetContentRegionAvail().X, 96f);
      NV2 origin = ImGui.GetCursorScreenPos();
      ImDrawListPtr drawList = ImGui.GetWindowDrawList();

      ImGui.InvisibleButton(id, size);
      bool hovered = ImGui.IsItemHovered();
      bool active = ImGui.IsItemActive();
      NV2 mouse = ImGui.GetIO().MousePos;

      FloatCurve curve = emitter.RateCurve;

      drawList.AddRectFilled(origin, origin + size, BgColor);

      // —— 网格与时间刻度 ——
      int divisions = 8;
      for (int i = 0; i <= divisions; i++)
      {
        float x = origin.X + size.X * i / divisions;
        drawList.AddLine(new NV2(x, origin.Y), new NV2(x, origin.Y + size.Y), GridColor);
        float seconds = dur * i / divisions;
        drawList.AddText(new NV2(x + 2f, origin.Y + 2f), GridColor, $"{seconds:0.00}s");
      }
      for (int i = 0; i <= 4; i++)
      {
        float y = origin.Y + size.Y * i / 4f;
        drawList.AddLine(new NV2(origin.X, y), new NV2(origin.X + size.X, y), GridColor);
      }

      // —— 曲线 ——
      NV2 previous = MapPoint(curve.Keys.Count > 0 ? curve.Keys[0].Time * duration : 0f, curve.Keys.Count > 0 ? curve.Keys[0].Value : 1f);
      for (int step = 1; step <= 64; step++)
      {
        float time = duration * step / 64f;
        float value = curve.Evaluate(time / MathF.Max(duration, 1e-4f));
        NV2 point = MapPoint(time, value);
        drawList.AddLine(previous, point, CurveColor, 1.5f);
        previous = point;
      }

      // —— 播放头 ——
      float playX = origin.X + size.X * Math.Clamp(effectTime / MathF.Max(dur, 1e-4f), 0f, 1f);
      drawList.AddLine(new NV2(playX, origin.Y), new NV2(playX, origin.Y + size.Y), PlayheadColor, 2f);

      // —— 关键帧交互 ——
      List<CurveKey> keys = curve.Keys;
      int hitKey = HitTestKey(keys, key => MapPoint(key.Time * dur, key.Value), mouse, origin, size, hovered);

      if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
      {
        // 双击添加关键帧.
        float time = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f) * dur;
        float value = 1f;
        if (hitKey >= 0)
          value = keys[hitKey].Value;
        curve.AddKey(time / MathF.Max(dur, 1e-4f), value);
        onChanged?.Invoke();
      }
      else if (hitKey >= 0 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !_dragging)
      {
        _dragOwner = id;
        _dragKey = hitKey;
        _dragging = true;
      }
      else if (hitKey < 0 && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && hovered)
      {
        // 右键删除最近的关键帧.
        int nearest = NearestKey(keys, key => MapPoint(key.Time * dur, key.Value), mouse);
        if (nearest >= 0)
        {
          curve.RemoveKeyAt(nearest);
          onChanged?.Invoke();
        }
      }

      if (_dragging && _dragOwner == id && active && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
      {
        if (_dragKey >= 0 && _dragKey < keys.Count)
        {
          CurveKey key = keys[_dragKey];
          float newTime = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f);
          float newValue = Math.Clamp(1f - (mouse.Y - origin.Y) / size.Y, 0f, 2f);
          keys[_dragKey] = new CurveKey(newTime, newValue);
          curve.Version++;
          onChanged?.Invoke();
        }
      }
      if (_dragging && _dragOwner == id && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
      {
        _dragging = false;
        _dragOwner = null;
        _dragKey = -1;
      }

      // —— 关键帧绘制 (最后画, 覆盖曲线) ——
      for (int i = 0; i < keys.Count; i++)
      {
        NV2 p = MapPoint(keys[i].Time * duration, keys[i].Value);
        drawList.AddCircleFilled(p, 4f, i == hitKey ? KeyColor : GridColor);
        drawList.AddCircle(p, 4f, KeyColor);
      }

      // —— 时长调整滑条 ——
      float durationValue = duration;
      if (ImGui.SliderFloat("效果时长 (秒)", ref durationValue, 0.05f, 10f))
      {
        duration = durationValue;
        onChanged?.Invoke();
      }

      NV2 MapPoint(float timeSeconds, float rateValue)
      {
        float x = origin.X + size.X * Math.Clamp(timeSeconds / MathF.Max(dur, 1e-4f), 0f, 1f);
        float y = origin.Y + size.Y * (1f - Math.Clamp(rateValue / 2f, 0f, 1f));
        return new NV2(x, y);
      }
    }

    /// <summary>
    /// 标量曲线编辑器 (横轴 0~1 生命周期, 纵轴 0~1.5 倍率).
    /// </summary>
    public static void FloatCurveEditor(string id, FloatCurve curve, NV2 size, Action onChanged)
    {
      NV2 origin = ImGui.GetCursorScreenPos();
      ImDrawListPtr drawList = ImGui.GetWindowDrawList();

      ImGui.InvisibleButton(id, size);
      bool hovered = ImGui.IsItemHovered();
      bool active = ImGui.IsItemActive();
      NV2 mouse = ImGui.GetIO().MousePos;

      drawList.AddRectFilled(origin, origin + size, BgColor);
      for (int i = 0; i <= 8; i++)
      {
        float x = origin.X + size.X * i / 8f;
        drawList.AddLine(new NV2(x, origin.Y), new NV2(x, origin.Y + size.Y), GridColor);
      }
      drawList.AddLine(new NV2(origin.X, origin.Y + size.Y * 0.5f), new NV2(origin.X + size.X, origin.Y + size.Y * 0.5f), PlayheadColor, 1f);

      // —— 曲线 ——
      NV2 previous = MapPoint(0f, curve.Evaluate(0f));
      for (int step = 1; step <= 64; step++)
      {
        float t = step / 64f;
        NV2 point = MapPoint(t, curve.Evaluate(t));
        drawList.AddLine(previous, point, CurveColor, 1.5f);
        previous = point;
      }

      // —— 关键帧交互 ——
      List<CurveKey> keys = curve.Keys;
      int hitKey = HitTestKey(keys, key => MapPoint(key.Time, key.Value), mouse, origin, size, hovered);

      if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
      {
        float t = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f);
        float value = Math.Clamp(1.5f * (1f - (mouse.Y - origin.Y) / size.Y), 0f, 1.5f);
        curve.AddKey(t, value);
        onChanged?.Invoke();
      }
      else if (hitKey < 0 && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && hovered)
      {
        int nearest = NearestKey(keys, key => MapPoint(key.Time, key.Value), mouse);
        if (nearest >= 0)
        {
          curve.RemoveKeyAt(nearest);
          onChanged?.Invoke();
        }
      }

      if (_dragging && _dragOwner == id && active && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
      {
        if (_dragKey >= 0 && _dragKey < keys.Count)
        {
          float newTime = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f);
          float newValue = Math.Clamp(1.5f * (1f - (mouse.Y - origin.Y) / size.Y), 0f, 1.5f);
          keys[_dragKey] = new CurveKey(newTime, newValue);
          curve.Version++;
          onChanged?.Invoke();
        }
      }
      if (_dragging && _dragOwner == id && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
      {
        _dragging = false;
        _dragOwner = null;
        _dragKey = -1;
      }

      for (int i = 0; i < keys.Count; i++)
      {
        NV2 p = MapPoint(keys[i].Time, keys[i].Value);
        drawList.AddCircleFilled(p, 4f, i == hitKey ? KeyColor : GridColor);
        drawList.AddCircle(p, 4f, KeyColor);
      }

      NV2 MapPoint(float t, float value)
      {
        float x = origin.X + size.X * Math.Clamp(t, 0f, 1f);
        float y = origin.Y + size.Y * (1f - Math.Clamp(value / 1.5f, 0f, 1f));
        return new NV2(x, y);
      }
    }

    /// <summary>
    /// 颜色曲线编辑器: 绘制 RGB 三通道曲线与渐变色带.
    /// </summary>
    public static void ColorCurveEditor(string id, ColorCurve curve, NV2 size, Action onChanged)
    {
      NV2 origin = ImGui.GetCursorScreenPos();
      ImDrawListPtr drawList = ImGui.GetWindowDrawList();

      ImGui.InvisibleButton(id, size);
      bool hovered = ImGui.IsItemHovered();
      bool active = ImGui.IsItemActive();
      NV2 mouse = ImGui.GetIO().MousePos;

      drawList.AddRectFilled(origin, origin + size, BgColor);

      // —— 渐变色带 ——
      for (int x = 0; x < size.X; x++)
      {
        float t = x / size.X;
        Microsoft.Xna.Framework.Vector4 evaluated = curve.Evaluate(t);
        NV4 color = new NV4(evaluated.X, evaluated.Y, evaluated.Z, evaluated.W);
        drawList.AddLine(
          new NV2(origin.X + x, origin.Y + size.Y - 10f),
          new NV2(origin.X + x, origin.Y + size.Y),
          ImGui.GetColorU32(new NV4(color.X, color.Y, color.Z, 1f)));
      }

      // —— 三通道曲线 ——
      for (int channel = 0; channel < 3; channel++)
      {
        uint channelColor = ImGui.GetColorU32(channel switch
        {
          0 => new NV4(1f, 0.35f, 0.35f, 1f),
          1 => new NV4(0.4f, 1f, 0.4f, 1f),
          _ => new NV4(0.4f, 0.5f, 1f, 1f)
        });
        NV2? previous = null;
        for (int step = 0; step <= 64; step++)
        {
          float t = step / 64f;
          Microsoft.Xna.Framework.Vector4 evaluated = curve.Evaluate(t);
          NV4 value = new NV4(evaluated.X, evaluated.Y, evaluated.Z, evaluated.W);
          NV2 point = new NV2(
            origin.X + size.X * t,
            origin.Y + size.Y * (1f - Math.Clamp(GetChannel(value, channel), 0f, 1f)) - 10f);
          if (previous.HasValue)
            drawList.AddLine(previous.Value, point, channelColor, 1.5f);
          previous = point;
        }
      }

      // —— 关键帧交互 (整体移动颜色) ——
      List<ColorKey> keys = curve.Keys;
      int hitKey = HitTestKey(keys, key => new NV2(origin.X + size.X * key.Time, origin.Y + size.Y * 0.5f), mouse, origin, size, hovered);

      if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
      {
        float t = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f);
        Microsoft.Xna.Framework.Vector4 evaluated = curve.Evaluate(t);
        curve.AddKey(t, evaluated);
        onChanged?.Invoke();
      }
      else if (hitKey < 0 && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && hovered)
      {
        int nearest = NearestKey(keys, key => new NV2(origin.X + size.X * key.Time, origin.Y + size.Y * 0.5f), mouse);
        if (nearest >= 0)
        {
          curve.RemoveKeyAt(nearest);
          onChanged?.Invoke();
        }
      }

      if (_dragging && _dragOwner == id && active && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
      {
        if (_dragKey >= 0 && _dragKey < keys.Count)
        {
          ColorKey key = keys[_dragKey];
          float newTime = Math.Clamp((mouse.X - origin.X) / size.X, 0f, 1f);
          float brightness = Math.Clamp(2f * (1f - (mouse.Y - origin.Y) / (size.Y - 10f)), 0.05f, 1.5f);
          NV4 scaled = new NV4(
            Math.Clamp(key.Color.X * brightness / MathF.Max(MathF.Max(key.Color.X, key.Color.Y), key.Color.Z), 0f, 1.5f),
            Math.Clamp(key.Color.Y * brightness / MathF.Max(MathF.Max(key.Color.X, key.Color.Y), key.Color.Z), 0f, 1.5f),
            Math.Clamp(key.Color.Z * brightness / MathF.Max(MathF.Max(key.Color.X, key.Color.Y), key.Color.Z), 0f, 1.5f),
            key.Color.W);
          keys[_dragKey] = new ColorKey(newTime, scaled);
          curve.Version++;
          onChanged?.Invoke();
        }
      }
      if (_dragging && _dragOwner == id && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
      {
        _dragging = false;
        _dragOwner = null;
        _dragKey = -1;
      }

      for (int i = 0; i < keys.Count; i++)
      {
        Microsoft.Xna.Framework.Vector4 color = keys[i].Color;
        NV2 p = new NV2(origin.X + size.X * keys[i].Time, origin.Y + size.Y * 0.5f);
        drawList.AddCircleFilled(p, 4f, i == hitKey ? KeyColor : ImGui.GetColorU32(new NV4(color.X, color.Y, color.Z, 1f)));
        drawList.AddCircle(p, 4f, KeyColor);
      }
    }

    private static float GetChannel(NV4 color, int channel) => channel switch
    {
      0 => color.X,
      1 => color.Y,
      _ => color.Z
    };

    private static int HitTestKey<T>(List<T> keys, Func<T, NV2> map, NV2 mouse, NV2 origin, NV2 size, bool hovered)
    {
      if (!hovered)
        return -1;
      for (int i = 0; i < keys.Count; i++)
      {
        NV2 p = map(keys[i]);
        if (NV2.Distance(p, mouse) <= KeyHitRadius)
          return i;
      }
      return -1;
    }

    private static int NearestKey<T>(List<T> keys, Func<T, NV2> map, NV2 mouse)
    {
      int nearest = -1;
      float best = KeyHitRadius * 3f;
      for (int i = 0; i < keys.Count; i++)
      {
        float distance = NV2.Distance(map(keys[i]), mouse);
        if (distance < best)
        {
          best = distance;
          nearest = i;
        }
      }
      return nearest;
    }
  }
}
