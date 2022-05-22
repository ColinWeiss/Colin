using Colin.Common.Core.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Core.Scenes
{
    /// <summary>
    /// 场景内容层: 场景用户交互界面内容层.
    /// </summary>
    public class SceneUILayer : SceneContentLayer
    {
        Container Container;

        public SceneUILayer( Scene scene ) : base( scene ) { }

        public override void Initialize( )
        {
            Container = new Container( );
            Container.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            Engine.Instance.Window.ClientSizeChanged += ( s, e ) =>
                Container.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            Container.DoInitialize( );
            base.Initialize( );
        }

        public override void Update( GameTime gameTime )
        {
            Container.DoUpdate( );
            base.Update( gameTime );
        }

        public override void Draw( GameTime gameTime )
        {
            Container.DoDraw( );
            base.Draw( gameTime );
        }

        public void Register( Container container ) => Container.Register( container );
    }
}