namespace Colin.Core.IO
{
    public static class Explorer
    {
        /// <summary>
        /// 获取指定路径的文件夹; 若没有获取到, 则创建.
        /// </summary>
        /// <param name="path"></param>
        public static string GetDirectory(params string[] path)
        {
            var result = Path.Combine(path);
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            return result;
        }

        public static DirectoryInfo[] GetDirectoryInfos(string directory)
            => new DirectoryInfo(directory).GetDirectories();

        public static FileInfo[] GetFileInfos(string directory, string searchPattern)
            => new DirectoryInfo(directory).GetFiles(searchPattern, SearchOption.AllDirectories);
    }
}