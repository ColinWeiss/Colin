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

    /// <summary>
    /// 测量渲染器内容所期望的尺寸, 供布局系统于自适应尺寸模式下使用.
    /// <br>任一分量为负表示渲染器无法测量内容尺寸; 可于子类重写以提供内容测量.</br>
    /// </summary>
    public virtual Vector2 MeasureContent() => new Vector2(-1f);

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