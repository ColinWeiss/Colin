using Colin.Core.Modulars.UserInterfaces.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.UserInterfaces.Forms
{
    public class PopupInputTextBox : Popup
    {
        public PopupInputTextBox( string name, int width, int height, int titleHeight ) : base( name, width, height, titleHeight ) { }

        public InputTextBox InputTextBox;

        public override void PopupInit( )
        {
            InputTextBox = new InputTextBox( "InputBox", 12 );
            InputTextBox.Layout.Left = 4;
            InputTextBox.Layout.Height = 4;
            InputTextBox.Layout.Width = Layout.Width - 8;
            InputTextBox.Layout.Height = Layout.Height - 8;
            InputTextBox.Label.Design.Color = new Color( 255, 223, 135 );
            Register( InputTextBox );
            base.PopupInit( );
        }
        public override void OnUpdate( GameTime time )
        {
            base.OnUpdate( time );
        }
    }
}
