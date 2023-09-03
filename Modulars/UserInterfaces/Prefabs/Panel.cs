using Colin.Core.Modulars.UserInterfaces.Renderers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Panel : Division
    {
        public Panel( string name ) : base( name ) { }
        public override void OnInit( )
        {
            BindRenderer<DivNinecutRenderer>( ).Bind( Sprite.Get( "UserInterfaces/Forms/ArchiveSelectButton" ) ).Cut = 8;
            base.OnInit( );
        }
    }
}