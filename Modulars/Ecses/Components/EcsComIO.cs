namespace Colin.Core.Modulars.Ecses.Components
{
    public class EcsComIO : ISectionComponent
    {
        public void DoInitialize() { }
        public virtual void LoadStep(BinaryReader reader) { }
        public virtual void SaveStep(BinaryWriter writer) { }
    }
}