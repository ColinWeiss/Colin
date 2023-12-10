﻿using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Prefabs;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Forms
{
    public class Form : Canvas
    {
        public Division Substrate;

        public Division TitleColumn;
        private Division _titleColumnDec;
        public Division Icon;
        private int _titleHeight;

        public Label TitleLabel;

        public Division CloseButton;

        public Division Block;

        public string Title
        {
            get => TitleLabel.FontRenderer.Text;
            set => TitleLabel.SetText(value);
        }

        private bool _isTransparent;
        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                _isTransparent = value;
                if (!_isTransparent)
                    Block.BindRenderer<DivPixelRenderer>();
                else
                    Block.ClearRenderer();
            }
        }

        public Form(string name, int width, int height, int titleHeight) : base(name)
        {
            Layout.Width = width;
            Layout.Height = height;
            _titleHeight = titleHeight;
        }
        public override sealed void OnInit()
        {
            Design.Scale = Vector2.One;
            Design.Color = Color.White;

            Controller = new DivGradientController(this);
            Interact.IsDraggable = true;
            Interact.IsSelectable = false;
            Interact.IsInteractive = true;

            Layout.PaddingLeft = 4;
            Layout.PaddingTop = 4;

            Substrate = new Division("Substrate");
            Substrate.Interact.IsInteractive = false;
            Substrate.Layout.Left = -4;
            Substrate.Layout.Top = -4;
            Substrate.Layout.Width = Layout.Width + 8;
            Substrate.Layout.Height = Layout.Height + _titleHeight + 8;
            DivNinecutRenderer _substrateRenderer = Substrate.BindRenderer<DivNinecutRenderer>();
            _substrateRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Substrate0"));
            _substrateRenderer.Cut = new Point(4, 4);
            base.Register(Substrate);

            Block = new Division("Behavior");
            if (!IsTransparent)
                Block.BindRenderer<DivPixelRenderer>();
            Block.Design.Color = new Color(24, 25, 28);
            Block.Layout.Top = _titleHeight;
            Block.Layout.Width = Layout.Width;
            Block.Layout.Height = Layout.Height;
            base.Register(Block);

            TitleColumn = new Division("TitleColumn");
            TitleColumn.BindRenderer<DivPixelRenderer>();
            TitleColumn.Interact.IsInteractive = false;
            TitleColumn.Design.Color = new Color(20, 22, 25);
            TitleColumn.Layout.Width = Layout.Width;
            TitleColumn.Layout.Height = _titleHeight;
            {
                _titleColumnDec = new Division("TitleColumn.Decoration");
                DivNinecutRenderer _decRenderer = _titleColumnDec.BindRenderer<DivNinecutRenderer>();
                _decRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Decoration"));
                _decRenderer.Cut = new Point(8, 8);
                _titleColumnDec.Interact.IsInteractive = false;
                _titleColumnDec.Layout.Width = TitleColumn.Layout.Width;
                _titleColumnDec.Layout.Height = TitleColumn.Layout.Height;
                TitleColumn.Register(_titleColumnDec);

                Icon = new Division("Icon");
                DivTextureRenderer _iconRenderer = Icon.BindRenderer<DivTextureRenderer>();
                _iconRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Icon0"));
                Icon.Interact.IsInteractive = true;
                Icon.Layout.Left = 8;
                Icon.Layout.Top = 6;
                Icon.Layout.Width = 24;
                Icon.Layout.Height = 24;
                TitleColumn.Register(Icon);

                TitleLabel = new Label("TitleLabel");
                TitleLabel.SetText("标题");
                TitleLabel.Layout.Left = Icon.Layout.Left + Icon.Layout.Width + 8;
                TitleLabel.FontRenderer.Font = FontAssets.Get("GlowSans").GetFont(32);
                TitleLabel.Design.Color = new Color(255, 223, 135);
                TitleColumn.Register(TitleLabel);

                CloseButton = new Division("CloseButton");
                DivTextureRenderer _closeRenderer = CloseButton.BindRenderer<DivTextureRenderer>();
                _closeRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Close0"));
                CloseButton.Interact.IsInteractive = true;
                CloseButton.Layout.Left = TitleColumn.Layout.Width - 32;
                CloseButton.Layout.Top = 6;
                CloseButton.Layout.Width = 24;
                CloseButton.Layout.Height = 24;
                CloseButton.Events.LeftClickBefore += () =>
                {
                    _closeRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Close0_Off"));
                };
                CloseButton.Events.LeftClickAfter += () =>
                {
                    _closeRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Close0"));
                    Close();
                };
                CloseButton.Events.HoverOver += () =>
                {
                    _closeRenderer.Bind(TextureAssets.Get("UserInterfaces/Forms/Close0"));
                };
                TitleColumn.Register(CloseButton);
            }

            Division titleLine = new Division("TitleLine");
            titleLine.Layout.Width = TitleColumn.Layout.Width;
            titleLine.Layout.Height = 2;
            titleLine.Design.Color = new Color(51, 45, 31);
            titleLine.BindRenderer<DivPixelRenderer>();
            Register(titleLine);

            base.Register(TitleColumn);

            FormInit();

            Layout.Width += 8;
            Layout.Height += _titleHeight + 8;

            Events.LeftClickBefore += () => Interface.Container.SetTop(this);
            Events.KeyClickBefore += (object s, KeyEventArgs e) =>
            {
                if (e.Key == Keys.Escape && IsVisible)
                {
                    e.Captured = true;
                    Close();
                }
            };
            base.OnInit();
        }
        public virtual void FormInit() { }
        public override bool Register(Division division, bool doInit = false) => Block.Register(division, doInit);

        public event Action OnOpen;
        public event Action OnFirstShow;

        public event Action OnClose;

        private bool _firstShow = false;
        public void Show()
        {
            OnOpen?.Invoke();
            if (!_firstShow)
            {
                OnFirstShow?.Invoke();
                _firstShow = true;
            }
            (Controller as DivGradientController).Open();
        }
        public void Close()
        {
            OnClose?.Invoke();
            (Controller as DivGradientController).Close();
        }
    }
}