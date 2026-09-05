using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Colin.Core.Graphics.Bridge;
using ComputeSharp;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Particle.Compute
{
  using Particle.Core;
  /// <summary>
  /// GPU 零拷贝更新策略 (策略模式): 粒子状态全程驻留显存.
  /// <br>每帧两个 ComputeSharp 派发: 生成 (写入新粒子) 与集成 (生命/运动/曲线外观);
  /// 粒子缓冲区为 MonoGame 与 ComputeSharp 共享的同一块显存 (奇偶双缓冲),
  /// D3D11 渲染端始终读取上一帧结果, 与本帧派发写入的另一缓冲区完全无冲突;
  /// 帧首以 ComputeSharp 完成栅栏同步 (几乎从不等待).</br>
  /// <br>任何初始化失败都会抛出, 由 ParticleManager 捕获并回退 CPU 策略.</br>
  /// </summary>
  public sealed unsafe class GpuUpdateStrategy : IParticleUpdateStrategy
  {
    public string Name => _zeroCopy ? "GPU 零拷贝 (ComputeSharp)" : "GPU 更新+读回 (ComputeSharp)";
    public bool IsZeroCopy => _zeroCopy;
    public int Capacity => _capacity;

    private GraphicsDevice _device;
    private int _capacity;
    private ParticleComputeDevice _compute;
    private SharedParticleBuffer<Particle>[] _parity = new SharedParticleBuffer<Particle>[2];
    private int _writeParity;
    private int _readParity;
    private ulong _lastFenceValue;
    private bool _pending;
    private int _drawEnd;

    // —— 读回模式 (驱动不支持跨设备共享时的 GPU 备选): 粒子更新仍在 GPU, 结果每帧读回上传 ——
    private bool _zeroCopy = true;
    private ReadWriteBuffer<Particle>[] _local = new ReadWriteBuffer<Particle>[2];
    private Particle[] _readback;
    private DynamicVertexBuffer _uploadBuffer;

    // —— 曲线缓冲缓存 (按发射器配置版本增量重建) ——
    private ReadOnlyBuffer<float4>[] _curveBuffers;
    private int[] _curveVersions;
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
        // —— 优先: 跨设备零拷贝共享缓冲区 ——
        for (int i = 0; i < 2; i++)
        {
          _parity[i] = SharedParticleBuffer<Particle>.Create(
              _device, _compute.Device, _compute.D3d12Device, _capacity,
              ParticleLayouts.InstanceVertexDeclaration);

          _compute.Device.For(_capacity, new ParticleClearShader(_parity[i].WriteView));
          _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        }
      }
      catch (Exception sharedFailure)
      {
        // —— 回退: 驱动不支持共享 (如部分 AMD 驱动拒绝缓冲区 NT 共享) → 纯 ComputeSharp 缓冲 + 帧读回.
        Console.WriteLine("Remind", "跨设备共享不可用 (" + sharedFailure.Message + "), GPU 策略切换为更新+读回模式.");
        _zeroCopy = false;
        for (int i = 0; i < 2; i++)
        {
          _parity[i]?.Dispose();
          _parity[i] = null;
          _local[i]?.Dispose();
          _local[i] = _compute.Device.AllocateReadWriteBuffer<Particle>(_capacity);
          _compute.Device.For(_capacity, new ParticleClearShader(_local[i]));
          _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        }
        _readback = new Particle[_capacity];
        _uploadBuffer = new DynamicVertexBuffer(_device, ParticleLayouts.InstanceVertexDeclaration, _capacity, BufferUsage.WriteOnly);
      }
      _pending = true;
      _writeParity = 0;
      _readParity = 1;
      _curveBuffers = new ReadOnlyBuffer<float4>[Math.Max(1, _curveBuffers?.Length ?? 0)];
      _drawEnd = 0;
      Console.WriteLine("Remind", $"GPU 粒子策略就绪 ({SharedParticleBuffer<Particle>.LastCreationMode}).");
    }

    public void Submit(ParticleSimFrame frame)
    {
      // —— 帧首同步: 上一帧派发此时几乎必然已完成 ——
      if (_pending)
        _compute.Fence.Wait(_lastFenceValue);

      // —— 翻转奇偶: 上帧写入端变为本帧读取端 (D3D11 绘制它), 新写入端为另一块 ——
      _readParity = _writeParity;
      _writeParity ^= 1;
      _pending = false;
      _drawEnd = frame.DrawEnd;

      ReadWriteBuffer<Particle> write = _zeroCopy ? _parity[_writeParity].WriteView : _local[_writeParity];
      ReadWriteBuffer<Particle> read = _zeroCopy ? _parity[_readParity].ReadView : _local[_readParity];

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
            parameters.SizeKeyCount));
        _lastFenceValue = _compute.Fence.ReadNextValue(_compute.Device);
        _pending = true;
      }
    }

    public (VertexBuffer Buffer, int InstanceCount) ResolveFrame()
    {
      if (_zeroCopy)
      {
        // 渲染端读取上一帧的写入结果 (本帧派发写入另一奇偶, 无冲突).
        return (_parity[_readParity].InstanceBuffer, Math.Min(_drawEnd, _capacity));
      }

      // 读回模式: 取回上一帧结果并上传动态顶点缓冲 (GPU 同步已在帧首等待完成).
      int count = Math.Min(_drawEnd, _capacity);
      if (count > 0 && _readback is not null)
      {
        _local[_readParity].CopyTo(_readback.AsSpan(0, count));
        _uploadBuffer.SetData(_readback, 0, count, SetDataOptions.Discard);
      }
      return (_uploadBuffer, count);
    }

    /// <summary>获取/重建发射器曲线缓冲 (配置版本号变化时重建).</summary>
    private ReadOnlyBuffer<float4> EnsureCurveBuffer(int emitterIndex, EmitterConfig config)
    {
      if (_curveBuffers.Length <= emitterIndex)
      {
        Array.Resize(ref _curveBuffers, emitterIndex + 1);
      }

      // 版本不足时扩容版本缓存 (新发射器).
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
        _curveScratch.Add(new Vector4(0f, 1f, 1f, 1f));   // 空曲线: 恒定白色 (与 CPU 求值一致)

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

      for (int i = 0; i < _parity.Length; i++)
      {
        _parity[i]?.Dispose();
        _parity[i] = null;
        _local[i]?.Dispose();
        _local[i] = null;
      }
      _uploadBuffer?.Dispose();
      _uploadBuffer = null;
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
