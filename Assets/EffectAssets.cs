using Colin.Core.IO;

namespace Colin.Core.Assets
{
    public class EffectAssets : IGameAsset
    {
        public string Name => "着色器";

        public float Progress { get; set; }

        public static Dictionary<string, Effect> Effects { get; set; } = new Dictionary<string, Effect>();

        public void LoadResource()
        {
            if(!Directory.Exists( Explorer.PathConcat( EngineInfo.Engine.Content.RootDirectory, "Effects" )))
                return;
            Effect _effect;
            string _fileName;
            string[] _xnbFileNames = Directory.GetFiles( Explorer.PathConcat( EngineInfo.Engine.Content.RootDirectory, "Effects" ), "*.xnb*", SearchOption.AllDirectories );
            for(int count = 0; count < _xnbFileNames.Length; count++)
            {
                Progress = count / _xnbFileNames.Length + 1 / _xnbFileNames.Length;
                _fileName = IGameAsset.ArrangementPath( _xnbFileNames[count] );
                _effect = EngineInfo.Engine.Content.Load<Effect>( _fileName );
                Effects.Add( _fileName, _effect );
            }
        }

        /// <summary>
        /// 根据路径获取着色器.
        /// <br>[!] 起始目录为 <![CDATA["Content/Effects"]]></br>
        /// </summary>
        /// <param name="path">路径.</param>
        /// <returns>着色器.</returns>
        public static Effect Get( string path )
        {
            Effect _texture;
            if(Effects.TryGetValue( Explorer.PathConcat( "Effects", path ), out _texture ))
                return _texture;
            else
            {
                Effects.Add( Explorer.PathConcat( "Effects", path ), _texture );
                _texture = EngineInfo.Engine.Content.Load<Effect>( Explorer.PathConcat( "Effects", path ) );
                return _texture;
            }
        }
    }
}