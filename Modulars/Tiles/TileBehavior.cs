using Colin.Core.ModLoaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    public class TileBehavior
    {
        public string Name => GetType( ).Namespace + "." + GetType().Name;

        internal Tile _tile;
        public Tile Tile => _tile;

        internal int id = -1;
        public int ID => id;

        internal int coordinateX;
        public int CoordinateX => coordinateX;

        internal int coordinateY;
        public int CoordinateY => coordinateY;

        public TileInfo? Info => Tile?.Infos[ID];

        public TileInfo? Bottom
        {
            get
            {
                if(CoordinateY + 1 <= _tile.Height - 1)
                    return Tile.Infos[CoordinateX, CoordinateY + 1];
                else
                    return null;
            }
        }

        public TileInfo? Top
        {
            get
            {
                if(CoordinateY - 1 >= 0)
                    return Tile.Infos[CoordinateX, CoordinateY - 1];
                else
                    return null;
            }
        }

        public TileInfo? Left
        {
            get
            {
                if(CoordinateX - 1 >= 0)
                    return Tile.Infos[CoordinateX - 1, CoordinateY];
                else
                    return null;
            }
        }

        public TileInfo? Right
        {
            get
            {
                if(CoordinateX + 1 <= _tile.Width - 1)
                    return Tile.Infos[CoordinateX + 1, CoordinateY];
                else
                    return null;
            }
        }

        /// <summary>
        /// 获取周围物块的状态.
        /// </summary>
        public TileAroundState AroundState
        {
            get
            {
                bool down = Bottom.HasValue ? !Bottom.Value.Empty : false;
                bool left = Left.HasValue ? !Left.Value.Empty : false;
                bool right = Right.HasValue ? !Right.Value.Empty : false;
                bool up = Top.HasValue ? !Top.Value.Empty : false;
                int result = 0;
                result += down ? 1 : 0;
                result += right ? 2 : 0;
                result += up ? 4 : 0;
                result += left ? 8 : 0;
                return (TileAroundState)result;
            }
        }

        public TileBehavior BottomBehavior
        {
            get
            {
                if(CoordinateY + 1 <= _tile?.Height - 1)
                    return Tile.Behaviors[CoordinateX, CoordinateY + 1];
                else
                    return null;
            }
        }

        public TileBehavior TopBehavior
        {
            get
            {
                if(CoordinateY - 1 >= 0)
                    return Tile.Behaviors[CoordinateX, CoordinateY - 1];
                else
                    return null;
            }
        }

        public TileBehavior LeftBehavior
        {
            get
            {
                if(CoordinateX - 1 >= 0)
                    return Tile.Behaviors[CoordinateX - 1, CoordinateY];
                else
                    return null;
            }
        }

        public TileBehavior RightBehavior
        {
            get
            {
                if(CoordinateX + 1 <= _tile?.Width - 1)
                    return Tile.Behaviors[CoordinateX + 1, CoordinateY];
                else
                    return null;
            }
        }

        public virtual void SetDefaults( ) { }
        public virtual void UpdateTile( int coordinateX, int coordinateY ) { }
        public virtual void RenderTexture( int coordinateX, int coordinateY ) { }
        public virtual void RenderBorder( int coordinateX, int coordinateY ) { }
        public void DoRefresh( int conduct )
        {
            if(conduct > 0)
            {
                TopBehavior?.DoRefresh( conduct - 1 );
                BottomBehavior?.DoRefresh( conduct - 1 );
                LeftBehavior?.DoRefresh( conduct - 1 );
                RightBehavior?.DoRefresh( conduct - 1 );
            }
            OnRefresh( );
        }
        /// <summary>
        /// 执行一次物块更新.
        /// </summary>
        public virtual void OnRefresh( ) { }
    }
}