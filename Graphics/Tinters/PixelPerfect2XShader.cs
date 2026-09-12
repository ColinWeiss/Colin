using ComputeSharp;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// A pixel-perfect magnification pass: every <c>unit × unit</c> block of the target
/// displays the source pixel at the block's top-left corner (nearest-neighbor, no filtering).
/// With <c>unit = 2</c> each on-screen pixel becomes a crisp 2×2 unit block, which keeps
/// hard pixel edges intact (no blurring, no blending across block borders).
/// The dispatch runs at target resolution, so the snapped source coordinate
/// always stays in bounds even when the size is not a multiple of <c>unit</c>.
/// </summary>
[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct PixelPerfect2XShader(
    ReadWriteTexture2D<float4> source,
    ReadWriteTexture2D<float4> target,
    int unit) : IComputeShader
{
  public void Execute()
  {
    // Snap to the top-left pixel of the unit block: nearest-neighbor 2x magnification.
    int2 snapped = (ThreadIds.XY / unit) * unit;
    target[ThreadIds.XY] = source[snapped];
  }
}
