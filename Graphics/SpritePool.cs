﻿namespace Colin.Core.Graphics
{
    /// <summary>
    /// 纹理缓存, 单例.
    /// </summary>
    public class SpritePool : Dictionary<string, Sprite>, IGameComponent, IUpdateable
    {
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public bool Enabled { get; } = true;
        public int UpdateOrder { get; }

        private static SpritePool _instance = new SpritePool();
        public static SpritePool Instance => _instance;
        public static float DepthSteps = 100000f;
        public bool LoadComplete { get; private set; } = false;

        public void Initialize()
        {
            if(LoadComplete is false)
            {
                EngineConsole.WriteLine( ConsoleTextType.Remind, "纹理缓存初始化完毕." );
                LoadComplete = true;
            }
        }
        public new void Add( string key, Sprite value )
        {
            if(!ContainsKey( key ))
            {
                value.Depth = Count / DepthSteps;
                base.Add( key, value );
            }
        }

        public void Update( GameTime gameTime )
        {
            Sprite _sprite;
            List<Sprite> _sprites = Values.ToList();
            for(int count = 0; count < _sprites.Count; count++)
            {
                _sprite = _sprites[count];
                if(_sprite.AutoUpdateFrame && _sprite.Frame.FrameMax > 1
                    && _sprite.Frame.IsLoop && _sprite.Frame.IsPlay)
                    _sprite.Frame.UpdateFrame();
            }
        }
    }
}