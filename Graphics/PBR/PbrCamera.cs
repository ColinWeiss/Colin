namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 透视相机. 绕 <see cref="Target"/> 的环绕参数 (Yaw/Pitch/Distance) 供演示/编辑器直接驱动.
  /// </summary>
  public sealed class PbrCamera
  {
    public Vector3 Position = new Vector3(0f, 0f, 4f);

    public Vector3 Target = Vector3.Zero;

    public Vector3 Up = Vector3.Up;

    public float FieldOfView = MathHelper.PiOver4;

    public float NearPlane = 0.03f;

    public float FarPlane = 20000f;

    public float Yaw = 0.7f;

    public float Pitch = 0.35f;

    public float Distance = 5f;

    public Matrix View { get; private set; } = Matrix.Identity;

    public Matrix Projection { get; private set; } = Matrix.Identity;

    public float Aspect { get; private set; } = 1f;

    public void Update(float aspect)
    {
      Aspect = aspect > 0f ? aspect : 1f;
      View = Matrix.CreateLookAt(Position, Target, Up);
      Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, Aspect, NearPlane, FarPlane);
    }

    /// <summary>直接指定透视相机的位置/朝向/投影参数.</summary>
    public void SetPerspective(
        Vector3 position, Vector3 target, Vector3 up, float fieldOfView, float aspect, float nearPlane, float farPlane)
    {
      Position = position;
      Target = target;
      Up = up;
      FieldOfView = fieldOfView;
      Aspect = aspect > 0f ? aspect : 1f;
      NearPlane = nearPlane;
      FarPlane = farPlane;
      View = Matrix.CreateLookAt(position, target, up);
      Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, Aspect, NearPlane, FarPlane);
    }

    /// <summary>外部直接给 view/projection (例如对齐 2D 场景相机).</summary>
    public void SetMatrices(Matrix view, Matrix projection, Vector3 eyePosition)
    {
      View = view;
      Projection = projection;
      Position = eyePosition;
    }

    /// <summary>按当前环绕参数放置相机. 半径以 <see cref="Target"/> 为球心.</summary>
    public void ApplyOrbit()
    {
      float cosPitch = MathF.Cos(Pitch);
      Vector3 offset = new Vector3(
          MathF.Sin(Yaw) * cosPitch,
          MathF.Sin(Pitch),
          MathF.Cos(Yaw) * cosPitch);
      Position = Target + offset * Distance;
    }

    public void Orbit(float yawDelta, float pitchDelta)
    {
      Yaw += yawDelta;
      Pitch = MathHelper.Clamp(Pitch + pitchDelta, -1.45f, 1.45f);
    }

    public void Zoom(float factor)
    {
      Distance = MathHelper.Clamp(Distance * factor, 0.01f, 100000f);
    }

    public void Frame(BoundingSphere sphere, float margin = 1.15f)
    {
      Target = sphere.Center;
      float radius = MathF.Max(sphere.Radius, 0.0001f);
      Distance = radius / MathF.Sin(FieldOfView * 0.5f) * margin;
      ApplyOrbit();
    }
  }
}
