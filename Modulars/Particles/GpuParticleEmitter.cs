using Colin.Core.Graphics.Shaders;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Colin.Core.Modulars.Particles
{
  /// <summary>
  /// 粒子发射器.
  /// </summary>
  public class GpuParticleEmitter
  {
    public readonly int ParticleCountMax;
    public GpuParticleEmitter(int particleCountMax) => ParticleCountMax = particleCountMax;

    /// <summary>
    /// 图形设备.
    /// </summary>
    public GraphicsDevice Device => CoreInfo.Graphics.GraphicsDevice;
    /// <summary>
    /// 数据像素模板的顶点缓冲区.
    /// </summary>
    public VertexBuffer TemplateDataVertexBuffer;
    /// <summary>
    /// 数据像素的顶点索引缓冲区.
    /// </summary>
    public IndexBuffer TemplateDataIndexBuffer;
    /// <summary>
    /// 数据像素顶点缓冲区.
    /// </summary>
    public VertexBuffer DataVertexBuffer;
    /// <summary>
    /// 数据绑定.
    /// </summary>
    public VertexBufferBinding[] DataBindings;
    /// <summary>
    /// 用于粒子数据的写入着色器.
    /// </summary>
    public Effect ParticleDataStream;
    /// <summary>
    /// 粒子数据数组缓冲区队列.
    /// <br>用于 <see cref="DataWriteStep"/>.</br>
    /// </summary>
    public Queue<GpuParticleData[]> DataBufferQueue = new Queue<GpuParticleData[]>();

    public VertexBuffer TemplateParticleVertexBuffer;
    public IndexBuffer TemplateParticleIndexBuffer;
    public VertexBuffer ParticleVertexBuffer;
    public VertexBufferBinding[] ParticleBindings;
    public Effect ParticleInstancing;
    public GpuParticleID[] ID;
    /// <summary>
    /// 用于存储粒子信息的渲染目标.
    /// <br>交由 ComputeShader 完成行为计算.</br>
    /// </summary>
    public UnorderedAccessTexture2D DataRt;
    /// <summary>
    /// 用于存储粒子信息的渲染目标.
    /// <br>该 Rt 为经过 ComputeShader 计算后的数据结果.</br>
    /// </summary>
    public UnorderedAccessTexture2D DataResultRt;
    /// <summary>
    /// 指示当前于 <see cref="DataRt"/> 的写入位置.
    /// </summary>
    public int WritePointer = 0;

    /// <summary>
    /// 粒子通用行为逻辑.
    /// <br>在此处进行位置、缩放、旋转的更新.</br>
    /// </summary>
    public ComputeShader ParticleUpdateCompute;

    /// <summary>
    /// 初始化步骤.
    /// <br>在这一步, 将会创建一张 <see cref="RenderTarget2D"/> 用于存储粒子数据.</br>
    /// <br>SurfaceFormat 为 <see cref="SurfaceFormat.Vector4"/>.</br>
    /// <br>渲染目标的宽度为支持的粒子上限大小.</br>
    /// <br>渲染目标的高度为 3.</br>
    /// <para>
    /// 此处为渲染目标的行和列对应的存储数据:
    /// <br>[0] 第一行存储粒子颜色.</br>
    /// <br>[1] R, G 存储粒子位置, B, A 存储粒子速度.</br>
    /// <br>[2] R, G 存储粒子缩放, B 存储粒子旋转角度, A 表示粒子是否活跃.</br>
    /// </para>
    /// </summary>
    public void DoInitialize()
    {
      ParticleDataStream = Asset.GetEffect("Particles/ParticleDataStream");
      ParticleUpdateCompute = Asset.GetComputeShader("ParticleUpdate");
      ParticleInstancing = Asset.GetEffect("Particles/ParticleInstancing");
      CreateDatasBuffer();
      CreateParticlesBuffer();
    }
    private void CreateDatasBuffer()
    {
      DataRt = new UnorderedAccessTexture2D(
      CoreInfo.Graphics.GraphicsDevice,
      ParticleCountMax,
      3,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents);

      DataResultRt = new UnorderedAccessTexture2D(
      CoreInfo.Graphics.GraphicsDevice,
      ParticleCountMax,
      3,
      false,
      SurfaceFormat.Vector4,
      DepthFormat.None,
      0,
      RenderTargetUsage.PreserveContents);

      VertexPosition[] vertices = new VertexPosition[4];
      vertices[0].Position = new Vector3(0, 0, 0);
      vertices[1].Position = new Vector3(0, 4, 0);
      vertices[2].Position = new Vector3(1, 0, 0);
      vertices[3].Position = new Vector3(1, 4, 0);
      TemplateDataVertexBuffer = new VertexBuffer(Device, VertexPosition.VertexDeclaration, 4, BufferUsage.WriteOnly);
      TemplateDataVertexBuffer.SetData(vertices);
      int[] indices = new int[4]
      {
                0, 1, 2, 3
      };
      TemplateDataIndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
      TemplateDataIndexBuffer.SetData(indices);
      DataBindings = new VertexBufferBinding[2];
      DataBindings[0] = new VertexBufferBinding(TemplateDataVertexBuffer);
    }
    private void CreateParticlesBuffer()
    {
      VertexPositionTexture[] vertices = new VertexPositionTexture[4];
      vertices[0].Position = new Vector3(0, 0, 0);
      vertices[0].TextureCoordinate = new Vector2(0, 0);
      vertices[1].Position = new Vector3(0, 28, 0);
      vertices[1].TextureCoordinate = new Vector2(0, 1);
      vertices[2].Position = new Vector3(28, 0, 0);
      vertices[2].TextureCoordinate = new Vector2(1, 0);
      vertices[3].Position = new Vector3(28, 28, 0);
      vertices[3].TextureCoordinate = new Vector2(1, 1);
      TemplateParticleVertexBuffer = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
      TemplateParticleVertexBuffer.SetData(vertices);
      int[] indices = new int[4]
      {
                0, 1, 2, 3
      };
      TemplateParticleIndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
      TemplateParticleIndexBuffer.SetData(indices);
      ParticleBindings = new VertexBufferBinding[2];
      ParticleBindings[0] = new VertexBufferBinding(TemplateParticleVertexBuffer);

      ParticleVertexBuffer = new VertexBuffer(Device, GpuParticleID.VertexDeclaration, ParticleCountMax, BufferUsage.WriteOnly);
      ID = new GpuParticleID[ParticleCountMax];
      for (int count = 0; count < ParticleCountMax; count++)
        ID[count].ID = count;
      ParticleVertexBuffer.SetData(ID);
      ParticleBindings[1] = new VertexBufferBinding(ParticleVertexBuffer, 0, 1);
    }
    public Matrix Transform;

    public void DoRender(SceneCamera camera)
    {
      Perfmon.Start();
      DataWriteStep(camera.Scene);
      DoCompute();
      DoParticleRender(camera);
      Perfmon.End("GpuParticle");
    }
    /// <summary>
    /// 在这一步, 发射器将读取 <see cref="DataBufferQueue"/> 的数据, 并将其全部写入 <see cref="DataRt"/>.
    /// </summary>
    public void DataWriteStep(Scene scene)
    {
      CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(DataRt);
      Transform = Matrix.CreateOrthographicOffCenter(CoreInfo.ViewRectangle, 0, 100);
      while (DataBufferQueue.Count > 0)
      {
        GpuParticleData[] datas = DataBufferQueue.Dequeue();
        CoreInfo.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
        CoreInfo.Batch.DrawRectangle(datas[0].ID, 0, datas.Length, 4, Color.Transparent);
        CoreInfo.Batch.End();
        DataVertexBuffer = new VertexBuffer(Device, GpuParticleData.VertexDeclaration, datas.Length, BufferUsage.WriteOnly);
        DataVertexBuffer.SetData(datas);
        DataBindings[1] = new VertexBufferBinding(DataVertexBuffer, 0, 1);
        Device.SamplerStates[0] = SamplerState.PointClamp;
        Device.RasterizerState = RasterizerState.CullNone;
        Device.BlendState = BlendState.Opaque;
        Device.Indices = TemplateDataIndexBuffer;
        Device.SetVertexBuffers(DataBindings);
        ParticleDataStream.Parameters["Transform"].SetValue(Transform);
        ParticleDataStream.CurrentTechnique.Passes["P0"].Apply();
        Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, datas.Length);
      }
      CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(scene.SceneRenderTarget);
    }
    /// <summary>
    /// 使用 <see cref="ComputeShader"/> 对粒子执行行为和数据的更新.
    /// <br>在这一步, 允许更换的计算着色器用于更新粒子的行为.</br>
    /// </summary>
    public void DoCompute()
    {
      ParticleUpdateCompute.SetTexture(0, DataRt);
      ParticleUpdateCompute.SetUnorderedTexture(0, DataResultRt);
      ParticleUpdateCompute.Dispatch(2048, 1, 1);

      ParticleUpdateCompute.SetTexture(0, null);
      ParticleUpdateCompute.SetUnorderedTexture(0, null);
    }
    /// <summary>
    /// 使用 ParticleInstancing 对粒子进行实例绘制.
    /// </summary>
    public void DoParticleRender(SceneCamera camera)
    {
      Device.SamplerStates[0] = SamplerState.PointClamp;
      Device.RasterizerState = RasterizerState.CullNone;
      Device.BlendState = BlendState.AlphaBlend;
      Device.Indices = TemplateParticleIndexBuffer;
      Device.SetVertexBuffers(ParticleBindings);

      ParticleInstancing.Parameters["ParticleCountMax"].SetValue(ParticleCountMax);
      ParticleInstancing.Parameters["Transform"].SetValue(camera.Transform);
      ParticleInstancing.CurrentTechnique.Passes["P0"].Apply();
      Device.Textures[0] = Sprite.Get("Items/Materials/RollingStone").Source;
      Device.Textures[1] = DataResultRt;
      Device.VertexTextures[1] = DataResultRt;

      Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, ParticleCountMax);

      (DataRt, DataResultRt) = (DataResultRt, DataRt);
    }
    public void NewParticle(GpuParticleData[] datas)
    {
      if (WritePointer + datas.Length > ParticleCountMax)
      {
        WritePointer = 0;
      }
      for (int count = 0; count < datas.Length; count++)
      {
        datas[count].ID = WritePointer + count;
      }
      WritePointer += datas.Length;
      DataBufferQueue.Enqueue(datas);
    }
  }
}