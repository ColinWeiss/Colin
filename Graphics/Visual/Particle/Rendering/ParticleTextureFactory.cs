using System.Collections.Generic;

namespace Colin.Core.Graphics.Visual.Particle.Rendering
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
          return CreateBladeStreak(device, 512, 128);
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
    /// 刀光条纹理 (512×128, 整张映射到刀光 Mesh 的 UV 0~1):
    /// <br>横向 (u: 0=尾 → 1=头) —— 尾部拖影渐入 → 主体流光带 → 光锋区亮白爆发 → 前端收尖;</br>
    /// <br>纵向 (v) —— 高斯截面 + 边缘羽化; 主体区叠加细微纵向流线, 避免出现"一节一节"的均匀感.</br>
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
          float absV = MathF.Abs(v);

          // —— 纵向: 亮芯 + 宽柔光晕双层截面 ——
          float core = MathF.Exp(-v * v * 26f);        // 中央亮芯.
          float halo = MathF.Exp(-absV * absV * 4.5f); // 外围柔光.
          float cross = core * 0.85f + halo * 0.5f;
          // 边缘羽化 (v 越接近 ±1 越透明).
          cross *= Math.Clamp(1f - absV * absV * absV * 0.6f, 0f, 1f);

          // —— 横向能量分布 (全段连续, 无分界突变): 主体爬升 + 头部高斯光锋叠加 ——
          float body = 0.10f + 0.45f * MathF.Pow(u, 0.8f);
          float flare = 0.75f * MathF.Exp(-MathF.Pow((u - 0.88f) / 0.10f, 2f));   // 头部光锋 (连续高斯峰).
          float tipFade = 1f - 0.55f * Math.Clamp((u - 0.93f) / 0.07f, 0f, 1f);   // 最前端柔化收尖.

          // 细微纵向流线: 若干条随 u 漂移的亮丝 (打破均匀, 制造速度感).
          float streak = 0f;
          for (int s = 0; s < 3; s++)
          {
            float phase = u * (9f + s * 3.7f) + s * 2.1f;
            float line = MathF.Exp(-MathF.Pow(v * 2.2f - MathF.Sin(phase) * 0.55f, 2f) * 30f);
            streak += line * (0.10f - s * 0.025f) * (0.5f + 0.5f * MathF.Sin(phase * 0.7f));
          }

          float intensity = Math.Clamp((body + flare) * cross * tipFade + streak * MathF.Max(0f, cross), 0f, 1f);
          intensity = MathF.Pow(intensity, 1.08f);

          // 冷色调: 亮部白、中调微青 (与顶点色叠加后的整体观感).
          float r = intensity * (0.92f + 0.08f * core);
          float g = intensity;
          float b = intensity * (0.98f - 0.06f * core);
          pixels[y * width + x] = new Color(r, g, b, intensity);
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
