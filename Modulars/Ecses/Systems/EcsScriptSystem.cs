using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
    /// <summary>
    /// 为EcsComScript提供生命周期钩子.
    /// </summary>
    public class EcsScriptSystem : SectionSystem
    {
        public override void Start()
        {
            foreach (ISectionComponent component in Current.Components.Values)
                if (component is EcsComScript script)
                    script.Start();
            base.Start();
        }
        public override void DoUpdate()
        {
            foreach (ISectionComponent component in Current.Components.Values)
                if (component is EcsComScript script)
                    script.Reset();
            foreach (ISectionComponent component in Current.Components.Values)
                if (component is EcsComScript script)
                    script.DoUpdate();
            base.DoUpdate();
        }
    }
}