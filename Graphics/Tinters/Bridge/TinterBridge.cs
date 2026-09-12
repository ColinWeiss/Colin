using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ComputeSharp;
using ComputeSharp.Descriptors;
using ComputeSharp.Interop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CsGraphicsDevice = ComputeSharp.GraphicsDevice;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Colin.Core.Graphics.Bridge;

/// <summary>Creates one shader stage of a processing chain: receives the stage's source
/// and destination textures, returns the shader instance to dispatch.</summary>
public delegate TShader ShaderStageFactory<TShader>(
    ReadWriteTexture2D<float4> source,
    ReadWriteTexture2D<float4> target)
    where TShader : struct, IComputeShader, IComputeShaderDescriptor<TShader>;

/// <summary>
/// Bridges MonoGame(D3D11, SharpDX后端)和ComputeSharp(D3D12)通过零拷贝共享GPU纹理:
/// 一系列C#计算着色器可处理任何Texture2D, 其结果可直接由MonoGame渲染——无需像素的CPU往返传输.
/// 用法：
/// <code>
/// var bridge = new TinterBridge(GraphicsDevice); 
/// // 默认情况下 PipelineDepth = 1
/// // 每帧：
/// bridge.BeginFrame();
/// // 将场景渲染到 SurfaceFormat.Vector4 的二维渲染目标中, 然后：
/// Texture2D processed = bridge.Process(
/// sceneRt,
/// (src, dst) => new GrayScaleShader(src, dst, wipeX),
/// (src, dst) => new VignetteShader(src, dst));
/// //(或者, 完全不进行复制：渲染到 bridge.GetInputTarget(w, h) 中, 并调用
/// // bridge.Dispatch(...) 而不是 Process)
/// </code>
/// 工作原理(MonoGame 3.8.5.1 + ComputeSharp 3.2.0, 运行时修改, 无 PR/fork)：
/// 每个管道纹理都在 MonoGame 的 D3D11 设备(Shared | SharedNTHandle)中创建, 
/// 通过原始虚表 ID3D12Device::OpenSharedHandle 在 ComputeSharp D3D12 设备上打开, 
/// 并交换到 ComputeSharp 纹理包装器中.MonoGame 渲染/采样与着色器读/写完全相同的内存.
/// 同步：每个ComputeSharp调度都会向设备的内部完成栅栏发送信号；桥接器会等待该信号(同步模式)或等待前一帧的值(流水线模式：通过帧奇偶性进行双缓冲输入/输出, 延迟一帧, 稳态下无CPU停滞).所有缓冲区都会在分辨率或链长发生变化时重新创建, 因此窗口大小调整会自动处理.
/// 如果手术在启动时失败(例如, 未来的ComputeSharp改变了其内部布局), 那么桥接将降级为CPU回读管道, 虽然速度较慢, 但能正确运行.
/// </summary>
public sealed unsafe class TinterBridge : IDisposable
{
  private readonly GraphicsDevice _mgd;
  private readonly SharpDX.Direct3D11.Device _d3d11;
  private readonly CsGraphicsDevice _csDevice;
  private void* _d3d12Device;              // AddRef'd by InteropServices.GetID3D12Device
  private readonly ComputeFenceSync _sync;

  // zero-copy pipeline buffers
  private SharedTexture[] _inputs = [];    // [parity], 1 or 2 buffers
  private SharedTexture[,] _chain = new SharedTexture[0, 0];   // [stage, parity]
  private readonly HashSet<Texture2D> _uploadedStatic = [];

  // fallback (readback) pipeline
  private RenderTarget2D _fbInput;
  private Texture2D _fbOutput;
  private float4[] _fbPixels;

  private int _width;
  private int _height;
  private int _builtParities = -1;
  private int _builtChainSlots = -1;
  private int _parity;
  private ulong _lastSubmittedValue;
  private bool _hasPending;

  /// <summary>0 = synchronous (Process waits for the GPU, returns the fresh result);
  /// 1 = pipelined (double-buffered, Process returns the previous frame's result, the
  /// CPU never stalls). Default: 1.</summary>
  public int PipelineDepth { get; set; } = 1;

  public bool IsZeroCopy { get; }
  public string Status { get; }

  public TinterBridge(GraphicsDevice graphicsDevice)
  {
    _mgd = graphicsDevice;
    _d3d11 = (SharpDX.Direct3D11.Device)graphicsDevice.Handle;

    if (_d3d11.FeatureLevel < SharpDX.Direct3D.FeatureLevel.Level_11_0)
    {
      throw new NotSupportedException(
          "GraphicsProfile must be HiDef: the default Reach profile creates a FL 9_3 device, " +
          "which can neither sample float textures nor share textures across devices.");
    }

    // pick the ComputeSharp device on the same physical adapter as MonoGame's device
    long adapterLuid;
    using (var dxgiDevice = _d3d11.QueryInterface<SharpDX.DXGI.Device>())
    using (var adapter = dxgiDevice.Adapter)
    {
      adapterLuid = adapter.Description.Luid;
    }

    foreach (var candidate in CsGraphicsDevice.EnumerateDevices())
    {
      if (CsReflection.LuidToInt64(candidate.Luid) == adapterLuid)
      {
        _csDevice = candidate;
      }
      else
      {
        candidate.Dispose();
      }
    }

    _csDevice ??= CsGraphicsDevice.GetDefault();
    _sync = new ComputeFenceSync(_csDevice);

    // validate the raw-vtable machinery, then probe the full surgery with a tiny texture
    string reason = "";
    try
    {
      Guid iid = D3D12Interop.IID_ID3D12Device;
      void* devicePtr = null;
      InteropServices.GetID3D12Device(_csDevice, &iid, &devicePtr);

      if (D3D12Interop.GetDeviceRemovedReason(devicePtr) != 0)
      {
        throw new InvalidOperationException("ID3D12Device::GetDeviceRemovedReason failed");
      }

      _d3d12Device = devicePtr;

      using var probe = SharedTexture.Create(_mgd, _d3d11, _csDevice, _d3d12Device, 4, 4);
      IsZeroCopy = true;
    }
    catch (Exception e)
    {
      reason = $"{e.GetType().Name}: {e.Message}";
      IsZeroCopy = false;
    }

    Status = IsZeroCopy
        ? $"zero-copy | {_csDevice.Name}"
        : $"readback fallback | {_csDevice.Name} | {reason}";
  }

  // ---------------------------------------------------------------- public API: frame flow

  /// <summary>
  /// Call once at the start of each frame (before rendering into inputs or drawing
  /// outputs). In pipelined mode this waits until the previous frame's dispatch has
  /// completed — by then it almost always already has, so the wait is free.
  /// </summary>
  public void BeginFrame()
  {
    if (IsZeroCopy && PipelineDepth > 0 && _hasPending)
    {
      _sync.Wait(_lastSubmittedValue);
    }
  }

  /// <summary>
  /// The zero-copy dynamic input: render your scene directly into this RenderTarget2D
  /// (SurfaceFormat.Vector4), then call <see cref="Dispatch{T}"/>. This avoids even the
  /// single same-device copy that Process performs for RenderTarget2D inputs.
  /// </summary>
  public RenderTarget2D GetInputTarget(int width, int height)
  {
    if (!IsZeroCopy)
    {
      EnsureFallbackInput(width, height);
      return _fbInput;
    }

    EnsureSize(width, height);
    return _inputs[CurrentParity].Wrapper;
  }

  public void Dispose()
  {
    DisposeChain();
    foreach (SharedTexture input in _inputs)
    {
      input.Dispose();
    }
    _inputs = [];

    _fbInput?.Dispose();
    _fbOutput?.Dispose();

    _sync.Dispose();

    if (_d3d12Device != null)
    {
      D3D12Interop.Release(_d3d12Device);   // ours: GetID3D12Device AddRef'd it
      _d3d12Device = null;
    }

    _csDevice.Dispose();
  }

  // ---------------------------------------------------------------- public API: Process (Texture2D in)

  /// <inheritdoc cref="ProcessCore{T}(Texture2D, Action{ComputeSharp.ReadWriteTexture2D{ComputeSharp.float4}[]}, int)"/>
  public Texture2D Process<T>(Texture2D source, ShaderStageFactory<T> stage)
      where T : struct, IComputeShader, IComputeShaderDescriptor<T>
  {
    return ProcessCore(
        source,
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage(chain[0], chain[1])));
  }

  public Texture2D Process<T1, T2>(Texture2D source, ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
  {
    return ProcessCore(
        source,
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])));
  }

  public Texture2D Process<T1, T2, T3>(Texture2D source, ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2, ShaderStageFactory<T3> stage3)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
      where T3 : struct, IComputeShader, IComputeShaderDescriptor<T3>
  {
    return ProcessCore(
        source,
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])),
        chain => _csDevice.For(chain[3].Width, chain[3].Height, stage3(chain[2], chain[3])));
  }

  public Texture2D Process<T1, T2, T3, T4>(Texture2D source, ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2, ShaderStageFactory<T3> stage3, ShaderStageFactory<T4> stage4)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
      where T3 : struct, IComputeShader, IComputeShaderDescriptor<T3>
      where T4 : struct, IComputeShader, IComputeShaderDescriptor<T4>
  {
    return ProcessCore(
        source,
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])),
        chain => _csDevice.For(chain[3].Width, chain[3].Height, stage3(chain[2], chain[3])),
        chain => _csDevice.For(chain[4].Width, chain[4].Height, stage4(chain[3], chain[4])));
  }

  // ---------------------------------------------------------------- public API: Dispatch (dynamic input)

  public Texture2D Dispatch<T>(ShaderStageFactory<T> stage)
      where T : struct, IComputeShader, IComputeShaderDescriptor<T>
  {
    return DispatchCore(
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage(chain[0], chain[1])));
  }

  public Texture2D Dispatch<T1, T2>(ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
  {
    return DispatchCore(
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])));
  }

  public Texture2D Dispatch<T1, T2, T3>(ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2, ShaderStageFactory<T3> stage3)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
      where T3 : struct, IComputeShader, IComputeShaderDescriptor<T3>
  {
    return DispatchCore(
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])),
        chain => _csDevice.For(chain[3].Width, chain[3].Height, stage3(chain[2], chain[3])));
  }

  public Texture2D Dispatch<T1, T2, T3, T4>(ShaderStageFactory<T1> stage1, ShaderStageFactory<T2> stage2, ShaderStageFactory<T3> stage3, ShaderStageFactory<T4> stage4)
      where T1 : struct, IComputeShader, IComputeShaderDescriptor<T1>
      where T2 : struct, IComputeShader, IComputeShaderDescriptor<T2>
      where T3 : struct, IComputeShader, IComputeShaderDescriptor<T3>
      where T4 : struct, IComputeShader, IComputeShaderDescriptor<T4>
  {
    return DispatchCore(
        chain => _csDevice.For(chain[1].Width, chain[1].Height, stage1(chain[0], chain[1])),
        chain => _csDevice.For(chain[2].Width, chain[2].Height, stage2(chain[1], chain[2])),
        chain => _csDevice.For(chain[3].Width, chain[3].Height, stage3(chain[2], chain[3])),
        chain => _csDevice.For(chain[4].Width, chain[4].Height, stage4(chain[3], chain[4])));
  }

  // ---------------------------------------------------------------- core

  /// <summary>
  /// "Texture2D in, shader chain, processed Texture2D out." Plain textures (static
  /// content) are uploaded once and cached; RenderTarget2D inputs (dynamic content) are
  /// GPU-copied into the shared input on every call — the CPU never touches pixel data.
  /// </summary>
  private Texture2D ProcessCore(Texture2D source, params Action<ReadWriteTexture2D<float4>[]>[] stages)
  {
    int stageCount = stages.Length;

    if (!IsZeroCopy)
    {
      return ProcessFallback(source, stages);
    }

    EnsureSize(source.Width, source.Height);
    EnsureChain(source.Width, source.Height, stageCount);

    if (source is RenderTarget2D)
    {
      if (source.Format != SurfaceFormat.Vector4)
      {
        throw new NotSupportedException("dynamic (RenderTarget2D) sources must be SurfaceFormat.Vector4");
      }

      // GPU-side copy into this frame's shared input (same device, zero CPU involvement).
      // SharpDX's CopyResource takes (source, destination) — C#-style, unlike the
      // native (pDst, pSrc) order. The source must not be bound as a render target
      // while being copied (undefined behavior in D3D11 that drivers punish by
      // invalidating its contents), so unbind first — Process hands back a ready
      // texture anyway, so the caller never needs the previous binding.
      _mgd.SetRenderTarget(null);
      _d3d11.ImmediateContext.CopyResource(
          CsReflection.GetNativeTexture(source),
          (SharpDX.Direct3D11.Resource)CsReflection.GetNativeTexture(_inputs[CurrentParity].Wrapper));
    }
    else if (_uploadedStatic.Add(source))
    {
      // static content: upload once into every input buffer
      Vector4[] pixels = GetPixels(source);
      foreach (SharedTexture input in _inputs)
      {
        input.Wrapper.SetData(pixels);
      }
    }

    return DispatchZeroCopy(stages);
  }

  private Texture2D DispatchCore(params Action<ReadWriteTexture2D<float4>[]>[] stages)
  {
    if (!IsZeroCopy)
    {
      if (_fbInput == null)
      {
        throw new InvalidOperationException("call GetInputTarget first");
      }

      return ProcessFallback(_fbInput, stages);
    }

    if (_width == 0)
    {
      throw new InvalidOperationException("no input size known yet — call GetInputTarget or Process first");
    }

    EnsureChain(_width, _height, stages.Length);
    return DispatchZeroCopy(stages);
  }

  private Texture2D DispatchZeroCopy(Action<ReadWriteTexture2D<float4>[]>[] stages)
  {
    int p = CurrentParity;
    int n = stages.Length;

    // textures[0] = this frame's input, textures[k+1] = stage k's output
    var textures = new ReadWriteTexture2D<float4>[n + 1];
    textures[0] = _inputs[p].CsTexture;
    for (int k = 0; k < n; k++)
    {
      textures[k + 1] = _chain[k, p].CsTexture;
    }

    ulong lastValue = 0;
    for (int k = 0; k < n; k++)
    {
      lastValue = _sync.ReadNextValue(_csDevice);
      stages[k](textures);
    }

    if (PipelineDepth > 0 && _hasPending)
    {
      // pipelined: hand out the previous frame's output (guaranteed complete by this
      // frame's BeginFrame wait); this frame's result becomes visible next frame.
      // (p - 1) mod Parities —— 与本帧写入端、上上帧读取端均错开.
      Texture2D result = _chain[n - 1, (p + Parities - 1) % Parities].Wrapper;
      _lastSubmittedValue = lastValue;
      _parity = (_parity + 1) % Parities;
      return result;
    }

    // synchronous (or the very first pipelined frame): wait, return the fresh result
    _sync.Wait(lastValue);

    if (PipelineDepth > 0)
    {
      _hasPending = true;
      _parity ^= 1;
    }

    return _chain[n - 1, p].Wrapper;
  }

  // 三缓冲: D3D12 (dispatch 写) 与 D3D11 (渲染读) 是无隐式同步的独立队列,
  // 双缓冲下 "帧 N 的写" 会撞上 "帧 N-1 的读" (BeginFrame 的栅栏只同步 D3D12 队列).
  // 三缓冲使任意写与读错开一整帧.
  private int Parities => PipelineDepth > 0 ? 3 : 1;

  private int CurrentParity => PipelineDepth > 0 ? _parity : 0;

  private void EnsureSize(int width, int height)
  {
    if (_width == width && _height == height && _builtParities == Parities)
    {
      return;
    }

    foreach (SharedTexture input in _inputs)
    {
      input.Dispose();
    }

    _inputs = new SharedTexture[Parities];
    for (int i = 0; i < _inputs.Length; i++)
    {
      _inputs[i] = SharedTexture.Create(_mgd, _d3d11, _csDevice, _d3d12Device, width, height);
    }

    _width = width;
    _height = height;
    _uploadedStatic.Clear();    // same content, new buffers
    DisposeChain();             // chain buffers must match the new size too
  }

  private void EnsureChain(int width, int height, int stages)
  {
    EnsureSize(width, height);

    if (_builtChainSlots == stages && _builtParities == Parities)
    {
      return;
    }

    DisposeChain();

    _chain = new SharedTexture[stages, Parities];
    for (int k = 0; k < stages; k++)
    {
      for (int p = 0; p < Parities; p++)
      {
        _chain[k, p] = SharedTexture.Create(_mgd, _d3d11, _csDevice, _d3d12Device, width, height);
      }
    }

    _builtChainSlots = stages;
    _builtParities = Parities;
  }

  private void DisposeChain()
  {
    if (_chain.Length > 0)
    {
      foreach (SharedTexture texture in _chain)
      {
        texture.Dispose();
      }
    }

    _chain = new SharedTexture[0, 0];
    _builtChainSlots = -1;
  }

  private static Vector4[] GetPixels(Texture2D source)
  {
    int count = source.Width * source.Height;

    switch (source.Format)
    {
      case SurfaceFormat.Vector4:
        {
          var pixels = new Vector4[count];
          source.GetData(pixels);
          return pixels;
        }

      case SurfaceFormat.Color:
        {
          var colors = new Color[count];
          source.GetData(colors);

          var pixels = new Vector4[count];
          for (int i = 0; i < count; i++)
          {
            pixels[i] = colors[i].ToVector4();
          }

          return pixels;
        }

      default:
        throw new NotSupportedException(
            $"source format {source.Format} is not supported; use SurfaceFormat.Color or SurfaceFormat.Vector4");
    }
  }

  // ---------------------------------------------------------------- fallback (readback) pipeline

  private void EnsureFallbackInput(int width, int height)
  {
    if (_fbInput != null && _fbInput.Width == width && _fbInput.Height == height)
    {
      return;
    }

    _fbInput?.Dispose();
    _fbOutput?.Dispose();
    _fbInput = new RenderTarget2D(_mgd, width, height, false, SurfaceFormat.Vector4, DepthFormat.None);
    _fbOutput = new Texture2D(_mgd, width, height, false, SurfaceFormat.Vector4);
    _fbPixels = new float4[width * height];
  }

  /// <summary>
  /// Degraded mode used when the zero-copy surgery fails at startup: plain ComputeSharp
  /// textures, CPU upload + readback per call. Correct, but this is exactly the cost the
  /// zero-copy path exists to avoid.
  /// </summary>
  private Texture2D ProcessFallback(Texture2D source, Action<ReadWriteTexture2D<float4>[]>[] stages)
  {
    EnsureFallbackInput(source.Width, source.Height);

    var chain = new ReadWriteTexture2D<float4>[stages.Length + 1];

    try
    {
      // input: CPU roundtrip into a plain ComputeSharp texture
      chain[0] = _csDevice.AllocateReadWriteTexture2D(
          MemoryMarshal.Cast<Vector4, float4>(GetPixels(source)).ToArray(),
          source.Width,
          source.Height);

      for (int k = 0; k < stages.Length; k++)
      {
        chain[k + 1] = _csDevice.AllocateReadWriteTexture2D<float4>(source.Width, source.Height);
        stages[k](chain);
      }

      // CopyTo blocks until the dispatch has completed — also the synchronization
      chain[stages.Length].CopyTo(_fbPixels);
      _fbOutput.SetData(MemoryMarshal.Cast<float4, Vector4>(_fbPixels).ToArray());
      return _fbOutput;
    }
    finally
    {
      foreach (var texture in chain)
      {
        texture?.Dispose();
      }
    }
  }
}
