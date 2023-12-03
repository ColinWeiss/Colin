using System.Runtime.Serialization;

namespace Colin.Core.Graphics
{
    /// <summary>
    ///  帧格信息.
    /// <br>[!] 该信息使用每帧大小相同的帧格格式.</br>
    /// </summary>
    public struct Frame
    {
        /// <summary>
        /// 横向选取帧格; 值为帧数.
        /// </summary>
        public int X;

        /// <summary>
        /// 纵向选取帧格; 值为帧数.
        /// </summary>
        public int Y;

        public Point Size => new Point( Width, Height );

        public Vector2 SizeF => new Vector2( Width, Height );

        public Point Half => new Point( Width / 2, Height / 2 );

        public Vector2 HalfF => new Vector2( Width / 2, Height / 2 );

        /// <summary>
        /// 帧格宽.
        /// </summary>
        public int Width;

        /// <summary>
        /// 帧格高.
        /// </summary>
        public int Height;

        /// <summary>
        /// 指示是否播放帧图.
        /// </summary>
        public bool IsPlay;

        /// <summary>
        /// 起始帧.
        /// </summary>
        public int Start;

        /// <summary>
        /// 当前帧.
        /// </summary>
        public int Current;

        /// <summary>
        /// 帧上限.
        /// </summary>
        public int FrameMax;

        /// <summary>
        /// 帧切换时间.
        /// </summary>
        public float Interval;

        /// <summary>
        /// 帧计时器.
        /// </summary>
        public float Timer;

        /// <summary>
        /// 指示播放是否为循环模式.
        /// </summary>
        public bool IsLoop;

        /// <summary>
        /// 指示该帧格读取的方向.
        /// </summary>
        public Direction Direction;

        /// <summary>
        /// 获取帧格.
        /// </summary>
        public Rectangle GetFrame()
        {
            switch(Direction)
            {
                case Direction.Portrait:
                    return new Rectangle( X * Width, Current * Height, Width, Height );
                case Direction.Transverse:
                    return new Rectangle( Current * Width, Y * Height, Width, Height );
            };
            return Rectangle.Empty;
        }

        /// <summary>
        /// 为帧格选取提供逻辑刷新.
        /// </summary>
        public void UpdateFrame()
        {
            if(!IsPlay)
                return;
            Timer += Time.DeltaTime;
            if(Timer > Interval)
            {
                Timer = 0;
                if(Current < FrameMax + Start)
                    Current++;
                else if(IsLoop)
                    Current = Start;
            }
        }
    }
}