namespace Colin.Core.Common
{
  /// <summary>
  /// 场景模块集合.
  /// </summary>
  public class SceneModuleList : IDisposable
  {
    private readonly Scene Scene;

    public SceneModuleList(Scene scene) => Scene = scene;

    public Dictionary<Type, ISceneModule> Components = new Dictionary<Type, ISceneModule>();

    public Dictionary<Type, IRenderableISceneModule> RenderableComponents = new Dictionary<Type, IRenderableISceneModule>();

    public string Name => nameof(SceneModuleList);

    public string DisplayName => Name;

    public int Count => Components.Count;

    public int RenderModuleCount => RenderableComponents.Count;

    public void DoStart()
    {
      ISceneModule _com;
      for (int count = 0; count < Components.Count; count++)
      {
        _com = Components.Values.ElementAt(count);
        if (_com.Enable)
          _com.Start();
      }
    }

    public void DoUpdate(GameTime gameTime)
    {
      ISceneModule _com;
      for (int count = 0; count < Components.Count; count++)
      {
        _com = Components.Values.ElementAt(count);
        if (_com.Enable)
          _com.DoUpdate(gameTime);
      }
    }

    public void DoRender(SpriteBatch batch)
    {
      IRenderableISceneModule renderMode;
      RenderTarget2D frameRenderLayer;
      CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(Scene.SceneRenderTarget);
      CoreInfo.Graphics.GraphicsDevice.Clear(Color.Transparent);
      for (int count = 0; count < RenderableComponents.Values.Count; count++)
      {
        renderMode = RenderableComponents.Values.ElementAt(count);
        frameRenderLayer = renderMode.RawRt;
        CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(frameRenderLayer);
        CoreInfo.Graphics.GraphicsDevice.Clear(Color.Transparent);
        if (renderMode.RawRtVisible)
        {
          renderMode.DoRawRender(CoreInfo.Graphics.GraphicsDevice, CoreInfo.Batch);
        }
      }
      CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(Scene.SceneRenderTarget);
      CoreInfo.Graphics.GraphicsDevice.Clear(Color.Transparent);
      for (int count = RenderableComponents.Values.Count - 1; count >= 0; count--)
      {
        renderMode = RenderableComponents.Values.ElementAt(count);
        frameRenderLayer = renderMode.RawRt;
        CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(Scene.SceneRenderTarget);
        if (renderMode.Presentation)
        {
          renderMode.DoRegenerateRender(CoreInfo.Graphics.GraphicsDevice, batch);
          if (Scene.ScreenReprocess.Effects.TryGetValue(renderMode, out Effect e))
            CoreInfo.Batch.Begin(SpriteSortMode.Deferred, effect: e);
          else
            CoreInfo.Batch.Begin(SpriteSortMode.Deferred);
          CoreInfo.Batch.Draw(frameRenderLayer, new Rectangle(0, 0, CoreInfo.ViewWidth, CoreInfo.ViewHeight), Color.White);
          CoreInfo.Batch.End();
        }
      }
    }

    /// <summary>
    /// 根据指定类型获取场景组件.
    /// </summary>
    /// <typeparam name="T">指定的 <see cref="ISceneModule"/> 类型.</typeparam>
    /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
    public T GetModule<T>() where T : ISceneModule => (T)Components.GetValueOrDefault(typeof(T), null);

    /// <summary>
    /// 根据指定类型获取场景渲染组件.
    /// </summary>
    /// <typeparam name="T">指定的 <see cref="IRenderableISceneModule"/> 类型.</typeparam>
    /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
    public T GetRenderModule<T>() where T : IRenderableISceneModule => (T)RenderableComponents.GetValueOrDefault(typeof(T), null);

    /// <summary>
    /// 根据指定类型删除场景模块.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>如果成功删除, 那么返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public bool RemoveModule<T>() where T : ISceneModule => Components.Remove(typeof(T));

    public T Register<T>() where T : ISceneModule, new()
    {
      T sceneMode = new T();
      sceneMode.Scene = Scene;
      sceneMode.Enable = true;
      if (sceneMode is IRenderableISceneModule dwMode)
      {
        dwMode.RawRtVisible = true;
        dwMode.Presentation = true;
        RenderableComponents.Add(dwMode.GetType(), dwMode);
      }
      sceneMode.DoInitialize();
      if (sceneMode is IRenderableISceneModule dwMode2)
      {
        if (dwMode2.RawRt is null)
        {
          dwMode2.InitRenderTarget();
          Scene.Events.ClientSizeChanged += dwMode2.OnClientSizeChanged;
        }
      }
      Components.Add(typeof(T), sceneMode);
      return sceneMode;
    }

    public T Register<T>(params object[] args) where T : ISceneModule
    {
      T sceneMode = (T)Activator.CreateInstance(typeof(T), args);
      sceneMode.Scene = Scene;
      sceneMode.Enable = true;
      if (sceneMode is IRenderableISceneModule dwMode)
      {
        dwMode.RawRtVisible = true;
        dwMode.Presentation = true;
        RenderableComponents.Add(dwMode.GetType(), dwMode);
      }
      sceneMode.DoInitialize();
      if (sceneMode is IRenderableISceneModule dwMode2)
      {
        if (dwMode2.RawRt is null)
        {
          dwMode2.InitRenderTarget();
          Scene.Events.ClientSizeChanged += dwMode2.OnClientSizeChanged;
        }
      }
      Components.Add(typeof(T), sceneMode);
      return sceneMode;
    }

    public KeyValuePair<Type,ISceneModule> ElementAt(int index)
    {
      return Components.ElementAt(index);
    }

    public void Add(ISceneModule sceneMode)
    {
      sceneMode.Scene = Scene;
      sceneMode.Enable = true;
      if (sceneMode is IRenderableISceneModule dwMode)
      {
        dwMode.RawRtVisible = true;
        dwMode.Presentation = true;
        RenderableComponents.Add(dwMode.GetType(), dwMode);
      }
      sceneMode.DoInitialize();
      if (sceneMode is IRenderableISceneModule dwMode2)
      {
        if (dwMode2.RawRt is null)
        {
          dwMode2.InitRenderTarget();
          Scene.Events.ClientSizeChanged += dwMode2.OnClientSizeChanged;
        }
      }
      Components.Add(sceneMode.GetType(), sceneMode);
    }

    public bool Remove(ISceneModule sceneMode)
    {
      if (Components.Remove(sceneMode.GetType()))
      {
        if (sceneMode is IRenderableISceneModule dwMode)
        {
          RenderableComponents.Remove(dwMode.GetType());
          Scene.Events.ClientSizeChanged -= dwMode.OnClientSizeChanged;
        }
        return true;
      }
      return false;
    }

    public void Clear()
    {
      Components.Clear();
      RenderableComponents.Clear();
    }

    public void Dispose()
    {
      for (int count = 0; count < Components.Count; count++)
        Components.Values.ElementAt(count).Dispose();
      for (int count = 0; count < RenderableComponents.Count; count++)
      {
        RenderableComponents.Values.ElementAt(count).RawRt.Dispose();
        RenderableComponents.Values.ElementAt(count).Dispose();
      }
    }
  }
}