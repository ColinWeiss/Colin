using Colin.Core.Common;
using Colin.Core.Extensions;
using
/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Assets;
在此之后:
using DeltaMachine.Core.GameContents.Tiles;
*/

/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using System.Threading.Tasks;
using DeltaMachine.Core.GameContents.Tiles;
在此之后:
using System.Threading.Tasks;
*/
DeltaMachine.Core.GameContents.Tiles;

namespace Colin.Core.Modulars.Tiles
{
    public class TileRenderer : ISceneModule, IRenderableISceneModule
    {
        public RenderTarget2D cacheRt;

        public RenderTarget2D cacheRtSwap;

        private Camera _camera;
        public Camera Camera => _camera;
        public void BindCamera( Camera camera ) => _camera = camera;

        public Tile Tile => Scene.GetModule<Tile>();

        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public RenderTarget2D SceneRt { get; set; }

        public void DoInitialize()
        {
            cacheRt = RenderTargetExt.CreateDefault();
            cacheRtSwap = RenderTargetExt.CreateDefault();
            EngineInfo.Engine.Window.ClientSizeChanged += CacheRenderTargetInit;
        }
        private void CacheRenderTargetInit( object sender, EventArgs e )
        {
            cacheRt?.Dispose();
            cacheRt = RenderTargetExt.CreateDefault();
            cacheRtSwap?.Dispose();
            cacheRtSwap = RenderTargetExt.CreateDefault();
        }
        public void Start()
        {
            _camera = Scene.SceneCamera;
        }
        public void DoUpdate( GameTime time )
        {
            if(Tile is null)
            {
                EngineConsole.WriteLine( ConsoleTextType.Error , "因缺少 Tile 模块, TileRenderer 无法运行, 现已自动弹出." );
                Scene.RemoveModule( this );
            }
        }
        public void First( SpriteBatch batch )
        {
            batch.Begin( samplerState: SamplerState.PointClamp, transformMatrix: Camera.View );
            Vector2 cP = Camera.Position - Camera.SizeF / 2;
            Point start = (cP / 16).ToPoint();
            Point view = (Camera.SizeF / 16).ToPoint();
            Point loop = start + view;
            start.X = Math.Clamp( start.X, 0, Tile.Width - 1 );
            start.Y = Math.Clamp( start.Y, 0, Tile.Height - 1 );
            loop.X = Math.Clamp( loop.X + 1, EngineInfo.ViewWidth / 16, Tile.Width - 1 );
            loop.Y = Math.Clamp( loop.Y + 1, EngineInfo.ViewHeight / 16, Tile.Height - 1 );
            // tuple元素为 深度，是边框还是填充，物块
            var tileList = new List<Tuple<float, bool, TileBehavior>>();
            for(int countX = start.X; countX < loop.X; countX++)
                for(int countY = start.Y; countY < loop.Y; countY++)
                {
                    var behavior = Tile.Behaviors[countX, countY];
                    float depth = 0;
                    if(behavior is MonoBlock block)
                        depth = block.Sprite.Depth;
                    // 保证顺序，同类物块先边框后填充
                    tileList.Add( new Tuple<float, bool, TileBehavior>( depth + 0.5f / SpritePool.DepthSteps, true, behavior ) );
                    tileList.Add( new Tuple<float, bool, TileBehavior>( depth, false, behavior ) );
                }
            tileList.Sort( ( x, y ) => x.Item1 < y.Item1 ? 1 : x.Item1 > y.Item1 ? -1 : 0 );
            foreach(var tuple in tileList)
            {
                var behavior = tuple.Item3;
                if(tuple.Item2)
                    behavior.RenderBorder();
                else
                    behavior.RenderTexture();
            }
            batch.End();
        }
        public void DoRender( SpriteBatch batch )
        {
            First( batch );
        }
    }
}