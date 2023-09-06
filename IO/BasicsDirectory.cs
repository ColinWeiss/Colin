namespace Colin.Core.IO
{
    public class BasicsDirectory
    {
        /// <summary>
        /// 使用 <see cref="Colin"/> 进行开发的程序的目录.
        /// </summary>
        public static string ProgramDir => 
            Explorer.ConvertPath( 
                Environment.GetFolderPath( Environment.SpecialFolder.Personal ), 
                "My Games", 
                EngineInfo.EngineName );

        /// <summary>
        /// 指示存档文件夹路径.
        /// </summary>
        public static string ArchiveDir => Explorer.ConvertPath( ProgramDir, "Archive" );

        /// <summary>
        /// 指示数据文件夹路径.
        /// </summary>
        public static string DataDir => Explorer.ConvertPath( ProgramDir, "Data" );

    }
}