namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : IEntityCom
  {
    public void SaveStep(BinaryWriter writer);
    public void LoadStep(BinaryReader reader);
  }
}