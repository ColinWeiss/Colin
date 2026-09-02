using ComputeSharp;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// A grayscale filter, written in plain C# and executed on the GPU via ComputeSharp.
/// Pixels left of the wipe line are converted to Rec. 709 luma grayscale,
/// pixels right of it are passed through untouched, so the wipe line makes the
/// per-frame GPU recomputation visibly "live".
/// </summary>
[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct GrayScaleShader(
    ReadWriteTexture2D<float4> source,
    ReadWriteTexture2D<float4> target,
    float wipeX) : IComputeShader
{
  public void Execute()
  {
    float4 color = source[ThreadIds.XY];

    // Rec. 709 luma
    float luma = (color.X * 0.2126f) + (color.Y * 0.7152f) + (color.Z * 0.0722f);

    float4 result = ThreadIds.X < wipeX
        ? new float4(luma, luma, luma, 1f)
        : color;

    target[ThreadIds.XY] = result;
  }
}