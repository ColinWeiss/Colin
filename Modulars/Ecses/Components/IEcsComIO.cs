namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : IEntityCom
  {
    public virtual void SaveStep(BinaryWriter writer) { }
    public virtual void LoadStep(BinaryReader reader) { }
  }
}