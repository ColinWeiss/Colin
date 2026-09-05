using System.Collections.Generic;

namespace Particle.Rendering
{
  /// <summary>
  /// 程序化粒子纹理工厂: 内置若干无需外部资源的默认纹理,
  /// 避免粒子系统对美术资产的硬依赖.
  /// <br>名称解析: 内置名 ("white"/"glow"/"blade"/"spark"/"smoke") 直接生成;
  /// 其余名称转发至 Colin 资产系统 (Asset.GetTexture).</br>
  /// </summary>
  public static class ParticleTextureFactory
  {
    /// <summary>内置纹理名集合.</summary>
    public static readonly HashSet<string> BuiltIn = new HashSet<string> { "white", "glow", "blade", "spark", "smoke" };

    /// <summary>生成内置纹理; 非内置名称返回 null (由渲染器转发资产系统).</summary>
    public static Texture2D Create(GraphicsDevice device, string name)
    {
      switch (name)
      {
        case "white":
          return CreateWhite(device);
        case "glow":
          return CreateRadial(device, 64, hardCore: 0.12f);
        case "spark":
          return CreateRadial(device, 32, hardCore: 0.35f);
        case "blade":
          return CreateBladeStreak(device, 128, 32);
        case "smoke":
          return CreateSmokePuff(device, 64);
        default:
          return Asset.GetTexture(name);
      }
    }

    /// <summary>1×1 白色纹理 (无纹理时的兜底).</summary>
    public static Texture2D CreateWhite(GraphicsDevice device)
    {
      Texture2D texture = new Texture2D(device, 1, 1);
      texture.SetData(new[] { Color.White });
      return texture;
    }

    /// <summary>
    /// 径向发光纹理: 中心亮核向外高斯衰减 (火焰/爆炸/火花).
    /// </summary>
    /// <param name="hardCore">中心硬核半径占比 (0~1), 内部保持全亮.</param>
    public static Texture2D CreateRadial(GraphicsDevice device, int size, float hardCore)
    {
      Color[] pixels = new Color[size * size];
      float center = (size - 1) * 0.5f;

      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          float dx = (x - center) / center;
          float dy = (y - center) / center;
          float distance = MathF.Sqrt(dx * dx + dy * dy);
          float intensity = distance <= hardCore ? 1f : MathF.Exp(-(distance - hardCore) * (distance - hardCore) * 9f);
          intensity = Math.Clamp(intensity, 0f, 1f);
          // 预乘形式: rgb 保存能量, alpha 保存覆盖度 (加法混合下 alpha 影响渐隐).
          pixels[y * size + x] = new Color(intensity, intensity, intensity, intensity);
        }
      }

      Texture2D texture = new Texture2D(device, size, size);
      texture.SetData(pixels);
      return texture;
    }

    /// <summary>
    /// 刀光梭形纹理: 横向 —— 头部 (u=1, 运动前方) 亮而锐利, 尾部 (u=0) 渐隐拉丝;
    /// 纵向 —— 高斯衰减. 速度拉伸公告牌 + 加法混合下呈现连续发光的刀光弧线.
    /// </summary>
    public static Texture2D CreateBladeStreak(GraphicsDevice device, int width, int height)
    {
      Color[] pixels = new Color[width * height];
      float halfHeight = (height - 1) * 0.5f;

      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          float u = x / (width - 1f);
          float v = (y - halfHeight) / halfHeight;

          // 纵向: 高斯截面, 核心亮线.
          float cross = MathF.Exp(-v * v * 7f);

          // 横向: 尾部 (u→0) 快速渐隐, 头部 (u→1) 收成亮锋.
          float head = MathF.Pow(u, 0.65f);
          float edge = MathF.Exp(-(1f - u) * (1f - u) * 3f);
          float intensity = Math.Clamp(cross * (head * 0.85f + edge * 0.4f), 0f, 1f);

          // 中心线提亮, 模拟刃锋高光.
          intensity = MathF.Pow(intensity, 1.2f);

          pixels[y * width + x] = new Color(intensity, intensity, intensity, intensity);
        }
      }

      Texture2D texture = new Texture2D(device, width, height);
      texture.SetData(pixels);
      return texture;
    }

    /// <summary>
    /// 烟雾纹理: 柔和圆斑叠加低成本伪噪声, 边缘破碎感.
    /// </summary>
    public static Texture2D CreateSmokePuff(GraphicsDevice device, int size)
    {
      Color[] pixels = new Color[size * size];
      float center = (size - 1) * 0.5f;

      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          float dx = (x - center) / center;
          float dy = (y - center) / center;
          float distance = MathF.Sqrt(dx * dx + dy * dy);

          // 三层正弦伪噪声制造不规则边缘.
          float noise = 0.5f
            + 0.18f * MathF.Sin(x * 0.42f + y * 0.17f)
            + 0.14f * MathF.Sin(x * 0.13f - y * 0.51f)
            + 0.10f * MathF.Sin((x + y) * 0.29f);

          float intensity = Math.Clamp((1f - distance) * noise * 1.35f, 0f, 1f);
          intensity *= intensity;

          pixels[y * size + x] = new Color(0.75f, 0.75f, 0.78f, intensity);
        }
      }

      Texture2D texture = new Texture2D(device, size, size);
      texture.SetData(pixels);
      return texture;
    }
  }
}
