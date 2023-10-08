using Colin.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
    public class SceneEventResponder : ISceneModule
    {
        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public event EventHandler ClientSizeChanged;
        public event EventHandler OrientationChanged;

        public MouseEventResponder Mouse;

        public KeysEventResponder Keyboard; 

        public void Start() { }
        public void DoInitialize()
        {
            Mouse = new MouseEventResponder( "Scene.MouseEventResponder" );
            Keyboard = new KeysEventResponder( "Scene.KeysEventResponder" );
        }
        public void DoUpdate( GameTime time )
        {
            MouseEventArgs mouseEvent = new MouseEventArgs( "MouseEvent" );
            KeysEventArgs keyboardEvent = new KeysEventArgs( "KeysEvent" );
            Mouse.Response( mouseEvent );
            Keyboard.Response( keyboardEvent );
        }
        public void InvokeSizeChange( object sender, EventArgs e )
        {
            ClientSizeChanged?.Invoke( this, e );
            OrientationChanged?.Invoke( this, e );
        }
        public void Dispose() { }
    }
}