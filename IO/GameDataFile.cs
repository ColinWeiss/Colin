using System.Text;

namespace Colin.Core.IO
{
    /// <summary>
    /// 游戏数据文件包.
    /// <br>[!] 不要误解类名, 它标识 <see cref="GameData"/> 的集合.</br>
    /// </summary>
    public class GameDataFile
    {
        /// <summary>
        /// 指示文件路径.
        /// </summary>
        public string path;

        public virtual string FileHeader { get; }

        /// <summary>
        /// 游戏数据缓冲区.
        /// </summary>
        public GameData[ ] buffer;

        /// <summary>
        /// 游戏数据列表.
        /// <br>键: 具有完整相对资源路径的、含扩展名的文件名称.</br>
        /// <br>值: 游戏数据.</br>
        /// </summary>
        public Dictionary<string, GameData> GameDatas { get; private set; } = new Dictionary<string, GameData>( );

        /// <summary>
        /// 指示该数据文件包的文件流.
        /// </summary>
        public FileStream FileStream { get; private set; }

        /// <summary>
        /// 指示该数据文件包是否已加载完毕.
        /// </summary>
        public bool Loaded { get; private set; } = false;

        public void Load( )
        {
            GameDatas.Clear( );
            buffer = null;
            using( FileStream = File.OpenRead( path ) )
            {
                using( BinaryReader reader = new BinaryReader( FileStream ) )
                {
                    if( Encoding.ASCII.GetString( reader.ReadBytes( FileHeader.Length ) ) != FileHeader )
                        throw new Exception( "Type error." );
                    buffer = new GameData[reader.ReadInt32( )]; //读文件个数, 创建缓冲区.
                    for( int i = 0; i < buffer.Length; i++ )
                    {
                        string fileName = reader.ReadString( ); //读名字
                        int length = reader.ReadInt32( ); //读大小
                        byte[ ] bytes = reader.ReadBytes( length ); //读数据
                        LoadDatas( reader );
                        GameData file = new GameData( fileName, length );
                        buffer[i] = file;
                        GameDatas[file.path] = file;
                    }
                }
            }
        }

        public virtual void LoadDatas( BinaryReader binaryReader ) { }

        /// <summary>
        /// 向包文件内添加文件.
        /// <br>[!!!] 所添加的文件的路径必须位于该数据文件包所在路径或其子级路径下.</br>
        /// </summary>
        /// <param name="filePath">包含文件名在内的路径.</param>
        /// <param name="fileBytes">文件字节.</param>
        public void AddFile( string filePath, byte[ ] fileBytes )
        {
            filePath = ArrangementPath( filePath );
            lock( GameDatas )
                GameDatas[filePath] = new GameData( filePath, fileBytes.Length );
        }

        public void Save( )
        {
            if( FileStream != null )
            {
                throw new IOException( path );
            }
            using( FileStream = File.Create( path ) )
            {
                using BinaryWriter writer = new BinaryWriter( FileStream );
                {
                    writer.Write( Encoding.ASCII.GetBytes( FileHeader ) ); //存文件头.

                    buffer = GameDatas.Values.ToArray( );
                    writer.Write( buffer.Length ); //存文件个数.

                    GameData[ ] array = buffer;
                    foreach( GameData f in array )
                    {
                        writer.Write( f.path );
                        writer.Write( f.length );
                        writer.Write( f.bytes );
                        SaveDatas( writer );
                    } //一个个存文件进去.
                }
            }
        }

        public virtual void SaveDatas( BinaryWriter binaryWriter ) { }

        public GameDataFile( string path )
        {
            this.path = path;
        }

        /// <summary>
        /// 对文件路径进行整理; 将"\\"替换为"/".
        /// </summary>
        public static string ArrangementPath( string path ) => path.Replace( '\\', '/' );

    }
}