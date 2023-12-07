using SharpDX.Direct3D9;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
    public class TileRefresher : ISceneModule
    {
        public Scene Scene { get; set; }
        public bool Enable { get; set; }

        public Tile Tile => Scene.GetModule<Tile>();

        public ConcurrentQueue<Point3> RefreshQueue = new();

        /// <summary>
        /// 用于模块之间的联动事件，如渲染器的刷新
        /// </summary>
        public Action<Point3> RefreshEvent = null;

        public void DoInitialize()
        {
        }

        public void Start()
        {
        }

        public void DoUpdate(GameTime time)
        {
            while (!RefreshQueue.IsEmpty)
            {
                RefreshQueue.TryDequeue(out Point3 coord);
                ref TileInfo info = ref Tile[coord];
                RefreshHandle(coord);
                if (info.Empty)
                {
                    info.Behavior = null;
                    info.Scripts.Clear();
                }
            }
        }

        public void RefreshMark(Point3 coord, int radius = 0)
        {
            for (int x = -radius; x <= radius; x++)
                for (int y = -radius; y <= radius; y++)
                    RefreshQueue.Enqueue(new Point3(coord.X + x, coord.Y + y, coord.Z));
        }

        public void RefreshHandle(Point3 coord)
        {
            ref TileInfo info = ref Tile[coord];
            info.Behavior?.OnRefresh(ref info);
            foreach (var script in info.Scripts.Values)
                script.OnRefresh();
            RefreshEvent?.Invoke(coord);
        }

        public void Dispose()
        {
            RefreshEvent = null;
        }
    }
}