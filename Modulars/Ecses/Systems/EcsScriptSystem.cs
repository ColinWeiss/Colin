using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
    /// <summary>
    /// 为EcsComScript提供生命周期钩子.
    /// </summary>
    public class EcsScriptSystem : SectionSystem
    {
        public override void DoUpdate()
        {
            foreach (ISectionComponent component in Current.Components.Values)
                if (component is IResetable resetableCom)
                    resetableCom.Reset();
            foreach (ISectionComponent component in Current.Components.Values)
                if (component is EcsComScript script)
                {
                    if (!script._updateStarted)
                    {
                        script.UpdateStart();
                        script._updateStarted = true;
                    }
                    script.DoUpdate();
                }
            base.DoUpdate();
        }
    }
}