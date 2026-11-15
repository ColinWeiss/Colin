using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ComputeSharp;
using ComputeSharp.MonoGame;
using Colin.Core.Graphics.Visual.Particle;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Colin.Core.Graphics.Visual.Particle
{
  public sealed class GpuUpdateStrategy : IParticleUpdateStrategy
  {
    public string Name => "GPU 零拷贝 (ComputeSharp.MonoGame)";
    public bool IsZeroCopy => true;
    public int Capacity => _capacity;

    private GraphicsDevice _device;
    private int _capacity;
    private CsBridge _bridge;
    private UnorderedTexture2D[] _parity = Array.Empty<UnorderedTexture2D>();
    private int _writeParity;
    private int _readParity;
    private int _drawEnd;
    private bool _pending;

    private ReadOnlyBuffer<float4>[] _curveBuffers = Array.Empty<ReadOnlyBuffer<float4>>();
    private int[] _curveVersions = Array.Empty<int>();
    private readonly List<Vector4> _curveScratch = new List<Vector4>(64);
    private float4[] _curvePacked = new float4[64];
    private readonly List<ParticleSpawnRecord> _spawnScratch = new List<ParticleSpawnRecord>(128);

    private const int ParityCount = 3;

    public void Initialize(GraphicsDevice device, int capacity)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _capacity = Math.Max(1, capacity);
      _bridge = CoreInfo.Tinter ?? throw new InvalidOperationException("ComputeSharp 桥未初始化");

      _parity = new UnorderedTexture2D[ParityCount];
      for (int i = 0; i < ParityCount; i++)
      {
        _parity[i] = new UnorderedTexture2D(_capacity, ParticleLayouts.DataRows);
        _bridge.Device.For(_capacity, ParticleLayouts.DataRows, new ParticleClearShader(_parity[i].Compute));
      }
      _bridge.Flush();

      _writeParity = 0;
      _readParity = 1;
      _drawEnd = 0;
      _pending = true;
    }

    public void Submit(ParticleSimFrame frame)
    {
      if (_pending)
        _bridge.Flush();

      _readParity = _writeParity;
      _writeParity = (_writeParity + 1) % ParityCount;
      _pending = false;
      _drawEnd = frame.DrawEnd;

      ReadWriteTexture2D<float4> write = _parity[_writeParity].Compute;
      ReadWriteTexture2D<float4> read = _parity[_readParity].Compute;

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
            _bridge.Device.AllocateReadOnlyBuffer(CollectionsMarshal.AsSpan(_spawnScratch));
        _bridge.Device.For(_spawnScratch.Count, new ParticleSpawnShader(write, spawnBuffer, _spawnScratch.Count));
        _pending = true;
      }

      for (int e = 0; e < frame.Emitters.Count; e++)
      {
        EmitterFrame emitter = frame.Emitters[e];
        ParticleSimParams parameters = emitter.Params;
        int capacity = parameters.RangeEnd - parameters.RangeStart;
        if (capacity <= 0)
          continue;

        ReadOnlyBuffer<float4> curveBuffer = EnsureCurveBuffer(e, emitter.Config);

        _bridge.Device.For(capacity, new ParticleIntegrateShader(
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
        _pending = true;
      }
    }

    public (Texture2D DataTexture, int InstanceCount) ResolveFrame()
    {
      return (_parity[_readParity].MonoGame, Math.Min(_drawEnd, _capacity));
    }

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
        _curveScratch.Add(new Vector4(0f, 1f, 1f, 1f));

      if (_curvePacked.Length < _curveScratch.Count)
        _curvePacked = new float4[_curveScratch.Count * 2];
      for (int i = 0; i < _curveScratch.Count; i++)
      {
        Vector4 key = _curveScratch[i];
        _curvePacked[i] = new float4(key.X, key.Y, key.Z, key.W);
      }

      _curveBuffers[emitterIndex]?.Dispose();
      _curveBuffers[emitterIndex] = _bridge.Device.AllocateReadOnlyBuffer(_curvePacked.AsSpan(0, _curveScratch.Count));
      _curveVersions[emitterIndex] = config.Version;
      return _curveBuffers[emitterIndex];
    }

    public void Dispose()
    {
      try
      {
        if (_pending && _bridge is not null)
          _bridge.Flush();
      }
      catch
      {
      }

      for (int i = 0; i < _parity.Length; i++)
      {
        _parity[i]?.Dispose();
        _parity[i] = null;
      }

      for (int i = 0; i < _curveBuffers.Length; i++)
      {
        _curveBuffers[i]?.Dispose();
        _curveBuffers[i] = null;
      }

      _pending = false;
    }
  }
}
