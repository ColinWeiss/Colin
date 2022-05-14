using Colin.Graphics;
using Colin.Localizations;
using Microsoft.Xna.Framework;

namespace Colin.Core.Tiled
{
    /// <summary>
    /// 一块单独的瓦片.
    /// </summary>
    public class Tile : ILocalizable, IEmptyState, IAnimation
    {
        public bool Empty { get; set; } = true;

        /// <summary>
        /// 获取该物块在所属区块中的横坐标.
        /// </summary>
        public int CoordinateX { get; private set; } = -1;

        /// <summary>
        /// 获取该物块在所属区块中的纵坐标.
        /// </summary>
        public int CoordinateY { get; private set; } = -1;

        /// <summary>
        /// 获取该瓦片所在的区块.
        /// </summary>
        public Chunk? Chunk { get; internal set; }

        public int CurrentFrameX { get; protected set; }

        public int CurrentFrameY { get; protected set; }

        public Rectangle Frame { get; protected set; }

        public virtual Rectangle GetFrame( )
        {
            return new Rectangle( CurrentFrameX * Frame.Width, CurrentFrameY * Frame.Height, Frame.Width, Frame.Height );
        }

        /// <summary>
        /// 在该物块所绑定的区块中的指定坐标放置该物块.
        /// </summary>
        public void Place( int coordinateX, int coordinateY )
        {
            Empty = false;
            CoordinateX = coordinateX;
            CoordinateY = coordinateY;
            Chunk.Tiles[ coordinateX, coordinateY ] = this;
            ModifyOnPlace( );
        }
        /// <summary>
        /// 在该物块被放置时执行.
        /// </summary>
        protected virtual void ModifyOnPlace( ) { }

        /// <summary>
        /// 破坏该物块.
        /// </summary>
        public void Destruction( )
        {
            Chunk.Tiles[ CoordinateX, CoordinateY ].Empty = true;
            ModifyOnDestruction( );
        }
        /// <summary>
        /// 在该物块被破坏时执行.
        /// </summary>
        protected virtual void ModifyOnDestruction( ) { }

        public virtual string GetName => "Tile";

        public virtual string GetInformation => "A tile.";
    }
}