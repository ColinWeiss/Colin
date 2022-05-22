using Microsoft.Xna.Framework.Graphics;

namespace Colin.Core.Scenes
{
    /// <summary>
    /// 一个游戏场景.
    /// </summary>
    public class Scene : EngineComponent
    {
        public List<SceneContentLayer>? ContentLayers { get; } = new List<SceneContentLayer>( );

        protected override void Initialization( )
        {
            for ( int count = 0; count < ContentLayers.Count; count++ )
                ContentLayers[ count ].Initialize( );
            base.Initialization( );
        }

        protected override void Update( )
        {
            for ( int count = 0 ; count < ContentLayers.Count; count++ )
                if ( ContentLayers[ count ].Enable )
                    ContentLayers[ count ].Update( HardwareInfo.GameTimeCache );
            base.Update( );
        }

        protected override void Draw( SpriteBatch spriteBatch )
        {
            for ( int count = ContentLayers.Count - 1 ; count >= 0 ; count-- )
                if ( ContentLayers[ count ].Visable )
                    ContentLayers[ count ].Draw( HardwareInfo.GameTimeCache );
            base.Draw( spriteBatch );

        }

        public void Register( SceneContentLayer layer )
        {
            layer.Scene = this;
            ContentLayers.Add( layer );
        }
    }
}