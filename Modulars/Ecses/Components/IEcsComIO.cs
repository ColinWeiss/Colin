namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : IEcsCom
  {
    void SaveStep(BinaryWriter writer);
    void LoadStep(BinaryReader reader);
  }
}