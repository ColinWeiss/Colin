namespace Colin.Core.IO
{
  public class BasicsDirectory
  {
    /// <summary>
    /// 使用 <see cref="Colin"/> 进行开发的程序的目录.
    /// </summary>
    public static string ProgramDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "My Games",
            EngineInfo.EngineName);

    /// <summary>
    /// 指示存档文件夹路径.
    /// </summary>
    public static string ArchiveDir => Path.Combine(ProgramDir, "Archives");

    /// <summary>
    /// 指示数据文件夹路径.
    /// </summary>
    public static string DataDir => Path.Combine(ProgramDir, "Datas");

    /// <summary>
    /// 指示缓存文件夹路径.
    /// </summary>
    public static string CacheDir => Path.Combine(ProgramDir, "Caches");

    /// <summary>
    /// 指示本地化相关内容文件夹路径.
    /// </summary>
    public static string LocalizationDir => Path.Combine(ProgramDir, "Localizations");

  }
}