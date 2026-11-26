namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// PBR 前向渲染器: 负责相机/光照 uniform、逐材质状态切换与绘制. 渲染目标由调用方绑定 (PbrRenderModule 或自检).
  /// </summary>
  public sealed class PbrRenderer : IDisposable
  {
    private struct PendingDraw
    {
      public PbrMesh Mesh;
      public Matrix World;
      public float DistanceSquared;
    }

    private static readonly RasterizerState CullBack = new RasterizerState
    {
      CullMode = CullMode.CullClockwiseFace
    };

    private static readonly RasterizerState CullFront = new RasterizerState
    {
      CullMode = CullMode.CullCounterClockwiseFace
    };

    private static readonly RasterizerState CullNone = new RasterizerState
    {
      CullMode = CullMode.None
    };

    private readonly GraphicsDevice _device;
    private readonly PbrEffect _effect;
    private readonly VertexBuffer _skyVertexBuffer;
    private readonly List<PendingDraw> _transparentDraws = new List<PendingDraw>();
    private RasterizerState _previousRasterizer;
    private DepthStencilState _previousDepth;
    private BlendState _previousBlend;

    /// <summary>在 Begin 时先画全屏天空盒背景.</summary>
    public bool DrawSkybox;

    /// <summary>单面材质的剔除方向. 世界内做 Y 反射的 3D 层需要换成 CullCounterClockwiseFace.</summary>
    public CullMode SingleSidedCullMode = CullMode.CullClockwiseFace;

    /// <summary>游戏 2D 光照模式; 为 true 时 PBR 只输出底色+环境反射, 由场景光照后处理上光.</summary>
    public bool GameLighting;

    public Vector3 CameraPosition { get; private set; }

    public PbrEffect Effect => _effect;

    public PbrRenderer(GraphicsDevice device)
    {
      _device = device;
      _effect = new PbrEffect(device);
      // 全屏天空三角形由 SV_VertexID 生成, 顶点缓冲只为满足 MonoGame 的 DrawPrimitives 前置检查.
      _skyVertexBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, 3, BufferUsage.WriteOnly);
      _skyVertexBuffer.SetData(new[]
      {
        new VertexPositionColor(new Vector3(-1f, -1f, 0f), Color.White),
        new VertexPositionColor(new Vector3(3f, -1f, 0f), Color.White),
        new VertexPositionColor(new Vector3(-1f, 3f, 0f), Color.White)
      });
    }

    public void Begin(PbrCamera camera, PbrLighting lighting)
    {
      _previousRasterizer = _device.RasterizerState;
      _previousDepth = _device.DepthStencilState;
      _previousBlend = _device.BlendState;
      _transparentDraws.Clear();
      CameraPosition = camera.Position;

      _effect.SetCamera(camera.View, camera.Projection, camera.Position);
      _effect.SetLighting(lighting);

      _device.RasterizerState = CullBack;
      _device.DepthStencilState = DepthStencilState.Default;
      _device.BlendState = BlendState.Opaque;

      if (DrawSkybox)
        DrawSky();
    }

    private void DrawSky()
    {
      _device.RasterizerState = CullNone;
      _device.DepthStencilState = DepthStencilState.None;
      _device.BlendState = BlendState.Opaque;
      _device.SetVertexBuffer(_skyVertexBuffer);
      _effect.SelectSkybox();
      _effect.ApplyPass();
      _device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
      _device.RasterizerState = CullBack;
      _device.DepthStencilState = DepthStencilState.Default;
    }

    public void DrawModel(PbrModel model)
    {
      model.Draw(this);
    }

    public void DrawModel(PbrModel model, Matrix transform)
    {
      model.Draw(this, transform);
    }

    /// <summary>提交网格: 不透明材质立即绘制, 透明材质按摄像机距离排序后在本帧 End 前绘制.</summary>
    public void SubmitMesh(PbrMesh mesh, Matrix world)
    {
      if (mesh.Material.AlphaMode != PbrAlphaMode.Blend)
      {
        DrawMesh(mesh, world);
        return;
      }

      Vector3 center = (mesh.BindBounds.Min + mesh.BindBounds.Max) * 0.5f;
      Vector3 worldCenter = Vector3.Transform(center, world);
      _transparentDraws.Add(new PendingDraw
      {
        Mesh = mesh,
        World = world,
        DistanceSquared = Vector3.DistanceSquared(worldCenter, CameraPosition)
      });
    }

    public void DrawMesh(PbrMesh mesh, Matrix world)
    {
      PbrMaterial material = mesh.Material;
      _effect.SelectTechnique(mesh.IsSkinned);
      _effect.SetWorld(world);
      _effect.SetGameLighting(GameLighting);
      _effect.BindMaterial(material);
      _effect.BindBones(mesh);

      // glTF/Assimp 的正面是 CCW; XNA 右手系经 Y 翻转投影后在屏幕上是 CW, 因此剔除 CW 面.
      CullMode cull = material.DoubleSided ? CullMode.None : SingleSidedCullMode;
      _device.RasterizerState = cull switch
      {
        CullMode.None => CullNone,
        CullMode.CullCounterClockwiseFace => CullFront,
        _ => CullBack
      };
      if (material.AlphaMode == PbrAlphaMode.Blend)
      {
        _device.BlendState = BlendState.NonPremultiplied;
        _device.DepthStencilState = DepthStencilState.DepthRead;
      }
      else
      {
        _device.BlendState = BlendState.Opaque;
        _device.DepthStencilState = DepthStencilState.Default;
      }

      _effect.ApplyPass();
      _device.SetVertexBuffer(mesh.VertexBuffer);
      _device.Indices = mesh.IndexBuffer;
      _device.DrawIndexedPrimitives(
          PrimitiveType.TriangleList,
          0,
          0,
          mesh.VertexCount,
          0,
          mesh.PrimitiveCount);
    }

    public void End()
    {
      if (_transparentDraws.Count > 0)
      {
        _transparentDraws.Sort(static (left, right) => right.DistanceSquared.CompareTo(left.DistanceSquared));
        for (int i = 0; i < _transparentDraws.Count; i++)
          DrawMesh(_transparentDraws[i].Mesh, _transparentDraws[i].World);
        _transparentDraws.Clear();
      }

      _device.RasterizerState = _previousRasterizer;
      _device.DepthStencilState = _previousDepth;
      _device.BlendState = _previousBlend;
    }

    public void Dispose()
    {
      _effect?.Dispose();
      _skyVertexBuffer?.Dispose();
    }
  }
}
