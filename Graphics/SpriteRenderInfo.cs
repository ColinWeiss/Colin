using System.Runtime.Serialization;

namespace Colin.Core.Graphics
{
    /// <summary>
    /// Sprite渲染信息.
    /// <br>[!] 该信息使用每帧大小相同的帧格格式.</br>
    /// </summary>
    [Serializable]
    public struct SpriteRenderInfo
    {
        /// <summary>
        /// 横向选取帧格; 值为帧数.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// 纵向选取帧格; 值为帧数.
        /// </summary>
        [DataMember]
        public int Y;

        public Point Size => new Point( X, Y );

        public Vector2 SizeF => new Vector2( X, Y );

        /// <summary>
        /// 帧格宽.
        /// </summary>
        [DataMember]
        public int Width;

        /// <summary>
        /// 帧格高.
        /// </summary>
        [DataMember]
        public int Height;

        /// <summary>
        /// 指示是否播放帧图.
        /// </summary>
        [DataMember]
        public bool IsPlay;

        /// <summary>
        /// 起始帧.
        /// </summary>
        [DataMember]
        public int Start;

        /// <summary>
        /// 当前帧.
        /// </summary>
        [DataMember]
        public int Current;

        /// <summary>
        /// 帧上限.
        /// </summary>
        [DataMember]
        public int FrameMax;

        /// <summary>
        /// 帧切换时间.
        /// </summary>
        [DataMember]
        public float Interval;

        /// <summary>
        /// 帧计时器.
        /// </summary>
        [DataMember]
        public float Timer;

        /// <summary>
        /// 指示该帧格读取的方向.
        /// </summary>
        [DataMember]
        public Direction Direction;

        /// <summary>
        /// 为帧格选取提供逻辑刷新.
        /// </summary>
        public void UpdateFrame( )
        {
            if( !IsPlay )
                return;
            Timer += Time.DeltaTime;
            if( Timer > Interval )
            {
                Timer = 0;
                Current++;
                if( Current > FrameMax + Start)
                    Current = Start;
            }
        }
        public Rectangle Frame
        {
            get
            {
                switch( Direction )
                {
                    case Direction.Portrait:
                        return new Rectangle( X * Width, Y * Height * Current, Width, Height );
                    case Direction.Transverse:
                        return new Rectangle( X * Width * Current, Y * Height, Width, Height );
                };
                return Rectangle.Empty;
            }
        }
    }
}