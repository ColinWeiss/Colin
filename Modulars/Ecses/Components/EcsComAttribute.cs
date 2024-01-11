namespace Colin.Core.Modulars.Ecses.Components
{
    public class EcsComAttribute : EcsComScript
    {
        /// <summary>
        /// 指示在移动时的速度增量.
        /// </summary>
        public Entry<float> MoveSpeedInc;

        /// <summary>
        /// 指示最大移动速度.
        /// </summary>
        public Entry<float> MoveSpeedMax;

        /// <summary>
        /// 指示不进行移动时, 移动速度的衰减速率.
        /// </summary>
        public Entry<float> MoveSpeedAtt;

        public override void DoInitialize()
        {
            MoveSpeedInc = new Entry<float>(0);
            MoveSpeedMax = new Entry<float>(0);
            MoveSpeedAtt = new Entry<float>(0);
        }
        public override void Reset()
        {
            MoveSpeedMax.Reset();
            MoveSpeedInc.Reset();
            MoveSpeedAtt.Reset();
            base.Reset();
        }
    }
}