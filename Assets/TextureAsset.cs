using FreeTypeSharp;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace Colin.Core.Assets
{
  public static class TextureAsset
  {
    public static Dictionary<byte[], Texture2D> Textures = new Dictionary<byte[], Texture2D>();
    /// <summary>
    /// 根据 byte[] 获取纹理, 加载过后的纹理则通过缓存直接返回.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Texture2D Get(byte[] bytes)
    {
      Texture2D texture;
      using (MemoryStream ms = new MemoryStream(bytes))
      {
        texture = Texture2D.FromStream(CoreInfo.Graphics.GraphicsDevice, ms);
        return texture;
      }
    }
  }
}