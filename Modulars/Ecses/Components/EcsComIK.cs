using DeltaMachine.Core.Common.IK;

namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// IK骨架组件，单纯负责存一个骨架.
    /// </summary>
    public class EcsComIK : ISectionComponent
    {
        public IKSkeleton skeleton;

        public EcsComIK()
        {
            skeleton = new IKSkeleton();
        }

        public void DoInitialize()
        {
        }
    }
}