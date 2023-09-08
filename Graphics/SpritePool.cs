namespace Colin.Core.Graphics
{
    /// <summary>
    /// 纹理缓存, 单例.
    /// </summary>
    public class SpritePool : Dictionary<string, Sprite>
    {
        private static SpritePool _instance = new SpritePool();
        public static SpritePool Instance => _instance;
        public static float DepthSteps = 1000000f;

        public new void Add( string key, Sprite value )
        {
            if(!ContainsKey( key ))
            {
                value.Depth = Count / DepthSteps;
                base.Add( key, value );
            }
        }
    }
}