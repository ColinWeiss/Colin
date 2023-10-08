using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Prefabs;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Forms
{
    public class Popup : Canvas
    {
        private int _titleHeight;

        public Popup( string name, int width, int height, int titleHeight ) : base( name )
        {
            Layout.Width = width;
            Layout.Height = height;
            _titleHeight = titleHeight;
        }

        public Division Substrate;

        public Division TitleColumn;

        public Division Icon;

        public Division CloseButton;

        public Division Block;

        public override sealed void OnInit()
        {
            Design.Scale = Vector2.One;
            Design.Color = Color.White;

            Controller = new DivGradientController( this );
            Interact.IsDraggable = true;
            Interact.IsSelectable = false;
            Interact.IsInteractive = true;

            Layout.PaddingLeft = 4;
            Layout.PaddingTop = 4;

            Substrate = new Division( "Substrate" );
            Substrate.Interact.IsInteractive = false;
            Substrate.Layout.Left = -4;
            Substrate.Layout.Top = -4;
            Substrate.Layout.Width = Layout.Width + 8;
            Substrate.Layout.Height = Layout.Height + _titleHeight + 8;
            DivNinecutRenderer _substrateRenderer = Substrate.BindRenderer<DivNinecutRenderer>();
            _substrateRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Substrate1" ) );
            _substrateRenderer.Cut = 6;
            base.Register( Substrate );

            Block = new Division( "Behavior" );
            Block.BindRenderer<DivPixelRenderer>();
            Block.Design.Color = new Color( 17, 18, 20 );
            Block.Layout.Top = _titleHeight;
            Block.Layout.Width = Layout.Width;
            Block.Layout.Height = Layout.Height;
            base.Register( Block );

            TitleColumn = new Division( "TitleColumn" );
            DivNinecutRenderer _tileColumnRenderer = TitleColumn.BindRenderer<DivNinecutRenderer>();
            _tileColumnRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/TitleColumn1" ) );
            _tileColumnRenderer.Cut = 6;
            TitleColumn.Interact.IsInteractive = false;
            TitleColumn.Layout.Width = Layout.Width;
            TitleColumn.Layout.Height = _titleHeight;
            {
                Icon = new Division( "Icon" );
                DivTextureRenderer _iconRenderer = Icon.BindRenderer<DivTextureRenderer>();
                _iconRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Icon1" ) );
                Icon.Interact.IsInteractive = false;
                Icon.Layout.Left = 4;
                Icon.Layout.Top = 4;
                Icon.Layout.Width = 8;
                Icon.Layout.Height = 8;
                TitleColumn.Register( Icon );

                CloseButton = new Division( "CloseButton" );
                DivTextureRenderer _closeRenderer = CloseButton.BindRenderer<DivTextureRenderer>();
                _closeRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Close1" ) );
                CloseButton.Interact.IsInteractive = true;
                CloseButton.Layout.Left = TitleColumn.Layout.Width - 16;
                CloseButton.Layout.Top = 2;
                CloseButton.Layout.Width = 14;
                CloseButton.Layout.Height = 12;
                CloseButton.Events.LeftClickBefore += ( ) =>
                {
                    _closeRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Close1_Off" ) );
                };
                CloseButton.Events.LeftClickAfter += ( ) =>
                {
                    _closeRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Close1" ) );
                    Close();
                };
                CloseButton.Events.HoverOver += ( ) =>
                {
                    _closeRenderer.Bind( TextureAssets.Get( "UserInterfaces/Forms/Close1" ) );
                };
                TitleColumn.Register( CloseButton );
            }
            base.Register( TitleColumn );

            PopupInit();

            Layout.Width += 8;
            Layout.Height += _titleHeight + 8;

            Events.KeyClickAfter += ( object s, KeyEventArgs e ) =>
            {
                if(e.Key == Keys.Escape && IsVisible)
                {
                    e.Captured = true;
                    Close();
                }
            };
            base.OnInit();
        }
        public virtual void PopupInit() { }
        public override bool Register( Division division, bool doInit = false ) => Block.Register( division, doInit );
        public void Show() => (Controller as DivGradientController).Open();
        public void Close() => (Controller as DivGradientController).Close();
    }
}