using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Panel : Division
    {
        public Panel( string name ) : base( name ) { }
        public override void OnInit()
        {
            BindRenderer<DivNinecutRenderer>().Bind( Sprite.Get( "UserInterfaces/Forms/ArchiveSelectButton" ) ).Cut = new Point( 8, 8 );
            base.OnInit();
        }
    }
}