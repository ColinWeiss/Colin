using Colin.Core.Common.Debugs;
using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 高级自定义渲染系统.
  /// 处理AdvancedRender和DeferredRender的绘制
  /// </summary>
  public class EcsAdvancedRenderSystem : Entitiesystem
  {
    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      using (DebugProfiler.Tag("Entity"))
      {
        Entity _current;
        batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: Ecs.Scene.SceneCamera.View);
        for (int count = 0; count < Ecs.Entities.Length; count++)
        {
          _current = Ecs.Entities[count];
          if (_current is null)
            continue;
          foreach (IEntityCom component in _current.Components.Values)
            if (component is EcsComDeferredRender renderer && renderer.Visible)
              renderer.DoRender(device, batch);
        }
        batch.End();

        for (int count = 0; count < Ecs.Entities.Length; count++)
        {
          _current = Ecs.Entities[count];
          if (_current is null)
            continue;
          foreach (IEntityCom component in _current.Components.Values)
            if (component is EcsComAdvancedRender renderer && component is not EcsComDeferredRender && renderer.Visible)
              renderer.DoRender(device, batch);
        }
      }
      base.DoRender(device, batch);
    }
  }
}