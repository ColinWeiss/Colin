namespace Colin.Core.Modulars.Ecses.Components
{
  public interface IEcsComIO : ISectionComponent
  {
    public virtual void SaveStep(BinaryWriter writer) { }
    public virtual void LoadStep(BinaryReader reader) { }
  }
}