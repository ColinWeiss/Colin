using ComputeSharp;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// A vignette pass (darkens towards the edges), so the demo exercises a multi-stage
/// shader chain: GrayScaleShader -> VignetteShader.
/// </summary>
[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct VignetteShader(
    ReadWriteTexture2D<float4> source,
    ReadWriteTexture2D<float4> target) : IComputeShader
{
  public void Execute()
  {
    float2 uv = new float2(ThreadIds.X, ThreadIds.Y) / new float2(DispatchSize.X, DispatchSize.Y);
    float d = Hlsl.Distance(uv, new float2(0.5f, 0.5f));
    float v = Hlsl.Saturate(1.1f - (d * 1.35f));

    float4 c = source[ThreadIds.XY];
    target[ThreadIds.XY] = new float4(c.RGB * v, c.W);
  }
}
