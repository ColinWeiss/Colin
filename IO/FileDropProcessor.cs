using MonoGame.Framework.Utilities;
using System.Reflection;

namespace Colin.Core.IO
{
  public sealed class FileDropProcessor : GameComponent
  {
    internal FileDropProcessor() : base(CoreInfo.Engine) { }
    private static FileDropProcessor _instance;
    public static FileDropProcessor Instance
    {
      get
      {
        if (_instance == null)
          _instance = new FileDropProcessor();
        return _instance;
      }
    }

    public bool initializeOnSwitch = true;

    public List<IFileDropBehavior> FileDropBehaviors = new List<IFileDropBehavior>();

    public override void Initialize()
    {
      foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (!item.IsAbstract && item.GetInterfaces().Contains(typeof(IFileDropBehavior)))
        {
          FileDropBehaviors.Add((IFileDropBehavior)Activator.CreateInstance(item));
        }
      }
      if (PlatformInfo.MonoGamePlatform == MonoGamePlatform.Windows || PlatformInfo.MonoGamePlatform == MonoGamePlatform.DesktopGL)
      {
        Game.Window.FileDrop += (s, e) =>
        {
          IFileDropBehavior fileDropBehavior;
          for (int count = 0; count < FileDropBehaviors.Count; count++)
          {
            fileDropBehavior = FileDropBehaviors[count];
            fileDropBehavior.OnFileDrop(e.Files);
          }
        };
      }
      base.Initialize();
    }
  }
}