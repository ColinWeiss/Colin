using Colin.Core.Modulars.Networks;

namespace Colin.Core.Modulars.Ecses.Components
{
  public class EcsComNetwork : EcsComScript, INetworkMode
  {
    public void ReceiveDatas(BinaryReader reader, NetModeState state)
    {
      Entity.Transform.Translation.X = reader.ReadSingle();
      Entity.Transform.Translation.Y = reader.ReadSingle();

    }
    public void SendDatas(BinaryWriter writer, NetModeState state)
    {
      writer.Write(Entity.Transform.Translation.X);
      writer.Write(Entity.Transform.Translation.Y);
    }
  }
}