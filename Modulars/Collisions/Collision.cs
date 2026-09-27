namespace Colin.Core.Modulars.Collisions
{
  /// <summary>
  /// 碰撞检测模块.
  /// </summary>
  public class Collision : ISceneModule
  {
    public Scene Scene { get; set; }

    public bool Enable { get; set; }

    public Dictionary<string, byte> LayerIdentifiers = new Dictionary<string, byte>();
    public byte GetLayer(string layerName)
      => LayerIdentifiers.GetValueOrDefault(layerName);

    public void AddLayer(string layerName)
    {
      LayerIdentifiers.Add(layerName, (byte)LayerIdentifiers.Count);
      ColliderLayers.Add(new List<Collider>());
      _layersVersion++;
    }

    public List<List<Collider>> ColliderLayers = new List<List<Collider>>();

    /// <summary>
    /// 添加碰撞体至模块列表.
    /// </summary>
    /// <param name="collider"></param>
    public bool AddCollider(Collider collider, string layerName = "Default Layer")
    {
      _layersVersion++;
      if (string.IsNullOrEmpty(collider.LayerName) || string.IsNullOrWhiteSpace(collider.LayerName))
        collider.LayerName = layerName;
      if (LayerIdentifiers.ContainsKey(collider.LayerName))
      {
        collider.Layer = GetLayer(collider.LayerName);
        ColliderLayers[collider.Layer].Add(collider);
      }
      else
      {
        AddLayer(collider.LayerName);
        collider.Layer = GetLayer(collider.LayerName);
        if (ColliderLayers[collider.Layer].Contains(collider) is false)
        {
          ColliderLayers[collider.Layer].Add(collider);
          return true;
        }
        else
          return false;
      }
      return true;
    }

    /// <summary>
    /// 删除指定碰撞体.
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public bool RemoveCollider(Collider collider)
    {
      _layersVersion++;
      return ColliderLayers[collider.Layer].Remove(collider);
    }

    public const int Block = 1920;

    // —— 分块缓存 ——
    // 同一层在同一帧内、同一共享模式下, 分块结果对所有询问碰撞器完全相同,
    // 因此每层每模式只建一次、整帧复用 (原先每个碰撞器×每层重建一次, O(n²) 且大量堆分配).
    // 碰撞体增删会递增 _layersVersion 使缓存失效; 字典实例跨帧复用, 重建时只做 Clear.
    private CacheEntry[] _blockCacheShared;
    private CacheEntry[] _blockCachePrivate;
    private long _frameStamp;
    private long _layersVersion;

    private struct CacheEntry
    {
      public long Frame;
      public long Version;
      public Dictionary<Point, List<Collider>> Map;
    }

    public event Action<Collider, Collider> OnAabb;

    public event Action<Collider, Collider> OnCollision;

    public void DoInitialize()
    {
    }

    public void Start()
    {
      for (int count = 0; count < LayerIdentifiers.Count; count++)
        ColliderLayers.Add(new List<Collider>());
    }

    public void DoUpdate(GameTime time)
    {
      _frameStamp++;
      List<Collider> layer;

      int startX;
      int endX;
      int startY;
      int endY;
      RectangleF bounds;
      List<byte> maskLayers = null; // 当前碰撞器需要检测的碰撞层列表
      Dictionary<Point, List<Collider>> collisions;
      Point blockCoord; // 网格单元格的坐标
      List<Collider> block;
      Collider collider;

      for (int layerIndex = 0; layerIndex < ColliderLayers.Count; layerIndex++)
      {
        layer = ColliderLayers[layerIndex];

        for (int cIndex = 0; cIndex < layer.Count; cIndex++)
        {
          collider = layer[cIndex]; // 获取当前碰撞器对象
          bounds = collider.Sensor.Bounds; //AABB
          maskLayers = collider.Mask; // 获取当前碰撞器需要检测的碰撞层列表
          for (int mask = 0; mask < maskLayers.Count; mask++)
          {
            if (maskLayers[mask] >= ColliderLayers.Count)
              continue;
            if (maskLayers[mask] == collider.Layer)
            {
              collisions = DoBlock(maskLayers[mask], true); //使用共享模式分块
              SameLayerCheck(collisions);
            }
            else
            {
              collisions = DoBlock(maskLayers[mask], false); //使用非共享模式分块
              startX = (int)Math.Floor(bounds.Left / Block);
              endX = (int)Math.Floor(bounds.Right / Block);
              startY = (int)Math.Floor(bounds.Top / Block);
              endY = (int)Math.Floor(bounds.Bottom / Block);
              for (int x = startX; x <= endX; x++)
              {
                for (int y = startY; y <= endY; y++)
                {
                  blockCoord = new Point(x, y);
                  block = collisions.GetValueOrDefault(blockCoord);
                  if (block is not null)
                  {
                    BlockCheck(collider, block);
                  }
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// 为指定层执行分块操作并返回分块字典.
    /// <br>同一帧内同层同模式的结果会被缓存复用; 层列表版本号变化时重建.</br>
    /// </summary>
    /// <param name="layerIndex">层索引.</param>
    /// <param name="colliderShare">同一个Collider是否允许被多个分块List共享; 若为 <see langword="true"/>, 则允许, 否则按左上角坐标取模.</param>
    /// <returns></returns>
    private Dictionary<Point, List<Collider>> DoBlock(int layerIndex, bool colliderShare = true)
    {
      CacheEntry[] cache = colliderShare ? _blockCacheShared : _blockCachePrivate;
      if (cache is null || cache.Length <= layerIndex)
      {
        Array.Resize(ref cache, Math.Max(ColliderLayers.Count, layerIndex + 1));
        if (colliderShare)
          _blockCacheShared = cache;
        else
          _blockCachePrivate = cache;
      }
      ref CacheEntry entry = ref cache[layerIndex];
      if (entry.Map is not null && entry.Frame == _frameStamp && entry.Version == _layersVersion)
        return entry.Map;
      // 版本失效的旧图此刻不应有进行中的遍历 (增删碰撞体只发生在碰撞阶段之外), 可安全清空重建
      if (entry.Map is null)
        entry.Map = new Dictionary<Point, List<Collider>>();
      else
        entry.Map.Clear();
      entry.Frame = _frameStamp;
      entry.Version = _layersVersion;

      var result = entry.Map;
      Collider target;
      RectangleF bounds;
      Point blockCoord;
      List<Collider> block;
      int startX;
      int endX;
      int startY;
      int endY;
      List<Collider> interactLayer = ColliderLayers[layerIndex]; //交互层
      for (int c = 0; c < interactLayer.Count; c++)
      {
        target = interactLayer[c]; // 获取目标碰撞器对象
        bounds = target.Sensor.Bounds;
        startX = (int)Math.Floor(bounds.Left / Block);
        endX = (int)Math.Floor(bounds.Right / Block);
        startY = (int)Math.Floor(bounds.Top / Block);
        endY = (int)Math.Floor(bounds.Bottom / Block);
        if (colliderShare)
        {
          blockCoord = new Point(startX, startY);
          if (!result.TryGetValue(blockCoord, out block))
            result[blockCoord] = block = new List<Collider>();
          block.Add(target);
        }
        else
        {
          for (int x = startX; x <= endX; x++)
          {
            for (int y = startY; y <= endY; y++)
            {
              blockCoord = new Point(x, y);
              if (!result.TryGetValue(blockCoord, out block))
                result[blockCoord] = block = new List<Collider>();
              block.Add(target);
            }
          }
        }
      }
      return result;
    }

    private void SameLayerCheck(Dictionary<Point, List<Collider>> layer)
    {
      Collider a;
      Collider b;
      Point aCoord;
      Point bCoord;
      List<Collider> block;
      foreach (KeyValuePair<Point, List<Collider>> blocks in layer)
      {
        Point coord = blocks.Key;
        block = blocks.Value;
        for (int i = 0; i < block.Count; i++)
        {
          a = block[i];
          aCoord = GetBlockCoord(a);
          for (int j = i + 1; j < block.Count; j++)
          {
            b = block[j];
            if (a.Guid == b.Guid)
              continue;
            bCoord = GetBlockCoord(b);
            if (CheckAABB(a, b))
            {
              if (aCoord == bCoord)
              {
                if (aCoord == coord)
                  DoCollisionEvent();
              }
              else
                DoCollisionEvent();
            }
          }
        }
      }
      void DoCollisionEvent()
      {
        OnAabb?.Invoke(a, b);
        if (CheckCollision(a, b))
        {
          OnCollision?.Invoke(a, b);
          a.DoCollision(b);
        }
      }
    }

    private void BlockCheck(Collider collider, List<Collider> block)
    {
      Collider target;
      for (int index = 0; index < block.Count; index++)
      {
        target = block[index];
        if (collider.Guid == target.Guid)
          continue;
        if (collider.CheckAabb(target))
        {
          OnAabb?.Invoke(collider, target);
          if (collider.CheckCollision(target))
          {
            OnCollision?.Invoke(collider, target);
            collider.DoCollision(target);
          }
        }
      }
    }

    public static bool CheckAABB(Collider a, Collider b) => a.CheckAabb(b);

    public static bool CheckCollision(Collider a, Collider b) => a.CheckCollision(b);

    private Point GetBlockCoord(Collider collider)
    {
      RectangleF bounds = collider.Sensor.Bounds;
      return new Point((int)Math.Floor(bounds.Left / Block), (int)Math.Floor(bounds.Top / Block));
    }

    public void Dispose()
    {
    }
  }
}