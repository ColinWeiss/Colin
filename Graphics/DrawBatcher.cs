using Colin.Core.Utilities;
using System.Buffers;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
namespace Colin.Core.Graphics
{
    public unsafe class DrawBatcher2D<T> : IDrawBatcher<T>, IDisposable where T : unmanaged, IVertexType
    {
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        readonly GraphicsDevice _graphicsDevice;

        T[] _primBuffer;
        T* _primBufferPtr;
        MemoryHandle _primBufferHandle;

        int[] _indexCache;
        int* _indexCachePtr;
        MemoryHandle _indexCacheHandle;

        VSpanQueue[] _queues;
        VSpanQueue* _queuesPtr;
        MemoryHandle _queuesHandle;

        int _primBufferCursor;
        int _queuesCursor;

        Dictionary<int, Texture2D> _textures;
        DynamicVertexBuffer _vertexBuffer;
        DynamicIndexBuffer _indexBuffer;

        bool _isDisposed;
        public DrawBatcher2D(GraphicsDevice graphicsDevice, int initCapacity = 4 * 100)
        {
            if (initCapacity < 10) throw new ArgumentOutOfRangeException();
            _graphicsDevice = graphicsDevice;
            _queues = new VSpanQueue[initCapacity];
            _queuesHandle = _queues.AsMemory().Pin();
            _queuesPtr = (VSpanQueue*)_queuesHandle.Pointer;
            _textures = new Dictionary<int, Texture2D>(10);
            AllocateBuffer(initCapacity);
        }
        public void DrawQuad(Texture2D texture, T vul, T vur, T vdr, T vdl, int sortingKey = -1)
        {
            int texKey = UnsafeObject.As(texture).GetUmanagedField<int>(72);

            if (_queuesCursor == 0 || !(texKey == _queuesPtr[_queuesCursor - 1].Texture && sortingKey == _queuesPtr[_queuesCursor - 1].SortingKey))
            {
                if (_queuesCursor >= _queues.Length) ResizeQueueList(_queues.Length * 3 / 2);
                ((int*)(_queuesPtr + _queuesCursor))[0] = sortingKey;
                ((int*)(_queuesPtr + _queuesCursor))[1] = texKey;
                ((int*)(_queuesPtr + _queuesCursor))[2] = _primBufferCursor;
                ((int*)(_queuesPtr + _queuesCursor))[3] = _primBufferCursor + 4;
                //equals to: _queuesPtr[_queueCursor] = new VSpanQueue(sortingKey, texKey, _primBufferCursor, _primBufferCursor + 4);
                _queuesCursor++;
            }
            else _queuesPtr[_queuesCursor - 1].End += 4;

            //ul---ur
            //|  \  |
            //dl---dr
            if (_primBufferCursor + 4 > _primBuffer.Length) AllocateBuffer(_primBuffer.Length * 3 / 2);
            _primBufferPtr[_primBufferCursor++] = vul;
            _primBufferPtr[_primBufferCursor++] = vur;
            _primBufferPtr[_primBufferCursor++] = vdr;
            _primBufferPtr[_primBufferCursor++] = vdl;

            if (!_textures.ContainsKey(texKey)) _textures.Add(texKey, texture);
        }
        /// <summary>
        /// Sort DrawQueues by sortingKey.
        /// </summary>
        public void Sort()
        {
            if (_queuesCursor == 0) return;
            Array.Sort(_queues, 0, _queuesCursor, new QueueComparer());
        }
        public void Flush(EffectPass? pass, bool clear = true)
        {
            if (_queuesCursor == 0) return;
            pass?.Apply();
            InternalFlush(clear);
        }
        public void Flush(Effect effect, bool clear = true)
        {
            if (_queuesCursor == 0) return;
            foreach (var p in effect.CurrentTechnique.Passes) p.Apply();
            InternalFlush(clear);
        }
        private void InternalFlush(bool clear)
        {
            _graphicsDevice.Indices = _indexBuffer;
            var indexCount = BuildIndex();
            _indexBuffer.SetData(_indexCache, 0, indexCount, SetDataOptions.None);
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _vertexBuffer.SetData(_primBuffer, 0, _primBufferCursor, SetDataOptions.Discard);
            DrawBatched();
            if (!clear) return;
            _queuesCursor = 0;
            _primBufferCursor = 0;
        }
        private int BuildIndex()
        {
            var indexCount = 0;
            for (int i = 0; i < _queuesCursor; i++)
            {
                if (_queuesPtr[i].End - _queuesPtr[i].First == 4)
                {
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 0;
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 1;
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 2;
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 0;
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 2;
                    _indexCachePtr[indexCount++] = _queuesPtr[i].First + 3;
                }
                else for (int j = _queuesPtr[i].First; j < _queuesPtr[i].End; j += 4)
                    {
                        //ul0---ur1
                        //|  \  |
                        //dl3---dr2
                        _indexCachePtr[indexCount++] = j + 0;
                        _indexCachePtr[indexCount++] = j + 1;
                        _indexCachePtr[indexCount++] = j + 2;
                        _indexCachePtr[indexCount++] = j + 0;
                        _indexCachePtr[indexCount++] = j + 2;
                        _indexCachePtr[indexCount++] = j + 3;
                    }
            }
            return indexCount;
        }
        private void DrawBatched()
        {
            var texture = _queuesPtr[0].Texture;
            var indexCount = 0;
            int indexDelta = 0;
            for (var qptr = _queuesPtr; qptr < _queuesPtr + _queuesCursor; qptr++)
            {
                if (texture != qptr->Texture)
                {
                    _graphicsDevice.Textures[0] = _textures[texture];
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, indexCount - indexDelta, indexDelta / 3);
                    texture = qptr->Texture;
                    indexDelta = 0;
                }
                indexCount += (qptr->End - qptr->First) * 3 / 2;
                indexDelta += (qptr->End - qptr->First) * 3 / 2;
            }
            _graphicsDevice.Textures[0] = _textures[texture];
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, indexCount - indexDelta, indexDelta / 3);

        }
        private void ResizeQueueList(int size)
        {
            _queuesHandle.Dispose();
            Array.Resize(ref _queues, size);
            _queuesHandle = _queues.AsMemory().Pin();
            _queuesPtr = (VSpanQueue*)_queuesHandle.Pointer;
        }
        private void AllocateBuffer(int size)
        {
            if (_primBuffer != null)
            {
                _primBufferHandle.Dispose();
                Array.Resize(ref _primBuffer, size);
                _primBufferHandle = _primBuffer.AsMemory().Pin();
                _primBufferPtr = (T*)_primBufferHandle.Pointer;

                _indexCacheHandle.Dispose();
                _indexCache = new int[size * 3 / 2];
                _indexCacheHandle = _indexCache.AsMemory().Pin();
                _indexCachePtr = (int*)_indexCacheHandle.Pointer;

                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
                _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(T), size, BufferUsage.WriteOnly);
                _indexBuffer = new DynamicIndexBuffer(_graphicsDevice, typeof(int), size * 3 / 2, BufferUsage.WriteOnly);
            }
            else
            {
                _primBuffer = new T[size];
                _primBufferHandle = _primBuffer.AsMemory().Pin();
                _primBufferPtr = (T*)_primBufferHandle.Pointer;

                _indexCache = new int[size * 3 / 2];
                _indexCacheHandle = _indexCache.AsMemory().Pin();
                _indexCachePtr = (int*)_indexCacheHandle.Pointer;

                _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(T), size, BufferUsage.WriteOnly);
                _indexBuffer = new DynamicIndexBuffer(_graphicsDevice, typeof(int), size * 3 / 2, BufferUsage.WriteOnly);
            }
        }
        private struct VSpanQueue
        {
            public int SortingKey;
            public int Texture;
            public int First;
            public int End;
            public VSpanQueue(int sortingKey, int texture, int first, int end)
            {
                SortingKey = sortingKey;
                Texture = texture;
                First = first;
                End = end;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _primBufferHandle.Dispose();
                    _indexCacheHandle.Dispose();
                    _queuesHandle.Dispose();
                    if (!_vertexBuffer.IsDisposed) _vertexBuffer.Dispose();
                    if (!_indexBuffer.IsDisposed) _indexBuffer.Dispose();
                }
                _primBuffer = null;
                _indexCache = null;
                _queues = null;

                _isDisposed = true;
            }
        }
        ~DrawBatcher2D()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        private class QueueComparer : IComparer<VSpanQueue>
        {
            public int Compare(VSpanQueue x, VSpanQueue y)
            {
                if (x.SortingKey == y.SortingKey) return x.Texture - y.Texture;
                else return x.SortingKey - y.SortingKey;
            }
        }
    }
}
