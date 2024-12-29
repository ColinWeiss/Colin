using Colin.Core.Resources;
using DeltaMachine.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块区块类.
  /// </summary>
  public class TileChunk
  {
    /// <summary>
    /// 获取物块区块所属的世界物块模块.
    /// </summary>
    public readonly Tile Tile;

    /// <summary>
    /// 获取物块区块所属的物块放置模块.
    /// </summary>
    public readonly TilePlacer Placer;

    /// <summary>
    /// 获取物块区块所属的物块破坏模块.
    /// </summary>
    public readonly TileDestructor Destructor;

    /// <summary>
    /// 获取物块区块所属的物块刷新模块.
    /// </summary>
    public readonly TileRefresher Refresher;

    /// <summary>
    /// 获取区块的深度.
    /// <br>它与 <see cref="Tile.Depth"/> 的值相等.</br>
    /// </summary>
    public readonly int Depth;

    /// <summary>
    /// 获取区块的宽度.
    /// <br>它与 <see cref="TileOption.ChunkWidth"/> 的值相等.</br>
    /// </summary>
    public int Width => TileOption.ChunkWidth;

    /// <summary>
    /// 获取区块的宽度.
    /// <br>它与 <see cref="TileOption.ChunkHeight"/> 的值相等.</br>
    /// </summary>
    public int Height => TileOption.ChunkHeight;

    /// <summary>
    /// 指示区块的横坐标.
    /// </summary>
    public int CoordX;

    /// <summary>
    /// 指示区块的纵坐标.
    /// </summary>
    public int CoordY;

    /// <summary>
    /// 指示区块的量子层.
    /// <br>同一二维位置可以存在不同量子层的区块，用于无缝子世界</br>
    /// </summary>
    public int QuantumLayer;

    public Point Coord => new Point(CoordX, CoordY);

    private Rectangle? _tileRect;
    public Rectangle TileRect =>
      _tileRect ??=
      new Rectangle(
        CoordX * TileOption.ChunkWidth,
        CoordY * TileOption.ChunkHeight,
        TileOption.ChunkWidth,
        TileOption.ChunkHeight);

    private TileChunk temp;
    public TileChunk Top
    {
      get
      {
        temp = GetOffset(0, -1);
        if (temp is not null)
          return temp;
        else
          return null;
      }
    }
    public TileChunk Bottom
    {
      get
      {
        temp = GetOffset(0, 1);
        if (temp is not null)
          return temp;
        else
          return null;
      }
    }
    public TileChunk Left
    {
      get
      {
        temp = GetOffset(-1, 0);
        if (temp is not null)
          return temp;
        else
          return null;
      }
    }
    public TileChunk Right
    {
      get
      {
        temp = GetOffset(1, 0);
        if (temp is not null)
          return temp;
        else
          return null;
      }
    }

    /// <summary>
    /// 根据指定偏移量获取相对于该区块坐标偏移的区块.
    /// </summary>
    public TileChunk GetOffset(int offsetX, int offsetY)
    {
      return Tile.GetChunk(CoordX + offsetX, CoordY + offsetY);
    }

    /// <summary>
    /// 区块内的物块信息.
    /// </summary>
    public TileInfo[] Infos;

    /// <summary>
    /// 索引器: 根据索引获取物块信息.
    /// </summary>
    public ref TileInfo this[int index] => ref Infos[index];

    /// <summary>
    /// 索引器: 根据索引获取物块信息.
    /// </summary>
    public ref TileInfo this[int x, int y, int z]
    {
      get
      {
        return ref Infos[z * Width * Height + x + y * Width];
      }
    }

    public void ForEach(Action<TileInfo> info, int x, int y, int width, int height, int depth)
    {
      ref TileInfo _i = ref TileInfo.Null;
      try
      {
        for (int cx = x; cx < x + width; cx++)
        {
          for (int cy = y; cy < y + height; cy++)
          {
            _i = ref this[cx, cy, depth];
            info.Invoke(_i);
          }
        }
      }
      catch
      {
        Console.WriteLine("Error","区块遍历异常, 请检查输入参数合理性.");
      }
    }

    public TileChunk(Tile tile)
    {
      Tile = tile;
      Placer = tile.Scene.GetModule<TilePlacer>();
      Destructor = tile.Scene.GetModule<TileDestructor>();
      Refresher = tile.Scene.GetModule<TileRefresher>();
      Depth = tile.Depth;
      CoordX = 0;
      CoordY = 0;
      Infos = new TileInfo[1];
    }

    /// <summary>
    /// 执行初始化操作.
    /// <br>该操作会令区块初始化其整个物块信息的数组.</br>
    /// </summary>
    public void DoInitialize()
    {
      Infos = new TileInfo[Width * Height * Depth];
      for (int count = 0; count < Infos.Length; count++)
        CreateInfo(count);
    }

    /// <summary>
    /// 清除区块.
    /// </summary>
    public void Clear() => DoInitialize();

    /// <summary>
    /// 在指定索引处创建空物块信息.
    /// </summary>
    public void CreateInfo(int index)
    {
      Infos[index] = new TileInfo();
      Infos[index].Tile = Tile;
      Infos[index].Chunk = this;
      Infos[index].Empty = true;
      Infos[index].Index = index;
      Infos[index].Scripts = new Dictionary<Type, TileScript>();
    }
    /// <summary>
    /// 在指定坐标处创建物块信息.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void CreateInfo(int x, int y, int z) => CreateInfo(z * Width * Height + x + y * Width);

    /// <summary>
    /// 根据坐标和指定类型放置物块.
    /// </summary>
    /// <param name="doEvent">指示是否触发放置事件.</param>
    /// <param name="doRefresh">指示是否触发物块刷新事件.</param>
    public bool Place<T>(int x, int y, int z) where T : TileBehavior, new()
    {
      ref TileInfo info = ref this[x, y, z];
      TileBehavior behavior = CodeResources<TileBehavior>.GetFromType(typeof(T));
      if (behavior.CanPlaceMark(ref info))
      {
        Placer.Mark(info.WorldCoord3, behavior);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// 根据坐标和指定类型放置物块.
    /// </summary>
    public bool Place(TileBehavior behavior, int x, int y, int z)
    {
      ref TileInfo info = ref this[x, y, z];
      if (behavior.CanPlaceMark(ref info))
      {
        Placer.Mark(info.WorldCoord3, behavior);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// 破坏指定坐标的物块.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Destruct(int x, int y, int z, bool doEvent = true)
      => Destruct(z * Width * Height + x + y * Width, doEvent);

    public void Destruct(int index, bool doEvent = true)
    {
      TileInfo info = this[index];
      if (info.IsNull)
        return;
      if (Tile.HasInfoReference(info.WorldCoord3))
      {
        info = Tile.GetInfoReference(info.WorldCoord3);
        if (info.Empty is false && !info.IsNull)
          Destructor.Mark(info.WorldCoord3, doEvent);
      }
      else if (!Destructor.Queue.Contains((info.WorldCoord3, doEvent)))
      {
        if (!info.Empty)
          Destructor.Mark(info.WorldCoord3, doEvent);
      }
    }

    private bool _loading = false;
    public bool Loading => _loading;

    private bool _saved = false;
    public bool Saved => _saved;

    public void AsyncLoadChunk(string path)
    {
      _loading = true;
      DoInitialize();
      Task.Run(() =>
      {
        DoLoad(path);
      });
    }

    public void LoadChunk(string path)
    {
      DoInitialize();
      DoLoad(path);
    }

    private void DoLoad(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(fileStream))
        {
          ref TileInfo info = ref this[0, 0, 0];
          string typeName;
          TileScript script;
          for (int count = 0; count < Infos.Length; count++)
          {
            info = ref this[count];
            info.LoadStep(reader);
            if (!info.Empty)
            {
              typeName = CodeResources<TileBehavior>.GetTypeNameFromHash(reader.ReadInt32());
              if (typeName is not null)
              {
                info.Behavior = CodeResources<TileBehavior>.GetFromTypeName(typeName);
                info.Behavior.Tile = Tile;
                info.Behavior.OnInitialize(ref info); //执行行为初始化放置
                info.Behavior.OnScriptAdded(ref info);
                Refresher.Mark(info.WorldCoord3, 0);
              }
              for (int i = 0; i < info.Scripts.Count; i++)
              {
                script = info.Scripts.Values.ElementAt(i);
                script.LoadStep(reader);
              }
            }
          }
        }
      }
    }

    public void AsyncSaveChunk(string path)
    {
      Task.Run(() =>
      {
        DoSave(path);
        _saved = true;
      });
    }

    public void SaveChunk(string path)
    {
      try
      {
        DoSave(path);
        _saved = true;
      }
      catch
      {

      }
    }

    private void DoSave(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(fileStream))
        {
          int? hash;
          TileBehavior behavior;
          TileScript script;
          ref TileInfo info = ref TileInfo.Null;
          for (int count = 0; count < Infos.Length; count++)
          {
            info = ref Infos[count];
            behavior = info.Behavior;
            info.SaveStep(writer);
            if (!info.Empty && behavior is not null)
            {
              hash = CodeResources<TileBehavior>.GetHashFromTypeName(behavior.Identifier);
              if (hash.HasValue)
              {
                writer.Write(hash.Value);
              }
              for (int i = 0; i < info.Scripts.Count; i++)
              {
                script = info.Scripts.Values.ElementAt(i);
                script.SaveStep(writer);
              }
            }
          }
        }
      }
    }
  }
}