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
      Section _current;
      batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: Ecs.Scene.SceneCamera.View);
      for (int count = 0; count < Ecs.Sections.Length; count++)
      {
        _current = Ecs.Sections[count];
        if (_current is null)
          continue;
        foreach (ISectionCom component in _current.Components.Values)
          if (component is EcsComDeferredRender renderer && renderer.Visible)
            renderer.DoRender(device, batch);
      }
      batch.End();

      for (int count = 0; count < Ecs.Sections.Length; count++)
      {
        _current = Ecs.Sections[count];
        if (_current is null)
          continue;
        foreach (ISectionCom component in _current.Components.Values)
          if (component is EcsComAdvancedRender renderer && component is not EcsComDeferredRender && renderer.Visible)
            renderer.DoRender(device, batch);
      }
      base.DoRender(device, batch);
    }
  }
}