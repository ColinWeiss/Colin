
using Colin.Core.IO;

namespace Colin.Core.Assets
{
    /// <summary>
    /// 包含游戏已加载的纹理资产类.
    /// <br>加载模组后即可从此类获取模组内纹理资产.</br>
    /// </summary>
    public class TextureAssets : IGameResource
    {
        public string Name => "纹理资源";

        public float Progress { get; set; }

        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>( );
        public static Dictionary<string, Texture2D> Textures => _textures;

        public void LoadResource( )
        {
            if( !Directory.Exists( string.Concat( EngineInfo.Engine.Content.RootDirectory, "/Textures" ) ) )
                return;
            Texture2D _texture;
            string _fileName;
            string[ ] TextureFileNames = Directory.GetFiles( string.Concat( EngineInfo.Engine.Content.RootDirectory, "/Textures" ), "*.xnb*", SearchOption.AllDirectories );
            for( int count = 0; count < TextureFileNames.Length; count++ )
            {
                Progress = count / TextureFileNames.Length + 1 / TextureFileNames.Length;
                _fileName = IGameResource.ArrangementPath( TextureFileNames[count] );
                _texture = EngineInfo.Engine.Content.Load<Texture2D>( _fileName );
                _textures.Add( _fileName, _texture );
            }
        }

        /// <summary>
        /// 根据路径获取纹理贴图.
        /// <br>[!] 原版纹理资产的获取起始目录为 <![CDATA["Content/Textures"]]>.</br>
        /// <br>若进行模组纹理加载, 则需要从模组主目录开始索引.</br>
        /// </summary>
        /// <param name="path">路径.</param>
        /// <returns>纹理贴图.</returns>
        public static Texture2D Get( string path )
        {
            Texture2D _texture;
            if(_textures.TryGetValue( Explorer.ConvertPath( "Textures" , path ) , out _texture ) )
                return _texture;
            else
            {
                _texture = EngineInfo.Engine.Content.Load<Texture2D>( Explorer.ConvertPath( "Textures", path ) );
                _textures.Add( Explorer.ConvertPath( "Textures", path ), _texture );
                return _texture;
            }
        }
    }
}