using System.Text;

namespace Colin.Core.Modulars.UserInterfaces.Design
{
  /// <summary>
  /// 一个可写代码的项目 (共享项目 <c>.projitems</c> 或普通 <c>.csproj</c>).
  /// </summary>
  public sealed class UIDesignProject
  {
    /// <summary>项目文件的绝对路径.</summary>
    public string ProjectFile;

    /// <summary>项目文件所在目录.</summary>
    public string Directory;

    /// <summary>根命名空间 (共享项目取 <c>Import_RootNamespace</c>).</summary>
    public string RootNamespace;

    /// <summary>编译项是否用 <c>$(MSBuildThisFileDirectory)</c> 前缀.</summary>
    public bool UseThisFileDirectory;

    public override string ToString() => string.Concat(RootNamespace, " → ", ProjectFile);
  }

  /// <summary>
  /// 一次导出的落点.
  /// </summary>
  public sealed class UIDesignTarget
  {
    /// <summary>生成文件 (.UI.cs) 的绝对路径.</summary>
    public string FilePath;

    /// <summary>要登记编译项的项目文件; 为空表示无需登记 (文件已存在).</summary>
    public string ProjectFile;

    /// <summary>目标 partial 类的命名空间.</summary>
    public string Namespace;

    /// <summary>目标 partial 类的类名.</summary>
    public string ClassName;

    /// <summary>是不是新建 (新建落 Cache, 由作者自己挪到想放的地方).</summary>
    public bool IsNew;
  }

  /// <summary>
  /// 生成代码往哪儿写: 按<b>命名空间</b>找到对应项目与文件夹; 找不到的 (新建 UI) 统一落到 UI 路径下的 <c>Cache</c>.
  /// <br>规则: 先按类型找已有的 <c>X.cs</c> / <c>X.UI.cs</c>, 找到就写在它旁边 —— 这保证"人在哪, 代码就在哪";</br>
  /// <br>没有才按根命名空间拼文件夹. 于是历史遗留的"命名空间与文件夹不一致"也不会把文件写错地方.</br>
  /// <br>[!] 找文件时<b>同时核对文件里的 namespace</b>: 不同命名空间里的同名类各有一份 <c>X.UI.cs</c>,
  /// 只按类名找会覆盖到别人头上.</br>
  /// </summary>
  public static class UIDesignPaths
  {
    private static readonly List<UIDesignProject> _projects = new List<UIDesignProject>();
    private static readonly HashSet<string> _skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
      "bin", "obj", ".git", ".vs", ".idea", "node_modules", "Temp", "Logs", "Saved",
    };

    /// <summary>解决方案目录; 未发现时为 null.</summary>
    public static string SolutionDirectory { get; private set; }

    /// <summary>新建 UI 的暂存目录 (UI 路径下的 Cache).</summary>
    public static string CacheDirectory { get; private set; }

    /// <summary>已发现的项目.</summary>
    public static IReadOnlyList<UIDesignProject> Projects => _projects;

    private static bool _discovered;

    /// <summary>文件名 (小写) → 绝对路径列表; 首次查找时建一次, 之后 O(1), 不再逐节点扫盘.</summary>
    private static readonly Dictionary<string, List<string>> _fileIndex = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    private static int _indexedProjectCount = -1;

    /// <summary>
    /// 扫描解决方案目录, 建立"根命名空间 → 项目"的映射. 未找到解决方案时返回 false.
    /// </summary>
    public static bool Discover(string solutionDirectory = null)
    {
      _projects.Clear();
      InvalidateIndex();
      if (string.IsNullOrEmpty(solutionDirectory))
        solutionDirectory = FindSolutionDirectory();
      SolutionDirectory = solutionDirectory;
      _discovered = true;
      if (string.IsNullOrEmpty(solutionDirectory) || System.IO.Directory.Exists(solutionDirectory) is false)
        return false;
      foreach (string file in EnumerateProjectFiles(solutionDirectory))
        AddProject(file);
      CacheDirectory = ResolveCacheDirectory(solutionDirectory);
      return _projects.Count > 0;
    }

    /// <summary>丢弃文件索引 (写盘后调用, 让下一次查找看到新文件).</summary>
    public static void InvalidateIndex()
    {
      _fileIndex.Clear();
      _indexedProjectCount = -1;
    }

    /// <summary>
    /// 从程序所在目录往上找 <c>*.sln</c>; 找不到返回 null (发布版的游戏里本来就没有解决方案).
    /// </summary>
    public static string FindSolutionDirectory()
    {
      DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
      for (int depth = 0; directory is not null && depth < 8; depth++)
      {
        try
        {
          if (directory.GetFiles("*.sln").Length > 0)
            return directory.FullName;
        }
        catch (UnauthorizedAccessException)
        {
          return null;
        }
        directory = directory.Parent;
      }
      return null;
    }

    /// <summary>
    /// 为已存在的类型找落点; 找不到合适位置时退回 <see cref="CacheDirectory"/>.
    /// </summary>
    public static UIDesignTarget Resolve(Type type, out string reason)
    {
      reason = null;
      if (type is null)
      {
        reason = "类型为空";
        return null;
      }
      EnsureDiscovered();
      string className = type.Name;
      string @namespace = type.Namespace ?? string.Empty;

      //1) 已经导出过: 同名 .UI.cs 就在那儿 (核对命名空间, 避免同名类互相覆盖)
      if (TryFindSourceFile(string.Concat(className, ".UI.cs"), @namespace, out string existing))
        return new UIDesignTarget
        {
          FilePath = existing,
          ProjectFile = FindProjectFile(existing)?.ProjectFile,
          Namespace = @namespace,
          ClassName = className,
          IsNew = false,
        };

      //2) 手写文件在旁边: 生成文件放同目录 (命名空间与文件夹不一致也照样对)
      if (TryFindSourceFile(string.Concat(className, ".cs"), @namespace, out string handWritten))
      {
        string file = Path.Combine(Path.GetDirectoryName(handWritten) ?? string.Empty, string.Concat(className, ".UI.cs"));
        return new UIDesignTarget
        {
          FilePath = file,
          ProjectFile = FindProjectFile(handWritten)?.ProjectFile,
          Namespace = @namespace,
          ClassName = className,
          IsNew = false,
        };
      }

      //3) 按根命名空间拼文件夹
      UIDesignProject project = MatchProject(@namespace);
      if (project is not null)
      {
        string relative = @namespace.Length > project.RootNamespace.Length
          ? @namespace.Substring(project.RootNamespace.Length).TrimStart('.')
          : string.Empty;
        string folder = string.IsNullOrEmpty(relative)
          ? project.Directory
          : Path.Combine(project.Directory, relative.Replace('.', Path.DirectorySeparatorChar));
        if (System.IO.Directory.Exists(folder))
          return new UIDesignTarget
          {
            FilePath = Path.Combine(folder, string.Concat(className, ".UI.cs")),
            ProjectFile = project.ProjectFile,
            Namespace = @namespace,
            ClassName = className,
            IsNew = false,
          };
        reason = string.Concat("命名空间对应的文件夹不存在: ", folder);
      }

      //4) 兜底: Cache, 由作者自己挪
      if (string.IsNullOrEmpty(CacheDirectory) is false)
      {
        reason ??= "没有匹配的项目";
        return new UIDesignTarget
        {
          FilePath = Path.Combine(CacheDirectory, string.Concat(className, ".UI.cs")),
          ProjectFile = MatchProject(@namespace)?.ProjectFile ?? _projects.FirstOrDefault()?.ProjectFile,
          Namespace = @namespace,
          ClassName = className,
          IsNew = true,
        };
      }
      reason ??= "未发现解决方案目录";
      return null;
    }

    /// <summary>
    /// 为"一个 builder 静态类"找落点: 先找同名 <c>XxxUI.UI.cs</c>, 再找同名手写文件旁边, 最后落 Cache.
    /// <br>命名空间: 复用已有文件的 namespace; 没有就落到 Cache 的命名空间.</br>
    /// </summary>
    public static UIDesignTarget ResolveBuilder(string className, out string reason)
    {
      reason = null;
      EnsureDiscovered();
      if (string.IsNullOrEmpty(className))
      {
        reason = "类名为空";
        return null;
      }
      if (TryFindAnyFile(string.Concat(className, ".UI.cs"), out string existing))
        return new UIDesignTarget
        {
          FilePath = existing,
          ProjectFile = FindProjectFile(existing)?.ProjectFile,
          Namespace = ReadNamespace(existing) ?? CacheNamespace,
          ClassName = className,
          IsNew = false,
        };
      if (TryFindAnyFile(string.Concat(className, ".cs"), out string handWritten))
      {
        string file = Path.Combine(Path.GetDirectoryName(handWritten) ?? string.Empty, string.Concat(className, ".UI.cs"));
        return new UIDesignTarget
        {
          FilePath = file,
          ProjectFile = FindProjectFile(handWritten)?.ProjectFile,
          Namespace = ReadNamespace(handWritten) ?? CacheNamespace,
          ClassName = className,
          IsNew = false,
        };
      }
      if (string.IsNullOrEmpty(CacheDirectory) is false)
      {
        return new UIDesignTarget
        {
          FilePath = Path.Combine(CacheDirectory, string.Concat(className, ".UI.cs")),
          ProjectFile = _projects.FirstOrDefault()?.ProjectFile,
          Namespace = CacheNamespace,
          ClassName = className,
          IsNew = true,
        };
      }
      reason = "未发现解决方案目录, 也没有同名文件可定位";
      return null;
    }

    /// <summary>新建 builder 的默认命名空间 (Cache 目录归属).</summary>
    public const string CacheNamespace = "Colin.Core.Modulars.UserInterfaces.Cache";

    /// <summary>读文件里声明的命名空间 (前 40 行); 没有返回 null.</summary>
    public static string ReadNamespace(string file)
    {
      try
      {
        using StreamReader reader = new StreamReader(file);
        for (int line = 0; line < 40; line++)
        {
          string text = reader.ReadLine();
          if (text is null)
            break;
          int at = text.IndexOf("namespace ", StringComparison.Ordinal);
          if (at < 0)
            continue;
          string rest = text.Substring(at + "namespace ".Length).Trim();
          int end = 0;
          while (end < rest.Length && (char.IsLetterOrDigit(rest[end]) || rest[end] == '.' || rest[end] == '_'))
            end++;
          return end == 0 ? null : rest.Substring(0, end);
        }
      }
      catch (IOException)
      {
        return null;
      }
      return null;
    }

    /// <summary>按文件名找任意源码文件 (不核对命名空间); 同名多份时取第一个.</summary>
    public static bool TryFindAnyFile(string fileName, out string path)
    {
      path = null;
      EnsureDiscovered();
      List<string> candidates = FindCandidates(fileName);
      if (candidates.Count == 0)
        return false;
      path = candidates[0];
      return true;
    }

    /// <summary>为一个还不存在的类准备落点 (新建 UI → Cache).</summary>
    public static UIDesignTarget ResolveNew(string @namespace, string className)
    {
      EnsureDiscovered();
      if (string.IsNullOrEmpty(className))
        return null;
      UIDesignProject project = MatchProject(@namespace ?? string.Empty) ?? _projects.FirstOrDefault();
      string folder = string.IsNullOrEmpty(CacheDirectory) is false
        ? CacheDirectory
        : (project?.Directory ?? System.IO.Directory.GetCurrentDirectory());
      return new UIDesignTarget
      {
        FilePath = Path.Combine(folder, string.Concat(className, ".UI.cs")),
        ProjectFile = project?.ProjectFile,
        Namespace = @namespace ?? project?.RootNamespace ?? string.Empty,
        ClassName = className,
        IsNew = true,
      };
    }

    /// <summary>
    /// 把文件写进项目: 目录建好, 内容落盘, 并保证项目里有对应的编译项.
    /// <br>已存在的编译项不动 —— 编辑器里挪过文件的话, 那里已经是对的.</br>
    /// </summary>
    public static bool TryWrite(UIDesignTarget target, string content, out string error)
    {
      error = null;
      if (target is null || string.IsNullOrEmpty(target.FilePath))
      {
        error = "落点为空";
        return false;
      }
      try
      {
        string folder = Path.GetDirectoryName(target.FilePath);
        if (string.IsNullOrEmpty(folder) is false)
          System.IO.Directory.CreateDirectory(folder);
        File.WriteAllText(target.FilePath, content, new UTF8Encoding(false));
        target.IsNew = false;
        InvalidateIndex();
      }
      catch (Exception exception)
      {
        error = string.Concat("写文件失败 (文件未写入): ", exception.Message);
        return false;
      }
      if (string.IsNullOrEmpty(target.ProjectFile) is false)
      {
        if (TryRegisterCompileItem(FindProjectFile(target.ProjectFile), target.FilePath, out string registerError) is false)
        {
          error = string.Concat("文件已写入, 但登记编译项失败: ", registerError);
          return false;
        }
      }
      return true;
    }

    /// <summary>确保项目里有该文件的编译项; 已存在 (或工程本身就是隐式包含) 返回 true 且不改动.</summary>
    public static bool TryRegisterCompileItem(UIDesignProject project, string filePath, out string error)
    {
      error = null;
      if (project is null || string.IsNullOrEmpty(project.ProjectFile))
        return true;
      if (string.IsNullOrEmpty(filePath) || File.Exists(filePath) is false)
      {
        error = string.Concat("文件不存在: ", filePath);
        return false;
      }
      if (IsUtf16(project.ProjectFile))
      {
        error = "项目文件是 UTF-16 编码, 本工具不修改 (请手工加编译项)";
        return false;
      }
      string relative = Path.GetRelativePath(project.Directory, filePath);
      string fragment = relative.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      try
      {
        bool bom = HasByteOrderMark(project.ProjectFile);
        string text = File.ReadAllText(project.ProjectFile);
        if (text.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0
          || text.IndexOf(fragment.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase) >= 0)
          return true;
        string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        int insertAt = -1;
        string indent = "    ";
        for (int index = 0; index < lines.Length; index++)
        {
          if (lines[index].Contains("<Compile Include=", StringComparison.Ordinal))
          {
            insertAt = index;
            indent = lines[index].Substring(0, lines[index].Length - lines[index].TrimStart().Length);
          }
        }
        if (insertAt < 0)
        {
          //SDK 风格工程默认隐式包含所有 .cs: 没有 <Compile Include=...> 是正常的, 不该报错.
          if (ImplicitlyCompiles(project.ProjectFile, text))
            return true;
          error = "项目文件里既没有 <Compile Include=...> 锚点, 又显式关闭了默认编译项 (EnableDefaultCompileItems=false)";
          return false;
        }
        string prefix = project.UseThisFileDirectory ? "$(MSBuildThisFileDirectory)" : string.Empty;
        string line = string.Concat(indent, "<Compile Include=\"", prefix, fragment, "\" />");
        List<string> output = new List<string>(lines.Length + 1);
        for (int index = 0; index < lines.Length; index++)
        {
          output.Add(lines[index]);
          if (index == insertAt)
            output.Add(line);
        }
        File.WriteAllText(project.ProjectFile, string.Join(newline, output), new UTF8Encoding(bom));
        return true;
      }
      catch (Exception exception)
      {
        error = string.Concat("登记编译项失败: ", exception.Message);
        return false;
      }
    }

    // —— 内部 ——

    /// <summary>SDK 风格工程的默认编译项是否生效 (没被 <c>EnableDefaultCompileItems=false</c> 关掉).</summary>
    private static bool ImplicitlyCompiles(string projectFile, string text)
    {
      bool disabled = text.IndexOf("<EnableDefaultCompileItems>false</EnableDefaultCompileItems>", StringComparison.OrdinalIgnoreCase) >= 0
        || text.IndexOf("<EnableDefaultCompileItems> false </EnableDefaultCompileItems>", StringComparison.OrdinalIgnoreCase) >= 0;
      if (disabled)
        return false;
      //老式工程 (.projitems / 非 SDK) 没有隐式包含; SDK 风格才有 <Project Sdk=.
      return text.IndexOf("<Project Sdk=", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsUtf16(string file)
    {
      try
      {
        using FileStream stream = File.OpenRead(file);
        int first = stream.ReadByte();
        int second = stream.ReadByte();
        return first == 0xFF && second == 0xFE || first == 0xFE && second == 0xFF;
      }
      catch (IOException)
      {
        return false;
      }
    }

    private static void EnsureDiscovered()
    {
      if (_discovered is false)
        Discover();
    }

    private static string ResolveCacheDirectory(string solutionDirectory)
    {
      foreach (UIDesignProject project in _projects)
      {
        string ui = Path.Combine(project.Directory, "Modulars", "UserInterfaces");
        if (System.IO.Directory.Exists(ui))
          return Path.Combine(ui, "Cache");
        string gameUi = Path.Combine(project.Directory, "GameContents", "UserInterfaces");
        if (System.IO.Directory.Exists(gameUi))
          return Path.Combine(gameUi, "Cache");
      }
      return Path.Combine(solutionDirectory, "UI", "Cache");
    }

    private static IEnumerable<string> EnumerateProjectFiles(string root)
    {
      Stack<string> pending = new Stack<string>();
      pending.Push(root);
      while (pending.Count > 0)
      {
        string current = pending.Pop();
        List<string> files = new List<string>();
        string[] folders;
        try
        {
          files.AddRange(System.IO.Directory.GetFiles(current, "*.csproj"));
          files.AddRange(System.IO.Directory.GetFiles(current, "*.projitems"));
          folders = System.IO.Directory.GetDirectories(current);
        }
        catch (UnauthorizedAccessException)
        {
          continue;
        }
        for (int index = 0; index < files.Count; index++)
          yield return files[index];
        for (int index = 0; index < folders.Length; index++)
        {
          string name = Path.GetFileName(folders[index]);
          if (_skip.Contains(name) is false)
            pending.Push(folders[index]);
        }
      }
    }

    private static void AddProject(string file)
    {
      string directory = Path.GetDirectoryName(file);
      if (string.IsNullOrEmpty(directory))
        return;
      string text = null;
      try
      {
        text = File.ReadAllText(file);
      }
      catch (IOException)
      {
        return;
      }
      string rootNamespace = ReadElement(text, "Import_RootNamespace") ?? ReadElement(text, "RootNamespace");
      if (string.IsNullOrEmpty(rootNamespace))
        rootNamespace = Path.GetFileNameWithoutExtension(file);
      _projects.Add(new UIDesignProject
      {
        ProjectFile = file,
        Directory = directory,
        RootNamespace = rootNamespace,
        UseThisFileDirectory = text.Contains("$(MSBuildThisFileDirectory)", StringComparison.Ordinal),
      });
    }

    private static string ReadElement(string text, string name)
    {
      string open = string.Concat("<", name, ">");
      string close = string.Concat("</", name, ">");
      int start = text.IndexOf(open, StringComparison.Ordinal);
      if (start < 0)
        return null;
      start += open.Length;
      int end = text.IndexOf(close, start, StringComparison.Ordinal);
      if (end < 0)
        return null;
      return text.Substring(start, end - start).Trim();
    }

    private static UIDesignProject MatchProject(string @namespace)
    {
      UIDesignProject best = null;
      for (int index = 0; index < _projects.Count; index++)
      {
        UIDesignProject project = _projects[index];
        if (string.IsNullOrEmpty(project.RootNamespace))
          continue;
        bool match = string.Equals(@namespace, project.RootNamespace, StringComparison.Ordinal)
          || (@namespace?.StartsWith(project.RootNamespace + ".", StringComparison.Ordinal) ?? false);
        if (match is false)
          continue;
        if (best is null || project.RootNamespace.Length > best.RootNamespace.Length)
          best = project;
      }
      return best;
    }

    private static UIDesignProject FindProjectFile(string anyPath)
    {
      if (string.IsNullOrEmpty(anyPath))
        return null;
      UIDesignProject best = null;
      string full = Path.GetFullPath(anyPath);
      for (int index = 0; index < _projects.Count; index++)
      {
        UIDesignProject project = _projects[index];
        string directory = Path.GetFullPath(project.Directory);
        //前缀比较必须带分隔符: 否则 D:\Code\Game 会匹配上 D:\Code\Game2\...
        if (directory.EndsWith(Path.DirectorySeparatorChar) is false)
          directory = string.Concat(directory, Path.DirectorySeparatorChar);
        if (full.StartsWith(directory, StringComparison.OrdinalIgnoreCase) is false)
          continue;
        if (best is null || project.Directory.Length > best.Directory.Length)
          best = project;
      }
      return best;
    }

    /// <summary>按文件名找源码文件, 并用命名空间消除"同名类"的歧义.</summary>
    private static bool TryFindSourceFile(string fileName, string @namespace, out string path)
    {
      path = null;
      List<string> candidates = FindCandidates(fileName);
      if (candidates.Count == 0)
        return false;
      for (int index = 0; index < candidates.Count; index++)
      {
        if (DeclaresNamespace(candidates[index], @namespace))
        {
          path = candidates[index];
          return true;
        }
      }
      //没有命名空间能对上时: 只有唯一候选才敢用 (同名类各有一份文件时宁可让调用方回退, 也不覆盖别人).
      if (candidates.Count == 1)
      {
        path = candidates[0];
        return true;
      }
      return false;
    }

    private static List<string> FindCandidates(string fileName)
    {
      EnsureIndex();
      return _fileIndex.TryGetValue(fileName, out List<string> list) ? list : EmptyCandidates;
    }

    private static readonly List<string> EmptyCandidates = new List<string>();

    private static void EnsureIndex()
    {
      if (_indexedProjectCount == _projects.Count)
        return;
      _fileIndex.Clear();
      for (int index = 0; index < _projects.Count; index++)
        IndexProject(_projects[index]);
      _indexedProjectCount = _projects.Count;
    }

    private static void IndexProject(UIDesignProject project)
    {
      Stack<string> pending = new Stack<string>();
      pending.Push(project.Directory);
      while (pending.Count > 0)
      {
        string current = pending.Pop();
        string[] folders;
        string[] files;
        try
        {
          folders = System.IO.Directory.GetDirectories(current);
          files = System.IO.Directory.GetFiles(current, "*.cs");
        }
        catch (UnauthorizedAccessException)
        {
          continue;
        }
        for (int index = 0; index < files.Length; index++)
        {
          string name = Path.GetFileName(files[index]);
          if (_fileIndex.TryGetValue(name, out List<string> list) is false)
            _fileIndex[name] = list = new List<string>(1);
          list.Add(files[index]);
        }
        for (int index = 0; index < folders.Length; index++)
        {
          string name = Path.GetFileName(folders[index]);
          if (_skip.Contains(name) is false)
            pending.Push(folders[index]);
        }
      }
    }

    private static bool DeclaresNamespace(string file, string @namespace)
    {
      if (string.IsNullOrEmpty(@namespace))
        return false;
      string token = string.Concat("namespace ", @namespace);
      try
      {
        using StreamReader reader = new StreamReader(file);
        for (int line = 0; line < 80; line++)
        {
          string text = reader.ReadLine();
          if (text is null)
            break;
          int at = text.IndexOf(token, StringComparison.Ordinal);
          if (at < 0)
            continue;
          int end = at + token.Length;
          if (end >= text.Length)
            return true;
          char next = text[end];
          if (char.IsLetterOrDigit(next) || next == '.' || next == '_')
            continue;
          return true;
        }
      }
      catch (IOException)
      {
        return false;
      }
      return false;
    }

    private static bool HasByteOrderMark(string file)
    {
      try
      {
        using FileStream stream = File.OpenRead(file);
        int first = stream.ReadByte();
        int second = stream.ReadByte();
        int third = stream.ReadByte();
        return first == 0xEF && second == 0xBB && third == 0xBF;
      }
      catch (IOException)
      {
        return false;
      }
    }
  }
}
