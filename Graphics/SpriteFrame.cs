using System.Runtime.Serialization;

namespace Colin.Core.Graphics
{
    /// <summary>
    /// Sprite等大帧格.
    /// <br>[!] 即 每帧大小相同的帧格格式.</br>
    /// </summary>
    [Serializable]
    public struct SpriteFrame
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
        public int FrameCountStart;

        /// <summary>
        /// 当前帧.
        /// </summary>
        [DataMember]
        public int FrameCount;

        /// <summary>
        /// 帧上限.
        /// </summary>
        [DataMember]
        public int FrameCountMax;

        /// <summary>
        /// 帧切换时间.
        /// </summary>
        [DataMember]
        public int IntervalTime;

        /// <summary>
        /// 帧计时器.
        /// </summary>
        [DataMember]
        public int FrameTimer;

        /// <summary>
        /// 指示该帧格读取的方向.
        /// </summary>
        [DataMember]
        public Direction FrameDirection;

        /// <summary>
        /// 为帧格选取提供逻辑刷新.
        /// </summary>
        public void UpdateFrame( )
        {
            if( !IsPlay )
                return;
            FrameTimer++;
            if( FrameTimer % IntervalTime == 0 && FrameTimer != 0 )
            {
                FrameTimer = 0;
                FrameCount++;
                if( FrameCount > FrameCountMax )
                    FrameCount = FrameCountStart;
            }
        }

        public Rectangle Frame
        {
            get
            {
                switch( FrameDirection )
                {
                    case Direction.Portrait:
                        return new Rectangle( X * Width, Y * Height * FrameCount, Width, Height );
                    case Direction.Transverse:
                        return new Rectangle( X * Width * FrameCount, Y * Height, Width, Height );
                };
                return Rectangle.Empty;
            }
        }



    }
}