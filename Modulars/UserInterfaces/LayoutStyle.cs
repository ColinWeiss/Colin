namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 为划分元素指定布局样式.
    /// </summary>
    public struct LayoutStyle
    {
        public bool IsHidden;

        private bool _needRefreshSizeRelative;
        private bool _needRefreshLocationRelative;

        public int PaddingLeft;

        public int PaddingTop;

        public int Left;
        public int TotalLeft;
        private float _relativeLeft;
        public float RelativeLeft
        {
            get
            {
                return _relativeLeft;
            }
            set
            {
                _relativeLeft = value;
                _needRefreshLocationRelative = true;
            }
        }

        public int Top;
        public int TotalTop;
        private float _relativeTop;
        public float RelativeTop
        {
            get
            {
                return _relativeTop;
            }
            set
            {
                _relativeTop = value;
                _needRefreshLocationRelative = true;
            }
        }

        public Point Location
        {
            get => new Point( Left, Top );
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }
        public Point TotalLocation => new Point( TotalLeft, TotalTop );
        public Vector2 LocationF
        {
            get =>new Vector2( Left, Top );
            set
            {
                Left = (int)value.X;
                Top = (int)value.Y;
            }
        }
        public Vector2 TotalLocationF => new Vector2( TotalLeft, TotalTop );

        public int Width;
        private float _relativeWidth;
        public float RelativeWidth
        {
            get
            {
                return _relativeWidth;
            }
            set
            {
                _relativeWidth = value;
                _needRefreshSizeRelative = true;
            }
        }

        public int Height;
        private float _relativeHeight;
        public float RelativeHeight
        {
            get
            {
                return _relativeHeight;
            }
            set
            {
                _relativeHeight = value;
                _needRefreshSizeRelative = true;
            }
        }

        public Point Size
        {
            get => new Point( Width, Height );
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public Vector2 SizeF
        {
            get => new Vector2( Width, Height );
            set
            {
                Width = (int)value.X;
                Height = (int)value.Y;
            }
        }

        public Point Half => new Point( Width / 2, Height / 2 );
        public Vector2 HalfF => new Vector2( Width / 2, Height / 2 );

        public Rectangle HitBox => new Rectangle( Left, Top, Width, Height );

        public Rectangle TotalHitBox => new Rectangle( TotalLeft, TotalTop, Width, Height );

        public Rectangle InputBox;

        public Matrix CanvasTransform =>
                Matrix.CreateTranslation( new Vector3( -new Vector2( TotalLeft, TotalTop ), 0f ) ) *
                Matrix.CreateScale( 1f ) *
                Matrix.CreateRotationZ( 0f ) *
                Matrix.CreateTranslation( new Vector3( Vector2.Zero, 0f ) );

        /// <summary>
        /// 启用剪裁.
        /// </summary>
        public bool ScissorEnable;

        private bool scissorDefault;
        private Rectangle _scissor;
        /// <summary>
        /// 指示要进行剪裁的范围.
        /// </summary>
        public Rectangle Scissor
        {
            get
            {
                if(_scissor == Rectangle.Empty)
                    scissorDefault = true;
                return _scissor;
            }
            set
            {
                scissorDefault = false;
                _scissor = value;
            }
        }

        public bool IsCanvas { get; internal set; }

        /// <summary>
        /// 对样式进行计算.
        /// </summary>
        /// <param name="parent"></param>
        public void Calculation( LayoutStyle parent )
        {
            TotalLeft = parent.TotalLeft + Left + parent.PaddingLeft;
            TotalTop = parent.TotalTop + Top + parent.PaddingTop;
            if(_needRefreshSizeRelative)
            {
                Width = (int)(parent.Width * RelativeWidth);
                Height = (int)(parent.Height * RelativeHeight);
                _needRefreshSizeRelative = false;
            }
            if(_needRefreshLocationRelative)
            {
                Left = (int)(parent.Left * RelativeLeft);
                Top = (int)(parent.Top * RelativeTop);
                _needRefreshLocationRelative = false;
            }
            if(ScissorEnable && scissorDefault)
            {
                _scissor = TotalHitBox;
                if(parent.IsCanvas)
                    _scissor = HitBox;
            }
            if(IsCanvas)
            {
                _scissor.X = 0;
                _scissor.Y = 0;
            }
        }
    }
}