using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
    public class TileRefresher : ISceneModule
    {
        public Scene Scene { get; set; }
        public bool Enable { get; set; }

        public Tile Tile => Scene.GetModule<Tile>();

        public Queue<Point3> RefreshQueue = new Queue<Point3>();

        public void DoInitialize()
        {
        }
        public void Start()
        {
        }
        public void DoUpdate( GameTime time )
        {
            while(RefreshQueue.Count > 0)
            {
                Point3 coord = RefreshQueue.Dequeue();
                ref TileInfo info = ref Tile[coord];
                RefreshHandle( coord );
                if(info.Empty)
                {
                    info.Behavior = null;
                    info.Scripts.Clear();
                    info.AddScript<TileArtScript>();
                }
                else
                    info.Scripts.Remove( typeof( TileArtScript ));
            }
        }
        public void RefreshMark( Point3 coord, int radius = 0 )
        {
            RefreshQueue.Enqueue( coord );
            for(int i = 1; i <= radius; i++)
            {
                for(int x = -1; x <= 1; x++)
                    for(int y = -1; y <= 1; y++)
                    {
                        if(x == 0 && y == 0)
                            continue;
                        RefreshQueue.Enqueue( new Point3( coord.X + x * i, coord.Y + y * i, coord.Z ) );
                    }
            }
        }
        public void RefreshHandle( Point3 coord )
        {
            ref TileInfo info = ref Tile[coord];
            info.Behavior?.OnRefresh( ref info );
            foreach(var script in info.Scripts.Values)
                script.OnRefresh();
        }

        public void Dispose()
        {
        }
    }
}