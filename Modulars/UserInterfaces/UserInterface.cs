using Colin.Core.Common;
using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces
{
    public class UserInterface : ISceneModule, IRenderableISceneModule
    {
        public Division Focus;

        public Division LastFocus;

        private Container _contianer = new Container( "NomalContainer" );

        public Container Container => _contianer;

        public RenderTarget2D SceneRt { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public bool FinalPresentation { get; set; }

        public Scene Scene { get; set; }

        public EventResponder Events;

        public void DoInitialize()
        {
            Events = new EventResponder( "UserInterface.EventResponder" );
            Scene.Event.Mouse.Register( Events );
            Scene.Event.Keyboard.Register( Events );
        }

        public void Start() { }

        public void DoUpdate( GameTime time )
        {
            Container?.DoUpdate( time );
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
            container._interface = this;
            _contianer = container;
            container.DoInitialize();
            Events.Register( container.Events.Mouse );
            Events.Register( container.Events.Keys );
        }

        public void Dispose()
        {
            Scene = null;
            Container.Dispose();
        }
    }
}