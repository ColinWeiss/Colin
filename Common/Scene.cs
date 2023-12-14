/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Collections;
using Colin.Core.Extensions;
using Colin.Core.Assets;
在此之后:
using Colin.Core.Assets;
using Colin.Core.Collections;
using Colin.Core.Extensions;
*/

namespace Colin.Core.Common
{
    /// <summary>
    /// 场景.
    /// </summary>
    public class Scene : DrawableGameComponent
    {
        public SceneCamera SceneCamera;

        private SceneModuleList _components;
        /// <summary>
        /// 获取场景模块列表.
        /// </summary>
        public SceneModuleList Modules => _components;

        /// <summary>
        /// 指示场景在切换时是否执行初始化的值.
        /// </summary>
        public bool InitializeOnSwitch = true;

        /// <summary>
        /// 场景本身的 RenderTarget.
        /// </summary>
        public RenderTarget2D SceneRenderTarget;

        public SceneShaderManager SceneShaders = new SceneShaderManager();

        public SceneEventResponder Events;

        public Scene() : base(EngineInfo.Engine)
        {
            Events = new SceneEventResponder();
            // 仅此一处管理Game.Window事件，其他地方都用Scene.Event统一进行管理，不需要单独删除
        }

        public override sealed void Initialize()
        {
            Started = false;
            if (InitializeOnSwitch)
            {
                InitRenderTarget(this, new EventArgs());
                Events.OrientationChanged += InitRenderTarget;
                Events.ClientSizeChanged += InitRenderTarget;
                _components = new SceneModuleList(this);
                _components.Add(Events);
                _components.Add(SceneCamera = new SceneCamera());
                SceneInit();
                Game.Window.ClientSizeChanged += Events.InvokeSizeChange;
                Game.Window.OrientationChanged += Events.InvokeSizeChange;
            }
            base.Initialize();
        }

        internal void InitRenderTarget(object s, EventArgs e)
        {
            SceneRenderTarget?.Dispose();
            SceneRenderTarget = RenderTargetExt.CreateDefault();
        }

        /// <summary>
        /// 执行场景初始化.
        /// </summary>
        public virtual void SceneInit() { }

        internal bool Started = false;
        public override sealed void Update(GameTime gameTime)
        {
            if (!Started)
            {
                Start();
                Modules.DoStart();
                Started = true;
            }
            UpdatePreset();
            Modules.DoUpdate(gameTime);
            SceneUpdate();
            base.Update(gameTime);
        }
        public virtual void Start() { }
        public virtual void UpdatePreset() { }
        public virtual void SceneUpdate() { }

        private bool _skipRender = true;
        private bool _renderStarted = false;
        public override sealed void Draw(GameTime gameTime)
        {
            if (_skipRender is true)
            {
                _skipRender = false;
                return;
            }
            else if(_renderStarted is false)
            {
                RenderStart();
                _renderStarted = true;
            }
            RenderPreset();
            IRenderableISceneModule renderMode;
            RenderTarget2D frameRenderLayer;
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(SceneRenderTarget);
            EngineInfo.Graphics.GraphicsDevice.Clear(Color.Black);
            for (int count = 0; count < Modules.RenderableComponents.Values.Count; count++)
            {
                renderMode = Modules.RenderableComponents.Values.ElementAt(count);
                if (renderMode.Visible)
                {
                    frameRenderLayer = renderMode.ModuleRt;
                    EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(frameRenderLayer);
                    EngineInfo.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    renderMode.DoRender(EngineInfo.SpriteBatch);
                }
            }
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(SceneRenderTarget);
            for (int count = Modules.RenderableComponents.Values.Count - 1; count >= 0; count--)
            {
                renderMode = Modules.RenderableComponents.Values.ElementAt(count);
                if (renderMode.FinalPresentation)
                {
                    frameRenderLayer = renderMode.ModuleRt;
                    if (SceneShaders.Effects.TryGetValue(renderMode, out Effect e))
                        EngineInfo.SpriteBatch.Begin(SpriteSortMode.Deferred, effect: e);
                    else
                        EngineInfo.SpriteBatch.Begin(SpriteSortMode.Deferred);
                    EngineInfo.SpriteBatch.Draw(frameRenderLayer, new Rectangle(0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight), Color.White);
                    EngineInfo.SpriteBatch.End();
                }
            }
            SceneRender();
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(null);
            EngineInfo.SpriteBatch.Begin();
            EngineInfo.SpriteBatch.Draw(SceneRenderTarget, new Rectangle(0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight), Color.White);
            EngineInfo.SpriteBatch.End();
            base.Draw(gameTime);
        }
        public virtual void RenderStart() { }
        public virtual void RenderPreset() { }
        public virtual void SceneRender() { }

        /// <summary>
        /// 根据指定类型获取场景模块.
        /// </summary>
        /// <typeparam name="T">指定的 <see cref="ISceneModule"/> 类型.</typeparam>
        /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
        public T GetModule<T>() where T : ISceneModule => Modules.GetModule<T>();

        /// <summary>
        /// 根据指定类型获取场景渲染模块.
        /// </summary>
        /// <typeparam name="T">指定的 <see cref="IRenderableISceneModule"/> 类型.</typeparam>
        /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
        public T GetRenderModule<T>() where T : IRenderableISceneModule => Modules.GetRenderModule<T>();

        /// <summary>
        /// 根据指定类型删除场景模块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>如果成功删除, 那么返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
        public bool RemoveModule<T>() where T : ISceneModule => Modules.RemoveModule<T>();

        /// <summary>
        /// 根据指定对象删除场景模块.
        /// </summary>
        /// <returns>如果成功删除, 那么返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
        public bool RemoveModule(ISceneModule module) => Modules.Remove(module);

        /// <summary>
        /// 我不在乎你加不加载, 但我希望玩家的电脑犯病的时候我们能把重要数据保存下来.
        /// <br>这个方法将在 <see cref="Game.Exit"/> 执行时跟着执行执行.</br>
        /// <br>你也可以把它写成能手动调用用来保存数据的样子.</br>
        /// </summary>
        public virtual void SaveDatas() { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int count = 0; count < Modules.Components.Count; count++)
                    Modules.Components.Values.ElementAt(count).Dispose();
                for (int count = 0; count < Modules.RenderableComponents.Count; count++)
                {
                    Modules.RenderableComponents.Values.ElementAt(count).ModuleRt.Dispose();
                    Modules.RenderableComponents.Values.ElementAt(count).Dispose();
                }
                Modules.Clear();
            }
            if (Game.Window is not null)
            {
                Game.Window.ClientSizeChanged -= Events.InvokeSizeChange;
                Game.Window.OrientationChanged -= Events.InvokeSizeChange;
            }
            base.Dispose(disposing);
        }
    }
}