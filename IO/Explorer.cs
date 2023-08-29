using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.IO
{
    public class Explorer
    {
        public static DirectoryInfo[ ] GetDirectoryInfos( string directory )
            => new DirectoryInfo( directory ).GetDirectories( );

        public static FileInfo[ ] GetFileInfos( string directory, string searchPattern )
            => new DirectoryInfo( directory ).GetFiles( searchPattern, SearchOption.AllDirectories );
        
    }
}