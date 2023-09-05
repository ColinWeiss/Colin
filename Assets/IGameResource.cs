using Colin.Core.IO;

namespace Colin.Core.Assets
{
    /// <summary>
    /// 标识游戏资源.
    /// </summary>
    public interface IGameResource
    {
        /// <summary>
        /// 指示该游戏资产类对象的名称.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 指示当前加载进度.
        /// </summary>
        public float Progress { get; set; }

        /// <summary>
        /// 加载资源.
        /// </summary>
        public void LoadResource( );

        /// <summary>
        /// 整理路径.
        /// </summary>
        /// <param name="path">路径.</param>
        /// <returns>整理后的路径, 得到不含扩展名和 "Content/" 的资产路径.</returns>
        public static string ArrangementPath( string path )
        {
            string _result = path;
            _result = _result.Replace( ".xnb", "" );
            _result = _result.Replace( ".ttf", "" );
            _result = _result.Replace( ".otf", "" );
            _result = _result.Replace( string.Concat( EngineInfo.Engine.Content.RootDirectory, "/" ), "" );
            _result = _result.Replace( string.Concat( EngineInfo.Engine.Content.RootDirectory, "\\" ), "" );
            return _result;
        }
    }
}