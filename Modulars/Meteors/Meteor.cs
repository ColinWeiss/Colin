namespace Colin.Core.Modulars.Meteors
{
    public class Meteor
    {
        public Vector2 Position;

        public Vector2 Velocity;

        public float Rotation;

        public float Scale;

        public int LeftFrame;

        public int LeftFrameMax;

        public int ActiveIndex { get; set; }

        public bool Active { get; set; }

        public void Awake()
        {
            Active = true;
        }

        public void Dormancy()
        {
            Active = false;
        }
    }
}