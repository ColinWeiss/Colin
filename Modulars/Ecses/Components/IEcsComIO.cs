namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : ISectionCom
  {
    public virtual void SaveStep(BinaryWriter writer) { }
    public virtual void LoadStep(BinaryReader reader) { }
  }
}