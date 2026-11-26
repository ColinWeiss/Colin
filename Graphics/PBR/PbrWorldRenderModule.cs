namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 游戏世界内的 3D 层: 相机跟随一个 2D 世界锚点 (通常玩家), 用透视投影把 3D 模型挂在锚点附近.
  /// <br>世界坐标换算: 3D = (worldX, -worldY, depth), 相机 up = +Y, 因此模型 local +Y 朝屏幕上方.
  /// <br>该层透明背景, 不画天空盒; 2D 分层背景/地块照常由其它模块绘制.</br>
  /// </summary>
  public class PbrWorldRenderModule : PbrRenderModule
  {
    private readonly List<PbrAttachment> _attachments = new List<PbrAttachment>();
    private Texture2D _environment;

    /// <summary>宿主锚点 (2D 世界像素); 通常在游戏侧绑定本地玩家.</summary>
    public Func<Vector2> AnchorProvider;

    /// <summary>锚点附近的相机偏移 (2D 世界像素); 默认 0 表示相机正对锚点.</summary>
    public Vector2 CameraOffset;

    /// <summary>视场角 (弧度); 决定近大远小的强度.</summary>
    public float FieldOfView = MathHelper.PiOver4;

    /// <summary>相机到锚点的距离. &lt;= 0 时按视口高度自动取 1:1 像素距离.</summary>
    public float CameraDistance;

    public float NearPlane = 1f;

    public float FarPlane = 20000f;

    public IReadOnlyList<PbrAttachment> Attachments => _attachments;

    public override void DoInitialize()
    {
      base.DoInitialize();
      ClearColor = Color.Transparent;
      DrawSkybox = false;
      Renderer.SingleSidedCullMode = CullMode.CullClockwiseFace;
    }

    /// <summary>
    /// 挂一个模型到锚点上. scale 为 "世界像素/模型单位"; 模型底部中心对齐锚点.
    /// </summary>
    public PbrAttachment Attach(
        PbrModel model, Vector2 offset, float depth, float scale, float yaw = 0f, IPbrWorldAnchor anchor = null)
    {
      BoundingBox bounds = model.Bounds;
      Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
      PbrAttachment attachment = new PbrAttachment
      {
        Model = model,
        Anchor = anchor,
        Offset = offset,
        Depth = depth,
        Scale = scale,
        Yaw = yaw,
        Pivot = Matrix.CreateTranslation(-center.X, -bounds.Min.Y, -center.Z) * Matrix.CreateScale(scale)
      };
      _attachments.Add(attachment);
      Add(model);
      return attachment;
    }

    public bool Detach(PbrModel model)
    {
      for (int i = 0; i < _attachments.Count; i++)
      {
        if (ReferenceEquals(_attachments[i].Model, model) is false)
          continue;
        _attachments.RemoveAt(i);
        Remove(model);
        return true;
      }
      return false;
    }

    /// <summary>接入离线烘焙的环境图 (会补 mip); 只用于材质环境项, 不画成背景.</summary>
    public void SetEnvironment(Texture2D source)
    {
      _environment?.Dispose();
      _environment = PbrTextureFactory.CreateMipMapped(CoreInfo.Graphics.GraphicsDevice, source);
      Lighting.EnvironmentTexture = _environment;
    }

    public override void DoUpdate(GameTime time)
    {
      UpdateAttachments();
      base.DoUpdate(time);
    }

    protected override void UpdateCamera(float aspect)
    {
      Vector2 anchor = GetAnchorPosition();
      Vector3 target = new Vector3(anchor.X + CameraOffset.X, -(anchor.Y + CameraOffset.Y), 0f);
      float viewHeight = RawRt?.Height ?? CoreInfo.ViewHeight;
      float distance = CameraDistance > 0f
          ? CameraDistance
          : viewHeight * 0.5f / MathF.Tan(FieldOfView * 0.5f);
      Vector3 eye = target + new Vector3(0f, 0f, MathF.Max(1f, distance));
      Camera.SetPerspective(eye, target, Vector3.Up, FieldOfView, aspect, NearPlane, FarPlane);
    }

    private void UpdateAttachments()
    {
      for (int i = 0; i < _attachments.Count; i++)
      {
        PbrAttachment attachment = _attachments[i];
        Vector2 anchor = attachment.Anchor?.WorldPosition ?? GetAnchorPosition();
        Vector2 world = anchor + attachment.Offset;
        Vector3 position = new Vector3(world.X, -world.Y, attachment.Depth);
        attachment.Model.Transform =
            attachment.Pivot *
            Matrix.CreateRotationY(attachment.Yaw) *
            Matrix.CreateTranslation(position);
      }
    }

    private Vector2 GetAnchorPosition()
    {
      if (AnchorProvider is not null)
        return AnchorProvider();
      return Scene?.Camera?.Position ?? Vector2.Zero;
    }

    public override void Dispose()
    {
      _attachments.Clear();
      _environment?.Dispose();
      _environment = null;
      base.Dispose();
    }
  }
}
