using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.IO
{
    public static class Explorer
    {
        public static string ConvertPath( params string[] paths ) 
            => string.Join( Path.PathSeparator, paths.ToArray( ) );

        public static DirectoryInfo[ ] GetDirectoryInfos( string directory )
            => new DirectoryInfo( directory ).GetDirectories( );

        public static FileInfo[ ] GetFileInfos( string directory, string searchPattern )
            => new DirectoryInfo( directory ).GetFiles( searchPattern, SearchOption.AllDirectories );
        
    }
}