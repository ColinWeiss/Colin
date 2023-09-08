using Colin.Core.Common;

namespace Colin.Core.Modulars.UserInterfaces
{
    public class UserInterface : ISceneComponent, IRenderableSceneComponent
    {
        /// <summary>
        /// 指示当前焦点元素.
        /// </summary>
        public static Division Focus;

        private Container _contianer = new Container( "NomalContainer" );
        public Container Container => _contianer;

        public RenderTarget2D SceneRt { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public Scene Scene { get; set; }

        public void DoInitialize() => _contianer.DoInitialize();

        public void DoUpdate( GameTime time )
        {
            Container?.DoUpdate( time );
            Container.Seek()?.Events.Execute();
        }

        public void DoRender( SpriteBatch batch )
        {
            batch.Begin();
            Container?.DoRender( batch );
            batch.End();
        }

        public void Register( Container container ) => Container?.Register( container );

        public void Remove( Container container, bool dispose ) => Container?.Remove( container );

        public void SetContainer( Container container )
        {
            _contianer = container;
            _contianer._interface = this;
        }
    }
}