using Colin.Core.Graphics.Shaders;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core
{
    /// <summary>
    /// 抛弃晚餐 MGCB-Editor, 走向美好未来.
    /// </summary>
    public class Asset
    {
        public static string AssetPath = "Assets";

        public static string Platform = "DirectX_11";

        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        private Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();

        private Dictionary<string, ComputeShader> _computeShaders = new Dictionary<string, ComputeShader>();

        private Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();

        private Dictionary<string, Song> _songs = new Dictionary<string, Song>();

        public void LoadAssets()
        {

        }
    }
}