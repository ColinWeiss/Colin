namespace Colin.Core.Common
{
    /// <summary>
    /// 场景管理器.
    /// </summary>
    public sealed class SceneManager : ISingleton
    {
        public static SceneManager Instance => Singleton<SceneManager>.Instance;

        public SceneManager() { }

        public Dictionary<Type, Scene> Scenes = new Dictionary<Type, Scene>();

        public T CreateScene<T>() where T : Scene, new()
        {
            T scene = new T();
            if(!Scenes.ContainsKey( typeof( T ) ))
                Scenes.Add( typeof( T ), scene );
            else
            {
                Scenes.Remove( typeof( T ) );
                Scenes.Add( typeof( T ), scene );
            }
            return scene;
        }
        public T SetScene<T>() where T : Scene, new()
        {
            if(Scenes.TryGetValue( typeof( T ), out Scene gotScene ))
            {
                EngineInfo.Engine.SetScene( gotScene );
                return (T)gotScene;
            }
            else
            {
                T t = CreateScene<T>();
                EngineInfo.Engine.SetScene( t );
                return t;
            }
        }
        public void SetScene( Scene scene )
        {
            if(Scenes.TryGetValue( scene.GetType(), out Scene gotScene ))
                EngineInfo.Engine.SetScene( gotScene );
            else
            {
                Scenes.Add( scene.GetType(), scene );
                EngineInfo.Engine.SetScene( scene );
            }
        }
    }
}