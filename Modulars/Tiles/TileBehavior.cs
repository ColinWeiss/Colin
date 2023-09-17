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

        public void DoPlace( ref TileInfo info )
        {
            OnPlace( ref info );
            foreach(var script in info.Scripts.Values)
                script.OnPlace(); //在放置时执行所有脚本的 OnPlace 方法.
        }
        /// <summary>
        /// 执行于物块放置时.
        /// </summary>
        public virtual void OnPlace( ref TileInfo info ) { }

        public void DoRefresh( ref TileInfo info, int conduct = 1 )
        {
            if(conduct > 0)
            {
                if(!info.Top.Empty)
                    Console.WriteLine( info.Coordinate );//  info.Top.Behavior.DoRefresh( ref info.Top );
                if(!info.Bottom.Empty) 
                   Console.WriteLine( info.Coordinate );//  info.Bottom.Behavior.DoRefresh( ref info.Bottom );
                if(!info.Left.Empty) 
                    Console.WriteLine( info.Coordinate );// info.Left.Behavior.DoRefresh( ref info.Left );
                if(!info.Right.Empty) 
                   Console.WriteLine( info.Coordinate );//  info.Right.Behavior.DoRefresh( ref info.Right );
            }
            OnRefresh( ref info, conduct );
            foreach(var script in info.Scripts.Values)
                script.OnRefresh();
        }
        /// <summary>
        /// 执行于物块刷新时.
        /// </summary>
        public virtual void OnRefresh( ref TileInfo info, int conduct = 1 ) { }

        public void DoDestruction( ref TileInfo info )
        {
            OnDestruction( ref info );
        }
        /// <summary>
        /// 执行于物块被破坏时.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="conduct"></param>
        public virtual void OnDestruction( ref TileInfo info, int conduct = 1 ) { }
    }
}