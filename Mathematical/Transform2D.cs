﻿namespace Colin.Core
{
  /// <summary>
  /// 表示一个变换.
  /// </summary>
  public class Transform2D
  {
    /// <summary>
    /// 指示该变换的父级变换, 下文简称为"父元素".
    /// </summary>
    public Transform2D Parent;

    public Vector2 Anchor;

    /// <summary>
    /// 指示该变换的缩放.
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    /// 指示该变换的原点相对于父元素的偏移量.
    /// </summary>
    public Vector2 Translation;

    /// <summary>
    /// 指示该变换的旋转.
    /// </summary>
    public Angle Rotation;

    private Matrix _transform;
    public Matrix Transform
    {
      get
      {
        Calculate();
        return _transform;
      }
    }

    public void Calculate()
    {
      _transform =
        Matrix.CreateScale(Scale.X, Scale.Y, 0f) *
        Matrix.CreateTranslation(-Anchor.X * Scale.X, -Anchor.Y * Scale.Y, 0) *
        Matrix.CreateRotationZ(Rotation.RadiansF) *
        Matrix.CreateTranslation(Anchor.X * Scale.X, Anchor.Y * Scale.Y, 0) *
        Matrix.CreateTranslation(Translation.X, Translation.Y, 0);

      if (Parent is not null)
        _transform *= Parent.Transform;
    }
    public Vector2 GetLocation(Vector2 location)
    {
      Matrix result = Matrix.CreateTranslation(location.X, location.Y, 0);
      result *= Transform;
      return result.Translation.GetVector2();
    }
    public Vector2 GetLocation(int width, int height) => GetLocation(new Vector2(width, height));
  }
}