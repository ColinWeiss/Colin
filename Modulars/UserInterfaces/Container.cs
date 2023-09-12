namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 指代用户交互界面中的容器.
    /// </summary>
    public class Container : Division
    {
        public Container( string name ) : base( name ) => _container = this;
        public override sealed void OnInit()
        {
            Interact.IsInteractive = true;
            Interact.IsSelectable = false;
            Layout.Width = EngineInfo.ViewWidth;
            Layout.Height = EngineInfo.ViewHeight;
            EngineInfo.Engine.Window.ClientSizeChanged += Window_ClientSizeChanged;
            ContainerInitialize();
            base.OnInit();
        }
        private void Window_ClientSizeChanged( object sender, EventArgs e )
        {
            Layout.Width = EngineInfo.ViewWidth;
            Layout.Height = EngineInfo.ViewHeight;
        }
        /// <summary>
        /// 在此处进行容器初始化操作.
        /// </summary>
        public virtual void ContainerInitialize() { }
        public void SetTop( Division division )
        {
            if(Children.Contains( division ))
            {
                Children.Remove( division );
                Children.Add( division );
            }
        }
        public override bool Register( Division division, bool doInit = false )
        {
            if(base.Register( division, doInit ))
            {
                return true;
            }
            else
                return false;
        }
    }
}