using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Panel : Div
    {
        public Panel(string name) : base(name) { }
        public override void DivInit()
        {
            BindRenderer<DivNinecutRenderer>().Bind(Sprite.Get("UserInterfaces/Forms/ArchiveSelectButton")).Cut = new Point(8, 8);
            base.DivInit();
        }
    }
}