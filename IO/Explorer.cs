namespace Colin.Core.IO
{
    public static class Explorer
    {
        
        public static DirectoryInfo[] GetDirectoryInfos( string directory )
            => new DirectoryInfo( directory ).GetDirectories();

        public static FileInfo[] GetFileInfos( string directory, string searchPattern )
            => new DirectoryInfo( directory ).GetFiles( searchPattern, SearchOption.AllDirectories );
    }
}