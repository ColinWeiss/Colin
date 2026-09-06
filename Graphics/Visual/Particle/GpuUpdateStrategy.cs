using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Colin.Core.Graphics.Bridge;
using ComputeSharp;
using Particle.Core;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>
  /// GPU 更新策略 (策略模式, 纹理粒子实现): 粒子状态存于"数据纹理"
  /// (每粒子一列 × 5 行 RGBA32F 纹素, 逐行对应 <see cref="Particle"/> 的 5 个 float4),
  /// 每帧 ComputeSharp 派发生成/集成两个计算着色器, 渲染端顶点着色器按槽位 ID 取样纹素.
  /// <br>两档实现:</br>
  /// <br>- <b>零拷贝</b>: 数据纹理为 D3D11↔D3D12 共享纹理 (复用 TinterBridge 的 SharedTexture 机制,
  /// 该路径已被实战验证), 粒子数据全程驻留显存, CPU 不触碰;</br>
  /// <br>- <b>读回</b>: 驱动不支持共享时, ComputeSharp 侧本地纹理更新后每帧拷回并上传
  /// MonoGame 纹理, 更新仍在 GPU.</br>
  /// <br>奇偶双缓冲 + ComputeSharp 完成栅栏同步: D3D11 渲染端始终读取上一帧结果, 无跨设备写冲突.</br>
  /// </summary>
  public sealed unsafe class GpuUpdateStrategy : IParticleUpdateStrategy
  {
    public string Name => _zeroCopy ? "GPU 零拷贝 (ComputeSharp·共享纹理)" : "GPU 更新+读回 (ComputeSharp)";
    public bool IsZeroCopy => _zeroCopy;
    public int Capacity => _capacity;

    private GraphicsDevice _device;
    private int _capacity;
    private ParticleComputeDevice _compute;
    private bool _zeroCopy = true;

    // —— 零拷贝档: 共享数据纹理 (奇偶) ——
    private SharedTexture[] _sharedParity = new SharedTexture[ParityCount];

    // —— 读回档: ComputeSharp 本地纹理 + MonoGame 采样纹理 ——
    private ReadWriteTexture2D<float4>[] _localParity = new ReadWriteTexture2D<float4>[ParityCount];
    private Texture2D _monoTexture;
    private float4[] _readback;

    private int _writeParity;
    private int _readParity;
    private ulong _lastFenceValue;
    private bool _pending;
    private int _drawEnd;

    /// <summary>
    /// 数据纹理的缓冲深度. 必须为 3: D3D12 (ComputeSharp 写) 与 D3D11 (渲染读) 是两个
    /// 无隐式同步的独立队列, 双缓冲下"帧 N 的 D3D12 写"会撞上"帧 N-1 的 D3D11 读"
    /// (帧首栅栏只同步 D3D12 队列, 管不到 D3D11 的 draw/present) —— 实测表现为
    /// Clear 疑似失效/残影/驱动仲裁导致的风扇狂转. 三缓冲使任意写与读错开一整帧.
    /// </summary>
    private const int ParityCount = 3;

    // —— 曲线缓冲缓存 (按发射器配置版本增量重建) ——
    private ReadOnlyBuffer<float4>[] _curveBuffers = Array.Empty<ReadOnlyBuffer<float4>>();
    private int[] _curveVersions = Array.Empty<int>();
    private readonly List<Vector4> _curveScratch = new List<Vector4>(64);
    private float4[] _curvePacked = new float4[64];
    private readonly List<ParticleSpawnRecord> _spawnScratch = new List<ParticleSpawnRecord>(128);

    /// <summary>预检: 仅创建计算设备 (用于策略选择的失败探测), 不分配缓冲.</summary>
    public void Probe(GraphicsDevice device)
    {
      _compute = ParticleComputeDevice.GetOrCreate(device);
    }

    public void Initialize(GraphicsDevice device, int capacity)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _capacity = Math.Max(1, capacity);
      _compute ??= ParticleComputeDevice.GetOrCreate(device);

      try
      {
        // —— 优先: 跨设备共享数据纹理 (TinterBridge 同款机制, 纹理路径已被实战验证) ——
        for (int i = 0; i < ParityCount; i++)
        {
          _sharedParity[i] = SharedTexture.Create(
              _device, _compute.D3d11, _compute.Device, _compute.D3d12Device, _capacity, ParticleLayouts.DataRows);
          _compute.Device.For(_capacity, ParticleLayouts.DataRows, new ParticleClearShader(_sharedParity[i].CsTexture));
          _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        }
        _zeroCopy = true;
      }
      catch (Exception sharedFailure)
      {
        // —— 回退: ComputeSharp 本地纹理更新 + 帧读回上传 (更新仍在 GPU) ——
        _zeroCopy = false;
        for (int i = 0; i < ParityCount; i++)
        {
          _sharedParity[i]?.Dispose();
          _sharedParity[i] = null;
          _localParity[i]?.Dispose();
          _localParity[i] = _compute.Device.AllocateReadWriteTexture2D<float4>(_capacity, ParticleLayouts.DataRows);
          _compute.Device.For(_capacity, ParticleLayouts.DataRows, new ParticleClearShader(_localParity[i]));
          _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        }
        _readback = new float4[_capacity * ParticleLayouts.DataRows];
        _monoTexture = new Texture2D(_device, _capacity, ParticleLayouts.DataRows, false, SurfaceFormat.Vector4);
      }

      _pending = true;
      _writeParity = 0;
      _readParity = 1;
      _drawEnd = 0;
    }

    public void Submit(ParticleSimFrame frame)
    {
      // —— 帧首同步: 上一帧派发此时几乎必然已完成 ——
      if (_pending)
        _compute.Fence.Wait(_lastFenceValue);

      // —— 翻转: 上帧写入端变为本帧读取端 (D3D11 绘制它), 新写入端为第三块 (与读写双方均错开) ——
      _readParity = _writeParity;
      _writeParity = (_writeParity + 1) % ParityCount;
      _pending = false;
      _drawEnd = frame.DrawEnd;

      ReadWriteTexture2D<float4> write = _zeroCopy ? _sharedParity[_writeParity].CsTexture : _localParity[_writeParity];
      ReadWriteTexture2D<float4> read = _zeroCopy ? _sharedParity[_readParity].CsTexture : _localParity[_readParity];

      // —— 生成派发: 所有发射器的记录合并为一条生成缓冲 ——
      _spawnScratch.Clear();
      for (int e = 0; e < frame.Emitters.Count; e++)
      {
        List<ParticleSpawnRecord> spawns = frame.Emitters[e].Spawns;
        for (int i = 0; i < spawns.Count; i++)
          _spawnScratch.Add(spawns[i]);
      }

      if (_spawnScratch.Count > 0)
      {
        using ReadOnlyBuffer<ParticleSpawnRecord> spawnBuffer =
            _compute.Device.AllocateReadOnlyBuffer(CollectionsMarshal.AsSpan(_spawnScratch));
        _compute.Device.For(_spawnScratch.Count, new ParticleSpawnShader(write, spawnBuffer, _spawnScratch.Count));
        _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        _pending = true;
      }

      // —— 集成派发: 每发射器一次 (曲线参数按发射器独立) ——
      for (int e = 0; e < frame.Emitters.Count; e++)
      {
        EmitterFrame emitter = frame.Emitters[e];
        ParticleSimParams parameters = emitter.Params;
        int capacity = parameters.RangeEnd - parameters.RangeStart;
        if (capacity <= 0)
          continue;

        ReadOnlyBuffer<float4> curveBuffer = EnsureCurveBuffer(e, emitter.Config);

        _compute.Device.For(capacity, new ParticleIntegrateShader(
            write,
            read,
            curveBuffer,
            parameters.Dt,
            parameters.GravityX,
            parameters.GravityY,
            parameters.Drag,
            parameters.RangeStart,
            parameters.ColorKeyCount,
            parameters.AlphaKeyCount,
            parameters.SizeKeyCount,
            parameters.Interpolation));
        _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        _pending = true;
      }
    }

    public (Texture2D DataTexture, int InstanceCount) ResolveFrame()
    {
      if (_zeroCopy)
      {
        // 渲染端读取上一帧的写入结果 (本帧派发写入另一奇偶, 无冲突).
        return (_sharedParity[_readParity].Wrapper, Math.Min(_drawEnd, _capacity));
      }

      // 读回档: 取回上一帧结果并上传 MonoGame 采样纹理 (只传占用前缀列).
      int count = Math.Min(_drawEnd, _capacity);
      if (count > 0 && _readback is not null && _monoTexture is not null)
      {
        _localParity[_readParity].CopyTo(_readback.AsSpan(0, count * ParticleLayouts.DataRows));
        _monoTexture.SetData(0, new Rectangle(0, 0, count, ParticleLayouts.DataRows),
          _readback, 0, count * ParticleLayouts.DataRows);
      }
      return (_monoTexture, count);
    }

    /// <summary>获取/重建发射器曲线缓冲 (配置版本号变化时重建).</summary>
    private ReadOnlyBuffer<float4> EnsureCurveBuffer(int emitterIndex, EmitterConfig config)
    {
      if (_curveBuffers.Length <= emitterIndex)
        Array.Resize(ref _curveBuffers, emitterIndex + 1);

      if (_curveVersions is null || _curveVersions.Length <= emitterIndex)
      {
        int[] old = _curveVersions;
        _curveVersions = new int[emitterIndex + 1];
        if (old is not null)
          Array.Copy(old, _curveVersions, old.Length);
      }

      if (_curveBuffers[emitterIndex] is not null && _curveVersions[emitterIndex] == config.Version)
        return _curveBuffers[emitterIndex];

      config.PackGpuCurves(_curveScratch);
      if (_curveScratch.Count == 0)
        _curveScratch.Add(new Vector4(0f, 1f, 1f, 1f));   // 空曲线: 恒定白色 (与 CPU 求值一致).

      if (_curvePacked.Length < _curveScratch.Count)
        _curvePacked = new float4[_curveScratch.Count * 2];
      for (int i = 0; i < _curveScratch.Count; i++)
      {
        Vector4 key = _curveScratch[i];
        _curvePacked[i] = new float4(key.X, key.Y, key.Z, key.W);
      }

      _curveBuffers[emitterIndex]?.Dispose();
      _curveBuffers[emitterIndex] = _compute.Device.AllocateReadOnlyBuffer(_curvePacked.AsSpan(0, _curveScratch.Count));
      _curveVersions[emitterIndex] = config.Version;
      return _curveBuffers[emitterIndex];
    }

    public void Dispose()
    {
      try
      {
        if (_pending && _compute is not null)
          _compute.Fence.Wait(_lastFenceValue);
      }
      catch
      {
        // 释放路径尽力等待即可.
      }

      for (int i = 0; i < _sharedParity.Length; i++)
      {
        _sharedParity[i]?.Dispose();
        _sharedParity[i] = null;
        _localParity[i]?.Dispose();
        _localParity[i] = null;
      }
      _monoTexture?.Dispose();
      _monoTexture = null;
      _readback = null;

      if (_curveBuffers is not null)
      {
        for (int i = 0; i < _curveBuffers.Length; i++)
        {
          _curveBuffers[i]?.Dispose();
          _curveBuffers[i] = null;
        }
      }

      _pending = false;
    }
  }
}
