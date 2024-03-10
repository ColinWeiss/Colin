namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Canvas : Div
    {
        public Canvas(string name) : base(name, true) { }
        public override void DivInit()
        {
            SetCanvas(Layout.Width, Layout.Height);
            base.DivInit();
        }
        public void SetCanvas(float width, float height)
        {
            Layout.Width = width;
            Layout.Height = height;
            Design.Anchor = new Vector2(Layout.Width / 2, Layout.Height / 2);
            Canvas?.Dispose();
            Canvas = RenderTargetExt.CreateDefault((int)width, (int)height);
        }
    }
}