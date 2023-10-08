using Colin.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
    public class SceneEventResponder : EventResponder, ISceneModule
    {
        public SceneEventResponder( string name ) : base( name ) { }

        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public event EventHandler ClientSizeChanged;
        public event EventHandler OrientationChanged;

        public void Start() { }
        public void DoInitialize()
        {
        }
        public void DoUpdate( GameTime time )
        {
            MouseEventArgs mouseEvent = new MouseEventArgs( "MouseEvent" );
            Response( mouseEvent );
        }
        public void InvokeSizeChange( object sender, EventArgs e )
        {
            ClientSizeChanged?.Invoke( this, e );
            OrientationChanged?.Invoke( this, e );
        }
        public void Dispose() { }
    }
}