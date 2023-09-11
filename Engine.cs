/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Inputs;
using Colin.Core.IO;
using Colin.Core.Assets;
using MonoGame.Framework.Utilities;
在此之后:
using Colin.Core.Common;
using Colin.Core.Inputs;
using Colin.Core.IO;
using Colin.Core.ModLoaders;
*/
using Colin.Core.IO;
using Colin.Core.ModLoaders;
using Colin.Core.Developments;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace Colin.Core
{
#if WINDOWS
    public partial class Engine
    {
        private Form _form;
        /// <summary>
        /// 获取本程序在 Windows 平台下的窗体对象.
        /// </summary>
        public Form Form => _form;
        public void FormInitialize()
        {
            _form = Control.FromHandle( Window.Handle ) as Form;
            _form.MinimizeBox = false;
            _form.MaximizeBox = false;
            //       _form.MinimumSize = new System.Drawing.Size( 1280, 720 );
        }
    }
#endif
    public partial class Engine : Game, IMod
    {
        public string Name => "Colin.Core.Engine";

        public EngineInfo Info;

        public bool Enable { get; set; } = true;

        public bool Visiable { get; set; } = true;

        /// <summary>
        /// 指示当前活跃场景.
        /// </summary>
        public Scene CurrentScene { get; internal set; }

        public AssetLoader AssetLoader { get; private set; }

        private int _targetFrame = 60;
        /// <summary>
        /// 指示程序目标帧率.
        /// </summary>
        public int TargetFrame
        {
            get => _targetFrame;
            set => SetTargetFrame( value );
        }

        public Engine()
        {
            ProgramChecker.DoCheck();
            EngineInfo.Init( this );
            if(EngineInfo.Graphics == null)
            {
                EngineInfo.Graphics = new GraphicsDeviceManager( this )
                {
                    PreferHalfPixelOffset = false,
                    HardwareModeSwitch = false,
                    SynchronizeWithVerticalRetrace = true,
                    PreferMultiSampling = true,
                };
            }
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            IsFixedTimeStep = true;
#if WINDOWS
            FormInitialize();
#endif
        }

        public void SetTargetFrame( int frame )
        {
            _targetFrame = frame;
            TargetElapsedTime = new TimeSpan( 0, 0, 0, 0, (int)Math.Round( 1000f / frame ) );
        }

        /// <summary>
        /// 切换场景.
        /// </summary>
        /// <param name="scene">要切换到的场景对象.</param>
        public void SetScene( Scene scene )
        {
            if(CurrentScene != null)
            {
                if(CurrentScene.InitializeOnSwitch)
                {
                    Window.ClientSizeChanged -= CurrentScene.InitRenderTarget;
                    Window.OrientationChanged -= CurrentScene.InitRenderTarget;
                }
                CurrentScene.UnLoad();
                Components.Remove( CurrentScene );
            }
            Components.Clear();
            Components.Add( Singleton<ControllerResponder>.Instance );
            Components.Add( Singleton<MouseResponder>.Instance );
            Components.Add( Singleton<KeyboardResponder>.Instance );
            Components.Add( Singleton<Input>.Instance );
            Components.Add( scene );
            CurrentScene = scene;
            GC.Collect();
        }

        protected override sealed void Initialize()
        {
            EngineInfo.SpriteBatch = new SpriteBatch( EngineInfo.Graphics.GraphicsDevice );
            EngineInfo.Config = new Config();
            EngineInfo.Config.Load();
            TargetElapsedTime = new TimeSpan( 0, 0, 0, 0, (int)Math.Round( 1000f / TargetFrame ) );
            Components.Add( FileDropProcessor.Instance );
            DoInitialize();
            base.Initialize();
        }
        public virtual void DoInitialize() { }

        protected override sealed void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// 在程序开始运行时执行.
        /// </summary>
        public virtual void Start() { }

        private bool Started = false;
        protected override sealed void Update( GameTime gameTime )
        {
            if( !Enable )
                return;
            Time.Update( (float)gameTime.ElapsedGameTime.TotalSeconds );
            if(!Started)
            {
#if WINDOWS
                Form.Location =
                    new System.Drawing.Point(
                        Screen.PrimaryScreen.Bounds.Width / 2 - Form.Width / 2,
                        Screen.PrimaryScreen.Bounds.Height / 2 - Form.Height / 2
                        );
#endif
                AssetLoader = new AssetLoader();
                AssetLoader.OnLoadComplete += ( s, e ) => Start();
                SetScene( AssetLoader );
                Started = true;
            }
            EngineInfo.GetInformationFromDevice( gameTime );
            DoUpdate();
            base.Update( gameTime );
        }
        public virtual void DoUpdate() { }

        protected override sealed void Draw( GameTime gameTime )
        {
            if(!Visiable)
                return;
            GraphicsDevice.Clear( Color.Black );
            base.Draw( gameTime );
            DoRender();
        }
        public virtual void DoRender() { }

        protected override void OnExiting( object sender, EventArgs args )
        {
            CurrentScene?.SaveDatas();
            EngineInfo.Config.Save();
            base.OnExiting( sender, args );
        }

        protected override void OnActivated( object sender, EventArgs args )
        {
          //  Enable = true;
            base.OnActivated( sender, args );
        }
        protected override void OnDeactivated( object sender, EventArgs args )
        {
          //  Enable = false;
            base.OnDeactivated( sender, args );
        }
    }
}