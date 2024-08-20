namespace Colin.Core.Common
{
  public class SceneManager : GameComponent, ISingleton
  {
    private static Scene _currentScene;
    public static Scene CurrentScene => _currentScene;

    private static Scene _toBeUsedScene;

    private static Dictionary<string, Scene> _permanentScenes = new Dictionary<string, Scene>();

    public SceneManager() : base(CoreInfo.Core) { }

    public static T SetScene<T>() where T : Scene, new()
    {
      T scene = new T();
      _toBeUsedScene = scene;
      return scene;
    }
    public static void SetScene(Scene scene)
    {
      _toBeUsedScene = scene;
    }

    public static void AddPermanentScene(string sceneIdentifier, Scene scene)
    {
      if (_permanentScenes.ContainsKey(sceneIdentifier))
        Console.WriteLine("Error", "场景标识符重复.");
      else if (_permanentScenes.ContainsValue(scene))
        Console.WriteLine("Error", "场景重复.");
      else
        _permanentScenes.Add(sceneIdentifier, scene);
    }
    public static void SetPermanentScene(string sceneIdentifier)
    {
      if (_permanentScenes.TryGetValue(sceneIdentifier, out Scene scene))
        SetScene(scene);
      Console.WriteLine("Error", "场景标识符错误.");
    }
    public static void SetPermanentScene(string sceneIdentifier, Scene scene)
    {
      if (_permanentScenes.ContainsKey(sceneIdentifier))
      {
        SetPermanentScene(sceneIdentifier);
      }
      else
      {
        _permanentScenes.Add(sceneIdentifier, scene);
        SetScene(scene);
      }
    }
    public static T SetPermanentScene<T>(string sceneIdentifier) where T : Scene, new()
    {
      if (_permanentScenes.ContainsKey(sceneIdentifier))
      {
        SetPermanentScene(sceneIdentifier);
      }
      else
      {
        T scene = new T();
        _permanentScenes.Add(sceneIdentifier, scene);
        SetScene(scene);
        return scene;
      }
      return null;
    }

    private float _disposePromptTimer;
    public override void Update(GameTime gameTime)
    {
      if (_toBeUsedScene is not null)
      {
        if (_currentScene is not null)
        {
          if (_currentScene.InitializeOnSwitch)
          {
            CoreInfo.Core.Window.ClientSizeChanged -= _currentScene.InitRenderTarget;
            CoreInfo.Core.Window.OrientationChanged -= _currentScene.InitRenderTarget;
          }
          CoreInfo.Core.Components.Remove(_currentScene);
          while (!_currentScene.CanDispose)
          {
            _disposePromptTimer += Time.UnscaledDeltaTime;
            if (_disposePromptTimer >= 1)
            {
              Console.WriteLine(_currentScene.GetType().Name );
              _disposePromptTimer -= 1;
            }
          }
          _currentScene?.Dispose();
        }
        CoreInfo.Core.Components.Add(_toBeUsedScene);
        _currentScene = _toBeUsedScene;
        _toBeUsedScene = null;
      }
      base.Update(gameTime);
    }
  }
}