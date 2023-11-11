using Colin.Core.IO;
using Microsoft.Xna.Framework.Audio;

namespace Colin.Core.Assets
{
    public class SoundAssets : IGameAsset
    {
        public string Name => "声音资源";

        public float Progress { get; set; }

        public static Dictionary<string, SoundEffect> Sounds { get; set; } = new Dictionary<string, SoundEffect>();

        public void LoadResource()
        {
            if(!Directory.Exists( string.Concat( EngineInfo.Engine.Content.RootDirectory, "/Sounds" ) ))
                return;
            SoundEffect _sound;
            string _fileName;
            string[] TextureFileNames = Directory.GetFiles( string.Concat( EngineInfo.Engine.Content.RootDirectory, "/Sounds" ), "*.xnb*", SearchOption.AllDirectories );
            for(int count = 0; count < TextureFileNames.Length; count++)
            {
                Progress = count / TextureFileNames.Length + 1 / TextureFileNames.Length;
                _fileName = IGameAsset.ArrangementPath( TextureFileNames[count] );
                _sound = EngineInfo.Engine.Content.Load<SoundEffect>( _fileName );
                Sounds.Add( _fileName, _sound );
            }
        }

        /// <summary>
        /// 根据路径获取声音.
        /// <br>[!] 起始目录为 <![CDATA["Sounds"]]></br>
        /// </summary>
        /// <param name="path">路径.</param>
        /// <returns>声音.</returns>
        public static SoundEffect Get( string path )
        {
            SoundEffect _sound;
            if(Sounds.TryGetValue( Path.Combine( "Sounds", path ), out _sound ))
                return _sound;
            else
            {
                _sound = EngineInfo.Engine.Content.Load<SoundEffect>( Path.Combine( "Sounds", path ) );
                Sounds.Add( Path.Combine( "Sounds", path ), _sound );
                return _sound;
            }
        }
    }
}