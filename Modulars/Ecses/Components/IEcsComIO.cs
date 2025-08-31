namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : IEcsCom
  {
    public void SaveStep(BinaryWriter writer);
    public void LoadStep(BinaryReader reader);
  }
}