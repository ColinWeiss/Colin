namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  /// <summary>SDF 图标种类, 与 SdfIcon.fx 里的 IconKind 一一对应.</summary>
  public enum SdfIconKind
  {
    /// <summary>齿轮↔圆环, 形态由 morph 参数控制.</summary>
    Gear = 0,
    /// <summary>闪电(↯), 表示重新加载/生成.</summary>
    Bolt = 1,
    /// <summary>落进托盘的箭头(⇲), 表示安装.</summary>
    Install = 2,
    /// <summary>三条横线(≡), 表示打开目录/列表.</summary>
    Menu = 3,
    /// <summary>叉号(×), 表示关闭.</summary>
    Close = 4,
  }

  /// <summary>
  /// SDF 图标渲染器: 走 Texture+Shader 的路子.
  /// <br>拿白像素画个方块, 到边缘的距离场和覆盖率由像素着色器在 GPU 上现算——
  /// 不烘纹理, 尺寸随便变、形态随便动, 都是零成本的常量更新;
  /// 想加新图标也只是在着色器里添一个场函数, 可扩展性在这条路上才立得住.</br>
  /// <br>颜色取自 <see cref="DivDesign.Color"/>; <see cref="SetMorph"/> 用来在两个形态之间做渐变过渡.</br>
  /// </summary>
  public class DivSdfIconRenderer : DivRenderer
  {
    private static Effect _effect;
    private Sprite _pixel;
    private SdfIconKind _kind;
    private float _morph;

    public override void OnDivInitialize()
    {
      _pixel = Sprite.Get("Pixel");
      //特效懒加载, 用到才编, 图形设备没就绪也不碍事.
      _effect ??= Assets.Effect("Effects/SdfIcon.fx");
    }

    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      int size = (int)MathF.Min(Div.Layout.Width, Div.Layout.Height);
      if (size <= 0)
        return;

      //和 Div.BeginRender 同一套变换: 位于画布内用单位阵, 否则吃 UI 相机.
      Matrix transform = Div.UpperCanvas is null ? Div.Module.UICamera.View : Matrix.Identity;
      Matrix mvp = transform
        * Matrix.CreateOrthographicOffCenter(0f, device.Viewport.Width, device.Viewport.Height, 0f, 0f, -1f);
      _effect.Parameters["MatrixTransform"].SetValue(mvp);
      _effect.Parameters["IconKind"].SetValue((float)_kind);
      _effect.Parameters["Morph"].SetValue(Math.Clamp(_morph, 0f, 1f));
      _effect.Parameters["HalfSize"].SetValue(size / 2f);

      //自己的批次: 先把界面批次提交掉, 挂着特效重开一段画完就收,
      //批次状态机发现没有激活批次, 自然会把普通批次接回去.
      UIBatch.Flush();
      bool scissored = Div.UpperScissor is not null;
      if (scissored)
        device.ScissorRectangle = Div.ScissorBounds;
      CoreInfo.Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
        scissored ? DepthStencilState.Default : DepthStencilState.None,
        scissored ? UIBatch.ScissorTestRasterizer : UIBatch.DefaultRasterizer,
        _effect);
      Vector2 location = Div.Layout.RenderTargetLocation;
      int left = (int)(location.X + (Div.Layout.Width - size) / 2f);
      int top = (int)(location.Y + (Div.Layout.Height - size) / 2f);
      CoreInfo.Batch.Draw(_pixel.Source, new Rectangle(left, top, size, size), null,
        Div.Design.Color, 0f, Vector2.Zero, SpriteEffects.None, _pixel.Depth);
      CoreInfo.Batch.End();
    }

    /// <summary>绑一个图标.</summary>
    public DivSdfIconRenderer Bind(SdfIconKind kind)
    {
      _kind = kind;
      return this;
    }

    /// <summary>设形态渐变参数: 0 是初始形态, 1 是目标形态, 中间值就是"变形中"的那一帧.</summary>
    public DivSdfIconRenderer SetMorph(float morph)
    {
      _morph = Math.Clamp(morph, 0f, 1f);
      return this;
    }
  }
}
