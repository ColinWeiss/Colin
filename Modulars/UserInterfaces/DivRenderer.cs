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

    /// <summary>
    /// 指示渲染器内容是否经由独立的直通 alpha 混合批次绘制.
    /// <br>文本等需要直通 alpha 混合的内容应保持 <see langword="true"/> (默认);</br>
    /// <br>内容与普通混合批次视觉等价时置为 <see langword="false"/>, 可避免批次的来回切换.</br>
    /// </summary>
    public bool UseTranslucent = true;

    public virtual void OnBinded() { }
    public virtual void OnDivInitialize() { }

    /// <summary>
    /// 测量渲染器内容所期望的尺寸, 供布局系统于自适应尺寸模式下使用.
    /// <br>任一分量为负表示渲染器无法测量内容尺寸; 可于子类重写以提供内容测量.</br>
    /// </summary>
    public virtual Vector2 MeasureContent() => new Vector2(-1f);

    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (UseTranslucent)
      {
        //半透明批次: 状态机于状态一致时不产生提交.
        div.BeginRender(BlendState.HumanityTranslucent, SamplerState.PointWrap);
        RenderStep(device, batch);
        div.BeginRender(BlendState.AlphaBlend, SamplerState.PointWrap);
      }
      else
      {
        //直接绘制于当前批次, 避免批次来回切换.
        RenderStep(device, batch);
      }
    }
    public virtual void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {

    }
  }
}