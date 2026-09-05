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
    public override void DoUpdate()
    {
      Entity entity;
      EcsComRenderData renderData;
      for (int count = 0; count < Ecs.Entities.Length; count++)
      {
        entity = Ecs.Entities[count];
        if (entity is null)
          continue;
        renderData = entity.GetCom<EcsComRenderData>();
        if (renderData is null)
          continue;
        foreach (var update in renderData.Updates)
        {
          update.Function.Invoke();
        }
      }
      base.DoUpdate();
    }
    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      using (DebugProfiler.Tag("Entity"))
      {
        Entity entity;
        EcsComRenderData renderData;
        batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: Ecs.Scene.Camera.View);
        for (int count = 0; count < Ecs.Entities.Length; count++)
        {
          entity = Ecs.Entities[count];
          if (entity is null)
            continue;
          renderData = entity.GetCom<EcsComRenderData>();
          if (renderData is null)
            continue;
          foreach (var deferred in renderData.Deferreds)
          {
            deferred.Function.Invoke(device, batch);
          }
        }
        batch.End();

        // —— GPU 粒子与刀光 Mesh: 在实体层之上绘制 (特效覆盖于武器表现之上).
        Particle.Core.ParticleManager.Instance.TickAndRender(Time.DeltaTime, Ecs.Scene.Camera.Transform);
        if (Particle.Core.ParticleManager.Instance.IsInitialized)
        {
          Particle.Slash.SlashRenderer slash = Particle.Slash.SlashRenderer.GetOrCreate();
          slash.TickAll(Time.DeltaTime);
          slash.DrawAll(Ecs.Scene.Camera.Transform);
        }

        for (int count = 0; count < Ecs.Entities.Length; count++)
        {
          entity = Ecs.Entities[count];
          if (entity is null)
            continue;
          renderData = entity.GetCom<EcsComRenderData>();
          if (renderData is null)
            continue;
          foreach (var advanced in renderData.Advanceds)
          {
            advanced.Function.Invoke(device, batch);
          }
        }
      }
      base.DoRender(device, batch);
    }
  }
}