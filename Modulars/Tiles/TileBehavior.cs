using Colin.Core.Resources;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块行为.
    /// </summary>
    public class TileBehavior : ICodeResource
    {
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

        public void DoInitialize( ref TileInfo info )
        {
            OnInitialize( ref info );
            foreach(var script in info.Scripts.Values)
                script.OnInitialize(); //在放置时执行所有脚本的 OnPlace 方法.
        }
        /// <summary>
        /// 执行于物块初始化.
        /// </summary>
        /// <param name="info"></param>
        public virtual void OnInitialize( ref TileInfo info ) { }

        public void DoRefresh( ref TileInfo info, int conduct = 1 )
        {
            if(conduct > 0)
            {
                if(!info.Top.Empty)
                    info.Top.Behavior.DoRefresh( ref info.Top , conduct - 1 );
                if(!info.Bottom.Empty)
                    info.Bottom.Behavior.DoRefresh( ref info.Bottom, conduct - 1);
                if(!info.Left.Empty)
                    info.Left.Behavior.DoRefresh( ref info.Left, conduct - 1 );
                if(!info.Right.Empty)
                    info.Right.Behavior.DoRefresh( ref info.Right, conduct - 1 );
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
            foreach(var script in info.Scripts.Values)
                script.OnDestruction();
        }
        /// <summary>
        /// 执行于物块被破坏时.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="conduct"></param>
        public virtual void OnDestruction( ref TileInfo info, int conduct = 1 ) { }
    }
}