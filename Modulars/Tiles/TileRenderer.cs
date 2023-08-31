using Colin.Core.Common;
using Colin.Core.Extensions;
using Colin.Core.Graphics;
using Colin.Core.Assets;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    public class TileRenderer : ISceneComponent, IRenderableSceneComponent
    {
        public RenderTarget2D cacheRt;

        public RenderTarget2D cacheRtSwap;

        private Camera _camera;
        public Camera Camera => _camera;
        public void BindCamera( Camera camera ) => _camera = camera;

        public Tile Tile { get; }

        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public RenderTarget2D SceneRt { get; set; }

        public void DoInitialize( )
        {
            cacheRt = RenderTargetExt.CreateDefault( );
            cacheRtSwap = RenderTargetExt.CreateDefault( );
            EngineInfo.Engine.Window.ClientSizeChanged += CacheRenderTargetInit;
        }
        private void CacheRenderTargetInit( object sender, EventArgs e )
        {
            cacheRt?.Dispose( );
            cacheRt = RenderTargetExt.CreateDefault( );
            cacheRtSwap?.Dispose( );
            cacheRtSwap = RenderTargetExt.CreateDefault( );
        }
        public void DoUpdate( GameTime time )
        {

        }

        public void First( SpriteBatch batch )
        {
            batch.Begin( samplerState: SamplerState.PointClamp , transformMatrix: Camera.View );
            Vector2 cP = Camera.Position - Camera.SizeF / 2;
            Point start = (cP / 16).ToPoint( );
            Point view = (Camera.SizeF / 16).ToPoint( );
            Point loop = start + view;
            start.X = Math.Clamp( start.X, 0, Tile.Width - 1 );
            start.Y = Math.Clamp( start.Y, 0, Tile.Height - 1 );
            loop.X = Math.Clamp( loop.X + 1, EngineInfo.ViewWidth / 16, Tile.Width - 1 );
            loop.Y = Math.Clamp( loop.Y + 1, EngineInfo.ViewHeight / 16, Tile.Height - 1 );
            for( int countX = start.X ; countX < loop.X ; countX++ )
                for( int countY = start.Y ; countY < loop.Y ; countY++ )
                    Tile.behaviors[countX, countY].RenderTexture( countX , countY );
            batch.End( );
        }
        public void DoRender( SpriteBatch batch )
        {
            First( batch );
        }
        public TileRenderer( Tile tile, SceneCamera camera )
        {
            Tile = tile;
            _camera = camera;
        }
    }
}