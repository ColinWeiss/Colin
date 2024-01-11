namespace Colin.Core.Modulars.Ecses
{
    public class EnvironmentalController
    {
        /// <summary>
        /// 重力.
        /// </summary>
        public Vector2 UniGravity => Vector2.UnitY * 30;

        private float _airResistance;
        /// <summary>
        /// 指示空气阻力.
        /// </summary>
        public float AirResistance
        {
            get => _airResistance;
            set => _airResistance = value * Time.DeltaTime;
        }
    }
}