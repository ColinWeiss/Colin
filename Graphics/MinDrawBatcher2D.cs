using Colin.Core.Utilities;
#nullable enable
namespace Colin.Core.Graphics
{
    /// <summary>
    /// A mini batcher that no need to Dispose(). 
    /// Call <see cref="Flush()"/> to draw on the screen.
    /// </summary>
    /// <typeparam name="T">Vertex Type</typeparam>
    public unsafe class MinDrawBatcher2D<T> : IDrawBatcher<T> where T : unmanaged, IVertexType
    {
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        readonly GraphicsDevice _graphicsDevice;
        VQueues<T>[] _queues;
        T[] _cache;
        Dictionary<int, Texture2D> _textures;
        int _cursor;
        public MinDrawBatcher2D(GraphicsDevice graphicsDevice, int initCapacity = 4 * 100)
        {
            _graphicsDevice = graphicsDevice;
            _queues = new VQueues<T>[initCapacity];
            _cache = new T[initCapacity * 3 / 2];
            _textures = new Dictionary<int, Texture2D>( 10 );
        }
        /// <summary>
        /// vul---vur
        ///  |  \  |
        /// vdl---vdr
        /// </summary>
        /// <param name="sortingKey">Used by <see cref="Sort()"/></param>
        public void DrawQuad(Texture2D texture, T vul, T vur, T vdr, T vdl, int sortingKey = -1)
        {
            EnsureCapacity( _cursor + 1 );
            //notice that &(texture._sortingKey) - &(texture) = 72
            sortingKey = sortingKey == -1 ? UnsafeObject.As( texture ).GetUmanagedField<int>( 72 ) : sortingKey;
            //sortingKey = sortingKey == -1 ? UnsafeObject.As(texture).GetField<int>(typeof(Texture).GetField("_sortingKey", BindingFlags.NonPublic | BindingFlags.Instance)) : sortingKey;
            fixed (VQueues<T>* queuePtr = _queues)
            {
                *(int*)(queuePtr + _cursor) = sortingKey;
                ((T*)((int*)(queuePtr + _cursor) + 1))[0] = vul;
                ((T*)((int*)(queuePtr + _cursor) + 1))[1] = vur;
                ((T*)((int*)(queuePtr + _cursor) + 1))[2] = vdr;
                ((T*)((int*)(queuePtr + _cursor) + 1))[3] = vdl;
            }
            //equals to _queues[_cursor] = new VQueues<T>(sortingKey, vul, vur, vdr, vdl);
            if (!_textures.ContainsKey( sortingKey )) _textures.Add( sortingKey, texture );
            _cursor++;
        }
        /// <summary>
        /// Sort DrawQueues by sortingKey.
        /// </summary>
        public void Sort()
        {
            if (_cursor == 0) return;
            Array.Sort( _queues, 0, _cursor, new TexComparer<T>() );
        }
        /// <summary>
        /// Apply the provided pass and flush DrawQueues.
        /// </summary>
        public void Flush(EffectPass? pass, bool clear = true)
        {
            if (_cursor == 0) return;
            pass?.Apply();
            Flush( clear );
        }
        /// <summary>
        /// Apply the provided effect and flush DrawQueues.
        /// </summary>
        public void Flush(Effect effect, bool clear = true)
        {
            if (_cursor == 0) return;
            foreach (var p in effect.CurrentTechnique.Passes) p.Apply();
            Flush( clear );
        }
        private void Flush(bool clear)
        {
            if (_cache.Length < _cursor * 6) _cache = new T[_cursor * 6];
            fixed (T* cachePtr = _cache)
            {
                fixed (VQueues<T>* queuePtr = _queues)
                {
                    int texture = queuePtr[0].Texture;
                    int pCount = 0;
                    for (int i = 0; i < _cursor; i++)
                    {
                        if (texture != queuePtr[i].Texture)
                        {
                            _graphicsDevice.Textures[0] = _textures[texture];
                            _graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _cache, 0, pCount / 3 );
                            pCount = 0;
                            texture = queuePtr[i].Texture;
                        }
                        //ul---ur
                        //|  \  |
                        //dl---dr
                        cachePtr[pCount] = queuePtr[i].Vul;
                        cachePtr[pCount + 1] = queuePtr[i].Vur;
                        cachePtr[pCount + 2] = queuePtr[i].Vdr;
                        cachePtr[pCount + 3] = queuePtr[i].Vul;
                        cachePtr[pCount + 4] = queuePtr[i].Vdr;
                        cachePtr[pCount + 5] = queuePtr[i].Vdl;
                        pCount += 6;
                    }
                    _graphicsDevice.Textures[0] = _textures[texture];
                    _graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _cache, 0, pCount / 3 );
                }
            }
            if (clear) _cursor = 0;
        }
        private void EnsureCapacity(int target)
        {
            if (_queues.Length < target)
            {
                int l = _queues.Length * 3 / 2;
                for (; l < target; l = l * 3 / 2) ;
                var temp = new VQueues<T>[l];
                Array.Copy( _queues, temp, _queues.Length );
                _queues = temp;
            }
        }
        private struct VQueues<TVert> where TVert : unmanaged, IVertexType
        {
            public int Texture;
            public TVert Vul;
            public TVert Vur;
            public TVert Vdr;
            public TVert Vdl;
            public VQueues(int texture, TVert vul, TVert vur, TVert vdr, TVert vdl)
            {
                Texture = texture;
                Vul = vul;
                Vur = vur;
                Vdr = vdr;
                Vdl = vdl;
            }
        }
        private class TexComparer<TVert> : IComparer<VQueues<TVert>> where TVert : unmanaged, IVertexType
        {
            public int Compare(VQueues<TVert> x, VQueues<TVert> y)
            {
                return x.Texture - y.Texture;
            }
        }
    }
}