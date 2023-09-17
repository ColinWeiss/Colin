namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块行为.
    /// </summary>
    public class TileBehavior
    {
        /// <summary>
        /// 获取物块行为包含类名的完整命名空间.
        /// </summary>
        public string Identifier => (GetType().Namespace + "." + GetType().Name);

        public Tile Tile { get; internal set; }

        public virtual void DoPlace( TileInfo info )
        {
            OnPlace( info );
            foreach(var script in info.Scripts.Values)
                script.OnPlace(); //在放置时执行所有脚本的 OnPlace 方法.
        }
        /// <summary>
        /// 执行于物块放置时.
        /// </summary>
        public virtual void OnPlace( TileInfo info ) { }

        public void DoRefresh( TileInfo info, int conduct = 1 )
        {
            if(conduct > 0)
            {
           //    Top?.DoRefresh( info, conduct - 1 );
           //    Bottom?.DoRefresh( info, conduct - 1 );
           //    Left?.DoRefresh( info, conduct - 1 );
           //    Right?.DoRefresh( info, conduct - 1 );
            }
            OnRefresh( info , conduct );
            foreach(var script in info.Scripts.Values)
                script.OnRefresh();
        }
        /// <summary>
        /// 执行于物块刷新时.
        /// </summary>
        public virtual void OnRefresh( TileInfo info, int conduct = 1 ) { }
    }
}