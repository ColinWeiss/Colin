using FontStashSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Colin.Core.Modulars.Collisions
{
  public class Collision : ISceneModule
  {
    public Scene Scene { get; set; }

    public bool Enable { get; set; }

    public Dictionary<string, byte> LayerIdentifiers = new Dictionary<string, byte>();
    public byte GetLayer(string layerName)
      => LayerIdentifiers.GetValueOrDefault(layerName);

    public void AddLayer(string layerName)
      => LayerIdentifiers.Add(layerName, (byte)LayerIdentifiers.Count);

    public List<List<Collider>> ColliderLayers = new List<List<Collider>>();

    /// <summary>
    /// 添加碰撞体至模块列表.
    /// </summary>
    /// <param name="collider"></param>
    public bool AddCollider(Collider collider)
    {
      if (string.IsNullOrEmpty(collider.LayerName) || string.IsNullOrWhiteSpace(collider.LayerName))
        collider.LayerName = "Default Layer";
      collider.Layer = GetLayer(collider.LayerName);
      if (ColliderLayers[collider.Layer].Contains(collider) is false)
      {
        ColliderLayers[collider.Layer].Add(collider);
        return true;
      }
      else
        return false;
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

    public event Action<Guid, Guid> OnAabb;

    public event Action<Guid, Guid> OnCollision;

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
      _checkedPairs.Clear();

      List<Collider> layer;
      Dictionary<Point, List<Collider>> collisions;
      int startX;
      int endX;
      int startY;
      int endY;
      RectangleF bounds;
      Collider collider;
      Point blockCoord;
      List<Collider> interacts;
      List<byte> collisionLayers = null;
      for (int layerIndex = 0; layerIndex < ColliderLayers.Count; layerIndex++)
      {
        layer = ColliderLayers[layerIndex];
        for (int cIndex = 0; cIndex < layer.Count; cIndex++)
        {
          collider = layer[cIndex];
          bounds = collider.Sensor.Bounds;
          collisionLayers = collider.Mask;
          collisions = new Dictionary<Point, List<Collider>>();

          for (int interactLayer = 0; interactLayer < collisionLayers.Count; interactLayer++)
          {
            interacts = ColliderLayers[interactLayer];
            for (int c = 0; c < interacts.Count; c++)
            {
              startX = (int)Math.Floor(bounds.Left / Block);
              endX = (int)Math.Floor(bounds.Right / Block);
              startY = (int)Math.Floor(bounds.Top / Block);
              endY = (int)Math.Floor(bounds.Bottom / Block);
              if (endX - startX == 0 && endY - startY == 0)
              {
                blockCoord = new Point(startX, startY);
                if (!collisions.ContainsKey(blockCoord))
                  collisions[blockCoord] = new List<Collider>();
                collisions[blockCoord].Add(collider);
              }
              else
              {
                for (int x = startX; x <= endX; x++)
                {
                  for (int y = startY; y <= endY; y++)
                  {
                    blockCoord = new Point(x, y);
                    if (!collisions.ContainsKey(blockCoord))
                      collisions[blockCoord] = new List<Collider>();
                    collisions[blockCoord].Add(collider);
                  }
                }
              }
            }
          }
          foreach (var block in collisions.Values)
          {
            CollisionCheck(block);
          }
        }
      }
    }

    private HashSet<(Guid, Guid)> _checkedPairs = new HashSet<(Guid, Guid)>();
    public void CollisionCheck(List<Collider> group)
    {
      Collider a;
      Collider b;
      for (int i = 0; i < group.Count; i++)
      {
        a = group[i];
        for (int j = i + 1; j < group.Count; j++)
        {
          b = group[j];

          var pair = (a.Guid < b.Guid) ? (a.Guid, b.Guid) : (b.Guid, a.Guid);

          if (_checkedPairs.Contains(pair))
            continue;

          if (a.Sensor.Bounds.Intersects(b.Sensor.Bounds))
          {
            OnAabb?.Invoke(a.Guid, b.Guid);
            if (a.Sensor.CollidesWith(b.Sensor))
            {
              OnCollision?.Invoke(a.Guid, b.Guid);
              _checkedPairs.Add(pair);
            }
          }
        }
      }
    }
    public void Dispose()
    {

    }
  }
}