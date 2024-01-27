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

        public ScreenReprocess ScreenReprocess = new ScreenReprocess();

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
            SceneRenderPreset();
            Modules.DoRender(EngineInfo.SpriteBatch);
            SceneRender();
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(null);
            EngineInfo.SpriteBatch.Begin();
            EngineInfo.SpriteBatch.Draw(SceneRenderTarget, new Rectangle(0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight), Color.White);
            EngineInfo.SpriteBatch.End();
            base.Draw(gameTime);
        }
        public virtual void RenderStart() { }
        public virtual void SceneRenderPreset() { }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int count = 0; count < Modules.Components.Count; count++)
                    Modules.Components.Values.ElementAt(count).Dispose();
                for (int count = 0; count < Modules.RenderableComponents.Count; count++)
                {
                    Modules.RenderableComponents.Values.ElementAt(count).RawRt.Dispose();
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