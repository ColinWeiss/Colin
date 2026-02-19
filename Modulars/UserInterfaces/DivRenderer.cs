namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素渲染器.
  /// </summary>
  public class DivRenderer
  {
    internal Div div;
    public Div Div => div;
    public bool Adaptive = false;

    public bool UseTranslucent;

    public virtual void OnBinded() { }
    public virtual void OnDivInitialize() { }
    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      batch.End();
      div.BeginRender(BlendState.HumanityTranslucent, SamplerState.PointWrap);
      RenderStep(device, batch);
      batch.End();
      if(div.UpperBatch is not null)
        div.UpperBatch.BeginRender(BlendState.AlphaBlend, SamplerState.PointWrap);
      else
        div.BeginRender(BlendState.AlphaBlend, SamplerState.PointWrap);
    }
    public virtual void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {

    }
  }
}