/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Graphics;
using Colin.Core.Assets;
在此之后:
using Colin.Core.Common;
using Colin.Core.Graphics;
*/

namespace Colin.Core.Modulars.Backgrounds
{
    /// <summary>
    /// 场景背景.
    /// </summary>
    public sealed class Background : ISceneModule, IRenderableISceneModule
    {
        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public bool FinalPresentation { get; set; }

        public SceneCamera Camera => Scene.SceneCamera;

        public RenderTarget2D SceneRt { get; set; }

        public BackgroundStyle CurrentStyle { get; private set; }

        /// <summary>
        /// 绘制左右循环图层所使用的着色器文件.
        /// </summary>
        private Effect LeftRightLoopEffect;

        private Texture2D _screenMap;

        /// <summary>
        /// 设置背景样式.
        /// </summary>
        /// <param name="style"></param>
        public void SetBackgroundStyle(BackgroundStyle style)
        {
            //  if(CurrentStyle != style)
            CurrentStyle = style;
        }

        public void DoInitialize()
        {
            _screenMap = TextureAssets.Get("Pixel");
            LeftRightLoopEffect = EffectAssets.Get("LeftRightLoopMapping");
        }
        public void Start()
        {

        }

        public void DoUpdate(GameTime time)
        {
            CurrentStyle?.UpdateStyle();
        }

        public void DoRender(SpriteBatch batch)
        {
            if (CurrentStyle != null)
            {
                BackgroundLayer layer;
                for (int count = 0; count < CurrentStyle.Layers.Count; count++)
                {
                    layer = CurrentStyle.Layers[count];
                    if (layer.IsFix)
                        RenderFixBackground(layer);
                    if (layer.IsLoop && layer.LoopStyle == BackgroundLoopStyle.LeftRightConnect)
                        RenderLeftRightLoopBackground(layer);
                }
            }
        }

        public void RenderFixBackground(BackgroundLayer layer)
        {
            EngineInfo.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            EngineInfo.SpriteBatch.Draw(
                layer.Sprite.Source,
                CurrentStyle.FixLayerOverallOffset,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                CurrentStyle.FixLayerScale,
                SpriteEffects.None,
                layer.Sprite.Depth
                );
            EngineInfo.SpriteBatch.End();
        }

        public void RenderLeftRightLoopBackground(BackgroundLayer layer)
        {
            Vector3 translateBody = new Vector3(-(Camera.Position - CurrentStyle.LoopLayerDrawPosition) * layer.Parallax, 0f);
            Vector3 translateCenter = new Vector3(Camera.Translate, 0f);
            Vector2 drawCount = new Vector2((float)EngineInfo.ViewWidth / layer.Sprite.Width, (float)EngineInfo.ViewHeight / layer.Sprite.Height);
            Vector2 offset = Vector2.One / layer.Sprite.SizeF;
            layer.Transform = Matrix.CreateTranslation(translateBody) * Matrix.CreateTranslation(translateCenter);
            offset *= new Vector2(-layer.Translation.X, -layer.Translation.Y);
            offset.X += CurrentStyle.LoopLayerOffset.X / layer.Sprite.Height;
            offset.Y += CurrentStyle.LoopLayerOffset.Y / layer.Sprite.Width;

            EngineInfo.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            LeftRightLoopEffect.Parameters["MappingTexture"].SetValue(layer.Sprite.Source);
            LeftRightLoopEffect.Parameters["DrawCount"].SetValue(drawCount / Camera.Zoom);
            LeftRightLoopEffect.Parameters["Offset"].SetValue(offset);
            LeftRightLoopEffect.CurrentTechnique.Passes[0].Apply();

            //        DrawHelper.DrawRectSimply( EngineInfo.Graphics.GraphicsDevice, 0, 0, SceneRt.Width, SceneRt.Height
            //  , _screenMap, Color.White );

            EngineInfo.SpriteBatch.Draw(_screenMap, new Rectangle(0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight), Color.White);
            EngineInfo.SpriteBatch.End();
        }

        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                //     Scene = null;
                //    _camera = null;
                disposedValue = true;
            }
        }

        ~Background()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}