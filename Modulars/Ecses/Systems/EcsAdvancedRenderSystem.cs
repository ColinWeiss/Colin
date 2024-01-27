using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
    /// <summary>
    /// 高级自定义渲染系统.
    /// 处理AdvancedRender和DeferredRender的绘制
    /// </summary>
    public class EcsAdvancedRenderSystem : SectionSystem
    {
        public override void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            foreach (ISectionComponent component in Current.Components.Values)
            {
                if (component is EcsComAdvancedRender renderer && component is not EcsComDeferredRender)
                {
                    renderer.DoRender(device, batch);
                }
            }
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: Ecs.Scene.SceneCamera.View);
            foreach (ISectionComponent component in Current.Components.Values)
            {
                if (component is EcsComDeferredRender renderer)
                {
                    renderer.DoRender(device, batch);
                }
            }
            batch.End();
            base.DoRender(device, batch);
        }
    }
}