namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 指代用户交互界面中的一个划分元素.
    /// </summary>
    public class Division : IDisposable
    {
        /// <summary>
        /// 划分元素的名称.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 指示划分元素是否可见.
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// 划分元素的布局样式
        /// </summary>
        public LayoutStyle Layout;

        /// <summary>
        /// 划分元素的交互样式.
        /// </summary>
        public InteractStyle Interact;

        /// <summary>
        /// 划分元素的设计样式.
        /// </summary>
        public DesignStyle Design;

        /// <summary>
        /// 划分元素的事件响应器.
        /// </summary>
        public DivisionEventResponder Events;

        private DivisionRenderer _renderer;
        /// <summary>
        /// 划分元素的渲染器.
        /// </summary>
        public DivisionRenderer Renderer => _renderer;
        public T BindRenderer<T>() where T : DivisionRenderer, new()
        {
            _renderer = new T();
            _renderer._division = this;
            _renderer.RendererInit();
            return _renderer as T;
        }
        public T GetRenderer<T>() where T : DivisionRenderer
        {
            if(_renderer is T)
                return _renderer as T;
            else
                return null;
        }
        public void ClearRenderer() => _renderer = null;

        /// <summary>
        /// 划分元素控制器.
        /// </summary>
        public DivisionController Controller;

        /// <summary>
        /// 划分元素的父元素.
        /// </summary>
        public Division Parent;

        /// <summary>
        /// 划分元素可溯到的最近的 Canvas 元素.
        /// </summary>
        public Division ParentCanvas;

        /// <summary>
        /// 划分元素的子元素列表.
        /// </summary>
        public List<Division> Children;

        public RenderTarget2D Canvas;

        internal UserInterface _interface;
        public UserInterface Interface => _interface;

        internal Container _container;
        public Container Container => _container;

        public virtual bool IsCanvas => false;

        public bool InitializationCompleted = false;

        /// <summary>
        /// 实例化一个划分元素, 并用名称加以区分.
        /// <br>[!] 虽然此处的名称可重复, 但该名称的作用是利于调试, 故建议使用不同的、可辨识的名称加以区分.</br>
        /// </summary>
        /// <param name="name">划分元素的名称.</param>
        public Division( string name )
        {
            Name = name;
            Events = new DivisionEventResponder( this );
            Events.DragStart += Container_DragStart;
            Events.Dragging += Container_DragDragging;
            Events.DragEnd += Container_DragEnd;
            Interact.IsInteractive = true;
            Design.Color = Color.White;
            Design.Scale = Vector2.One;
            IsVisible = true;
            Children = new List<Division>();
            Controller = new DivisionController( this );
        }

        /// <summary>
        /// 执行划分元素的初始化内容.
        /// </summary>
        public void DoInitialize()
        {
            if(InitializationCompleted)
                return;
            OnInit();
            Controller?.OnInit();
            _renderer?.RendererInit();
            if(Parent != null)
                Layout.Calculation( Parent.Layout ); //刷新一下.
            ForEach( child => child.DoInitialize() );
            InitializationCompleted = true;
        }
        /// <summary>
        /// 发生于划分元素执行 <see cref="DoInitialize"/> 时, 可于此自定义初始化操作.
        /// </summary>
        public virtual void OnInit() { }
        private Point _cachePos = new Point( -1, -1 );
        private void Container_DragStart( object o, DivisionEvent e )
        {
            if(Parent != null)
            {
                Point mouseForParentLocation = MouseResponder.State.Position - Parent.Layout.Location;
                _cachePos = mouseForParentLocation - Layout.Location;
            }
            else
            {
                _cachePos = MouseResponder.State.Position - Layout.Location;
            }
        }
        private void Container_DragDragging( object o, DivisionEvent e )
        {
            if(Parent != null)
            {
                Point _resultLocation = MouseResponder.State.Position - Parent.Layout.Location - _cachePos;
                Layout.Left = _resultLocation.X;
                Layout.Top = _resultLocation.Y;
            }
            else
            {
                Point _resultLocation = MouseResponder.State.Position - _cachePos;
                Layout.Left = _resultLocation.X;
                Layout.Top = _resultLocation.Y;
            }
            if(Interact.IsDraggable && Interact.DragLimit != Rectangle.Empty)
            {
                Layout.Left = Math.Clamp( Layout.Left, 0, Interact.DragLimit.Width - Layout.Width );
                Layout.Top = Math.Clamp( Layout.Top, 0, Interact.DragLimit.Height - Layout.Height );
            }
        }
        private void Container_DragEnd( object o, DivisionEvent e )
        {
            _cachePos = new Point( -1, -1 );
        }

        private bool _started = false;

        /// <summary>
        /// 执行划分元素的逻辑刷新.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void DoUpdate( GameTime time )
        {
            PreUpdate( time );
            if(!IsVisible)
                return;
            if(Parent is Container container)
            {
                _interface = container._interface;
                _container = container;
            }
            else if(!(this is Container))
            {
                _interface = Parent?._interface;
                _container = Parent?._container;
            }
            if(!_started)
            {
                Start( time );
                _started = true;
            }
            Events.Independent();
            Controller?.Layout( ref Layout );
            if(Parent != null)
                Layout.Calculation( Parent.Layout );
            Controller?.Interact( ref Interact );
            Controller?.Design( ref Design );
            OnUpdate( time );
            UpdateChildren( time );
        }
        /// <summary>
        /// 发生于 <see cref="DoUpdate(GameTime)"/> 第一帧执行时.
        /// </summary>
        public virtual void Start( GameTime time ) { }
        /// <summary>
        /// 发生于 <see cref="DoUpdate"/> 执行时, 但不受 <see cref="IsVisible"/> 控制.
        /// <br>相较于 <see cref="UpdateChildren"/> 与 <see cref="OnUpdate"/> 最先执行.</br>
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void PreUpdate( GameTime time ) { }
        /// <summary>
        /// 发生于 <see cref="DoUpdate"/> 执行时, 受 <see cref="IsVisible"/> 控制.
        /// <br>相较于 <see cref="UpdateChildren"/> 更快执行.</br>
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void OnUpdate( GameTime time ) { }
        /// <summary>
        /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoUpdate"/>.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void UpdateChildren( GameTime time )
        {
            Children.ForEach( child =>
            {
                if(Layout.ScissorEnable && child.Layout.TotalHitBox.Intersects( Layout.TotalHitBox ))
                    child?.DoUpdate( time );
                else
                    child?.DoUpdate( time );
            } );
        }

        /// <summary>
        /// 执行划分元素的渲染.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public void DoRender( SpriteBatch batch )
        {
            if(!IsVisible && !Layout.IsHidden)
                return;
            if(IsCanvas)
            {
                batch.End();
                EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( Canvas );
                batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Layout.CanvasTransform );
                EngineInfo.Graphics.GraphicsDevice.Clear( Color.Transparent );
            }
            var rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            if(Layout.ScissorEnable)
            {
                batch.End();
                EngineInfo.Graphics.GraphicsDevice.ScissorRectangle = Layout.Scissor;
                if(IsCanvas)
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizerState, transformMatrix: Layout.CanvasTransform );
                else if(ParentCanvas == null)
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizerState );
                else
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: ParentCanvas?.Layout.CanvasTransform );
            }
            _renderer?.DoRender( batch );//渲染器进行渲染.
            RenderChildren( batch );
            if(Layout.ScissorEnable)
            {
                batch.End();
                batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: ParentCanvas?.Layout.CanvasTransform );
            }
            if(IsCanvas)
            {
                batch.End();
                if(Parent.IsCanvas)
                    EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( Parent.Canvas );
                if(ParentCanvas != null)
                    EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( ParentCanvas.Canvas );
                else
                    EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( Interface.SceneRt );

                //     batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp );
                if(Parent.IsCanvas)
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Parent.Layout.CanvasTransform );
                else if(ParentCanvas != null)
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: ParentCanvas.Layout.CanvasTransform );
                else
                    batch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp );
                batch.Draw( Canvas, Layout.TotalLocationF + Design.Anchor, null, Design.Color, 0f, Design.Anchor, Design.Scale, SpriteEffects.None, 0f );
            }
        }
        /// <summary>
        /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoRender"/>.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void RenderChildren( SpriteBatch spriteBatch )
        {
            Children.ForEach( child =>
            {
                //   if( Layout.ScissorEnable && child.Layout.TotalHitBox.Intersects( Layout.TotalHitBox ) )
                //       child?.DoRender( spriteBatch );
                //    else
                child?.DoRender( spriteBatch );
            } );
        }

        /// <summary>
		/// 添加子元素.
		/// </summary>
		/// <param name="division">需要添加的划分元素.</param>
		/// <param name="doInit">执行添加划分元素的初始化.</param>
		/// <returns>若添加成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
		public virtual bool Register( Division division, bool doInit = false )
        {
            division.Parent = this;
            division.ParentCanvas = ParentCanvas;
            if(IsCanvas)
                division.ParentCanvas = this;
            if(doInit)
            {
                division.DoInitialize();
                division.Layout.Calculation( Layout );
            }
            division._interface = _interface;
            division._container = _container;
            Children.Add( division );
            return true;
        }

        /// <summary>
		/// 移除子元素.
		/// </summary>
		/// <param name="division">需要移除的划分元素.</param>
		/// <returns>若移除成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
		public virtual bool Remove( Division division )
        {
            division.Parent = null;
            division.ParentCanvas = null;
            division._container = null;
            division._interface = null;
            return Children.Remove( division );
        }

        /// <summary>
        /// 移除所有子元素.
        /// </summary>
        public virtual void Clear()
        {
            Division _div;
            for(int count = 0; count < Children.Count; count++)
            {
                _div = Children[count];
                Remove( _div );
            }
            Children.Clear();
        }

        /// <summary>
		/// 遍历划分元素, 并执行传入方法.
		/// </summary>
		/// <param name="action">要执行的方法.</param>
		public void ForEach( Action<Division> action )
        {
            Division _div;
            for(int count = 0; count < Children.Count; count++)
            {
                _div = Children[count];
                action.Invoke( _div );
            }
        }

        public void Do( Action<Division> action ) => action( this );

        /// <summary>
		/// 判断该划分元素是否包含指定点.
		/// </summary>
		/// <param name="point">输入的点.</param>
		/// <returns>如果包含则返回 <see langword="true"/>，否则返回 <see langword="false"/>.</returns>
		public virtual bool ContainsPoint( Point point ) => Layout.TotalHitBox.Contains( point );

        /// <summary>
        /// 返回该划分元素下最先可进行交互的元素.
        /// </summary>
        /// <returns>如果寻找到非该划分元素之外的元素, 则返回寻找到的元素; 否则返回自己.</returns>
        public virtual Division Seek()
        {
            Division target = null;
            Division child;
            for(int sub = Children.Count - 1; sub >= 0; sub--)
            {
                child = Children[sub];
                if(child.Seek() == null)
                    target = null;
                else if(child.Seek() != null && child.IsVisible)
                {
                    target = child.Seek();
                    return target;
                }
            }
            if(Interact.IsInteractive && IsVisible && Interact.Interaction)
                return this;
            return target;
        }

        private bool disposedValue;
        protected virtual void Dispose( bool disposing )
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    Canvas?.Dispose();
                    for(int count = 0; count < Children.Count; count++)
                        Children[count].Dispose();
                }
                _renderer = null;
                disposedValue = true;
            }
        }
        ~Division()
        {
            Dispose( disposing: false );
        }
        public void Dispose()
        {
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}