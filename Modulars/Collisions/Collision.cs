

using FontStashSharp;
using System.Threading.Tasks;

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
    }

    public List<List<Collider>> ColliderLayers = new List<List<Collider>>();

    /// <summary>
    /// 添加碰撞体至模块列表.
    /// </summary>
    /// <param name="collider"></param>
    public bool AddCollider(Collider collider, string layerName = "Default Layer")
    {
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
      return ColliderLayers[collider.Layer].Remove(collider);
    }

    public const int Block = 1920;

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
    /// </summary>
    /// <param name="layerIndex">层索引.</param>
    /// <param name="colliderShare">同一个Collider是否允许被多个分块List共享; 若为 <see langword="true"/>, 则允许, 否则按左上角坐标取模.</param>
    /// <returns></returns>
    private Dictionary<Point, List<Collider>> DoBlock(int layerIndex, bool colliderShare = true)
    {
      var result = new Dictionary<Point, List<Collider>>();
      Collider target;
      RectangleF bounds;
      Point blockCoord;
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
          if (!result.ContainsKey(blockCoord))
            result[blockCoord] = new List<Collider>();
          result[blockCoord].Add(target);
        }
        else
        {
          for (int x = startX; x <= endX; x++)
          {
            for (int y = startY; y <= endY; y++)
            {
              blockCoord = new Point(x, y);
              if (!result.ContainsKey(blockCoord))
                result[blockCoord] = new List<Collider>();
              result[blockCoord].Add(target);
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
      Point coord;
      Point aCoord;
      Point bCoord;
      List<Collider> block;
      for (int layerIndex = 0; layerIndex < layer.Count; layerIndex++)
      {
        coord = layer.ElementAt(layerIndex).Key;
        block = layer.ElementAt(layerIndex).Value;
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
            Console.WriteLine("?");
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