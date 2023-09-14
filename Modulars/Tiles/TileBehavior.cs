namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块行为.
    /// </summary>
    public class TileBehavior
    {
        /// <summary>
        /// 获取物块包含类名的完整命名空间.
        /// </summary>
        public string Name => GetType().Namespace + "." + GetType().Name;

        /// <summary>
        /// 指示物块纹理表类型.
        /// </summary>
        public virtual string SpriteSheetCategory => "NormalBlock";

        internal Tile _tile;
        public Tile Tile => _tile;

        internal int id = -1;
        public int ID => id;

        internal int coordinateX;
        public int CoordinateX => coordinateX;

        internal int coordinateY;
        public int CoordinateY => coordinateY;

        public TileInfo? Info => Tile?.Infos[ID];

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

        public virtual void SetDefaults() { }

        public virtual void UpdateTile( int coordinateX, int coordinateY ) { }

        public virtual void RenderTexture() { }

        public virtual void RenderBorder() { }

        public void DoRefresh( int conduct )
        {
            if(conduct > 0)
            {
                TopBehavior?.DoRefresh( conduct - 1 );
                BottomBehavior?.DoRefresh( conduct - 1 );
                LeftBehavior?.DoRefresh( conduct - 1 );
                RightBehavior?.DoRefresh( conduct - 1 );
            }
            OnRefresh();
        }

        /// <summary>
        /// 执行一次物块更新.
        /// </summary>
        public virtual void OnRefresh() { }

        /// <summary>
        /// 判断指定相对于该物块坐标具有指定偏移位置处的物块是否相同.
        /// </summary>
        /// <param name="dx">偏移的X坐标.</param>
        /// <param name="dy">偏移的Y坐标.</param>
        /// <returns></returns>
        public bool IsSame( int dx, int dy )
        {
            if(CoordinateY + dy < 0 || CoordinateY + dy >= _tile.Height ||
                CoordinateX + dx < 0 || CoordinateX + dx >= _tile.Width)
            {
                return false;
            }
            return Tile.Behaviors[CoordinateX + dx, CoordinateY + dy].Name == Name;
        }
    }
}