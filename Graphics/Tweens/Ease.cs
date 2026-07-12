using System;

namespace Colin.Core.Graphics.Tweens
{
  public delegate float EaseFunction(float t);

  public static class Ease
  {
    public static float Linear(float t) => t;

    public static float SineIn(float t) => 1f - MathF.Cos(t * MathF.PI / 2f);
    public static float SineOut(float t) => MathF.Sin(t * MathF.PI / 2f);
    public static float SineInOut(float t) => -(MathF.Cos(MathF.PI * t) - 1f) / 2f;

    public static float QuadIn(float t) => t * t;
    public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);
    public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

    public static float CubicIn(float t) => t * t * t;
    public static float CubicOut(float t) => 1f - MathF.Pow(1f - t, 3f);
    public static float CubicInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

    public static float QuartIn(float t) => t * t * t * t;
    public static float QuartOut(float t) => 1f - MathF.Pow(1f - t, 4f);
    public static float QuartInOut(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f;

    public static float QuintIn(float t) => t * t * t * t * t;
    public static float QuintOut(float t) => 1f - MathF.Pow(1f - t, 5f);
    public static float QuintInOut(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;

    public static float ExpoIn(float t) => t <= 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
    public static float ExpoOut(float t) => t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
    public static float ExpoInOut(float t) => t <= 0f ? 0f : t >= 1f ? 1f : t < 0.5f
      ? MathF.Pow(2f, 20f * t - 10f) / 2f
      : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f;

    public static float CircIn(float t) => 1f - MathF.Sqrt(1f - t * t);
    public static float CircOut(float t) => MathF.Sqrt(1f - (t - 1f) * (t - 1f));
    public static float CircInOut(float t) => t < 0.5f
      ? (1f - MathF.Sqrt(1f - 4f * t * t)) / 2f
      : (MathF.Sqrt(1f - (-2f * t + 2f) * (-2f * t + 2f)) + 1f) / 2f;

    public static float BackIn(float t) => t * t * (2.70158f * t - 1.70158f);
    public static float BackOut(float t) => 1f - (1f - t) * (1f - t) * (-2.70158f * t + 1.70158f + 1f);
    public static float BackInOut(float t) => t < 0.5f
      ? (t * t * ((2.5949095f) * 2f * t - 2.5949095f)) * 2f
      : ((1f - t) * (1f - t) * ((-2.5949095f) * 2f * (1f - t) + 2.5949095f) + 1f) * 2f - 1f;

    public static float ElasticIn(float t)
    {
      if (t <= 0f) return 0f;
      if (t >= 1f) return 1f;
      return -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * (2f * MathF.PI) / 3f);
    }

    public static float ElasticOut(float t)
    {
      if (t <= 0f) return 0f;
      if (t >= 1f) return 1f;
      return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (2f * MathF.PI) / 3f) + 1f;
    }

    public static float ElasticInOut(float t)
    {
      if (t <= 0f) return 0f;
      if (t >= 1f) return 1f;
      if (t < 0.5f)
        return -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI) / 4.5f)) / 2f;
      return (MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * (2f * MathF.PI) / 4.5f)) / 2f + 1f;
    }

    public static float BounceOut(float t)
    {
      if (t < 1f / 2.75f) return 7.5625f * t * t;
      if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
      if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
      t -= 2.625f / 2.75f;
      return 7.5625f * t * t + 0.984375f;
    }

    public static float BounceIn(float t) => 1f - BounceOut(1f - t);
    public static float BounceInOut(float t) => t < 0.5f
      ? (1f - BounceOut(1f - 2f * t)) / 2f
      : (1f + BounceOut(2f * t - 1f)) / 2f;
  }
}
