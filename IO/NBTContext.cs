using Colin.Core.Modulars.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Colin.Core.IO
{
  public class NbtContext : INBT
  {
    public List<INBT> NBT;
    public NbtContext()
    {
      NBT = new List<INBT>();
    }

    public void AddNbt<T>() where T : INBT, new()
    {
      INBT nbt = new T();
      int id = INBT.NBTIDHelper<T>.ID;
      if (id >= NBT.Count)
        NBT.AddRange(Enumerable.Repeat<INBT>(null, id - NBT.Count + 1));
      NBT[id] = nbt;
    }

    public void LoadStep(BinaryReader reader)
    {
      int nbtCount = reader.ReadInt32();
      Dictionary<string, INBT> namedTag = new();
      for (int i = 0; i < NBT.Count; i++)
        namedTag[NBT[i].GetType().Name] = NBT[i];
      for (int i = 0; i < nbtCount; i++)
      {
        string name = reader.ReadString();
        if (namedTag.TryGetValue(name, out var matchedNBT))
        {
          matchedNBT.LoadStep(reader);
        }
      }
    }
    public void SaveStep(BinaryWriter writer)
    {
      writer.Write(NBT.Count);
      INBT nbt;
      for (int i = 0; i < NBT.Count; i++)
      {
        nbt = NBT.ElementAt(i);
        string name = nbt.GetType().Name;
        writer.Write(name);
        nbt.SaveStep(writer);
      }
    }
  }
}