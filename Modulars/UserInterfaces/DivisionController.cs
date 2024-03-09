namespace Colin.Core.Modulars.UserInterfaces
{
    public class DivisionController
    {
        internal Div div;
        public Div Div => div;
        public virtual void OnBinded() { }
        public virtual void OnDivInitialize() { }
        public virtual void Layout(ref DivFrontLayout layout) { }
        public virtual void Interact(ref InteractStyle interact) { }
        public virtual void Design(ref DesignStyle design) { }
    }
}