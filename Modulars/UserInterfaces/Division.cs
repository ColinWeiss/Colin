﻿namespace Colin.Core.Modulars.UserInterfaces
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

        private bool isVisible = true;
        /// <summary>
        /// 指示划分元素是否可见.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                ForEach(a => a.IsVisible = value);
                isVisible = value;
            }
        }

        private bool isHidden = false;
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                ForEach(a => a.isHidden = value);
                isHidden = value;
            }
        }

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
            if (_renderer is T)
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
        public Division(string name)
        {
            Name = name;
            Events = new DivisionEventResponder(this);
            Interact.IsInteractive = true;
            Interact.IsBubbling = true;
            Design.Color = Color.White;
            Design.Scale = Vector2.One;
            Children = new List<Division>();
            Controller = new DivisionController(this);
        }

        /// <summary>
        /// 执行划分元素的初始化内容.
        /// </summary>
        public void DoInitialize()
        {
            if (InitializationCompleted)
                return;
            if (!(this is Container))
            {
                _interface = Parent?._interface;
                _container = Parent?._container;
            }
            OnInit();
            Controller?.OnInit();
            _renderer?.RendererInit();
            if (Parent != null)
                LayoutStyle.Calculation(this); //刷新一下.
            ForEach(child => child.DoInitialize());
            Events.DoInitialize();
            InitializationCompleted = true;
        }
        /// <summary>
        /// 发生于划分元素执行 <see cref="DoInitialize"/> 时, 可于此自定义初始化操作.
        /// </summary>
        public virtual void OnInit() { }

        private bool _started = false;

        /// <summary>
        /// 执行划分元素的逻辑刷新.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public void DoUpdate(GameTime time)
        {
            PreUpdate(time);
            if (!IsVisible)
                return;
            if (this is Container is false)
            {
                _interface = Parent?._interface;
                _container = Parent?._container;
            }
            if (!_started)
            {
                Start(time);
                _started = true;
            }
            Controller?.Layout(ref Layout);
            if (Parent != null)
                LayoutStyle.Calculation(this);
            Controller?.Interact(ref Interact);
            Controller?.Design(ref Design);
            Events.DoUpdate();
            OnUpdate(time);
            UpdateChildren(time);
        }
        /// <summary>
        /// 发生于 <see cref="DoUpdate(GameTime)"/> 第一帧执行时.
        /// </summary>
        public virtual void Start(GameTime time) { }
        /// <summary>
        /// 发生于 <see cref="DoUpdate"/> 执行时, 但不受 <see cref="IsVisible"/> 控制.
        /// <br>相较于 <see cref="UpdateChildren"/> 与 <see cref="OnUpdate"/> 最先执行.</br>
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void PreUpdate(GameTime time) { }
        /// <summary>
        /// 发生于 <see cref="DoUpdate"/> 执行时, 受 <see cref="IsVisible"/> 控制.
        /// <br>相较于 <see cref="UpdateChildren"/> 更快执行.</br>
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void OnUpdate(GameTime time) { }
        /// <summary>
        /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoUpdate"/>.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void UpdateChildren(GameTime time)
        {
            Children.ForEach(child =>
            {
                if (Layout.ScissorEnable && child.Layout.TotalHitBox.Intersects(Layout.TotalHitBox))
                    child?.DoUpdate(time);
                else
                    child?.DoUpdate(time);
            });
        }

        /// <summary>
        /// 执行划分元素的渲染.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            if (!IsVisible && !IsHidden)
                return;
            if (IsCanvas)
            {
                batch.End();
                device.SetRenderTarget(Canvas);
                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Layout.CanvasTransform);
                device.Clear(Color.Transparent);
            }
            var rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            if (Layout.ScissorEnable)
            {
                batch.End();
                device.ScissorRectangle = Layout.Scissor;
                if (IsCanvas)
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizerState, transformMatrix: Layout.CanvasTransform);
                else if (ParentCanvas == null)
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizerState);
                else
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, rasterizerState: rasterizerState, transformMatrix: ParentCanvas?.Layout.CanvasTransform);
            }
            _renderer?.DoRender(device, batch);//渲染器进行渲染.
            RenderChildren(device, batch);
            if (Layout.ScissorEnable)
            {
                batch.End();
                batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: ParentCanvas?.Layout.CanvasTransform);
            }
            if (IsCanvas)
            {
                batch.End();
                if (Parent.IsCanvas)
                    device.SetRenderTarget(Parent.Canvas);
                if (ParentCanvas != null)
                    device.SetRenderTarget(ParentCanvas.Canvas);
                else
                    device.SetRenderTarget(Interface.RawRt);
                if (Parent.IsCanvas)
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Parent.Layout.CanvasTransform);
                else if (ParentCanvas != null)
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: ParentCanvas.Layout.CanvasTransform);
                else
                    batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                batch.Draw(Canvas, Layout.TotalLocationF + Design.Anchor, null, Design.Color , 0f, Design.Anchor, Design.Scale, SpriteEffects.None, 0f);
            }
        }
        /// <summary>
        /// 为 <see cref="Children"/> 内元素执行其 <see cref="DoRender"/>.
        /// </summary>
        /// <param name="time">游戏计时状态快照.</param>
        public virtual void RenderChildren(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            Children.ForEach(child =>
            {
                child?.DoRender(device, spriteBatch);
            });
        }

        /// <summary>
		/// 添加子元素.
		/// </summary>
		/// <param name="division">需要添加的划分元素.</param>
		/// <param name="doInit">执行添加划分元素的初始化.</param>
		/// <returns>若添加成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
		public virtual bool Register(Division division, bool doInit = false)
        {
            division.Parent = this;
            division.ParentCanvas = ParentCanvas;
            Events.Mouse.Register(division.Events.Mouse);
            Events.Keys.Register(division.Events.Keys);
            if (IsCanvas)
                division.ParentCanvas = this;
            if (doInit)
                division.DoInitialize();
            division._interface = _interface;
            division._container = _container;
            Children.Add(division);
            return true;
        }

        /// <summary>
		/// 移除子元素.
		/// </summary>
		/// <param name="division">需要移除的划分元素.</param>
		/// <returns>若移除成功, 返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
		public virtual bool Remove(Division division)
        {
            division.Parent = null;
            division.ParentCanvas = null;
            division._container = null;
            division._interface = null;
            return Children.Remove(division);
        }

        /// <summary>
        /// 移除所有子元素.
        /// </summary>
        public virtual void Clear(bool dispose = false)
        {
            Division _div;
            for (int count = 0; count < Children.Count; count++)
            {
                _div = Children[count];
                Remove(_div);
                if (dispose)
                    _div.Dispose();
            }
            Children.Clear();
        }

        /// <summary>
		/// 遍历划分元素, 并执行传入方法.
		/// </summary>
		/// <param name="action">要执行的方法.</param>
		public void ForEach(Action<Division> action)
        {
            Division _div;
            for (int count = 0; count < Children.Count; count++)
            {
                _div = Children[count];
                action.Invoke(_div);
            }
        }

        public void Do(Action<Division> action) => action(this);

        /// <summary>
		/// 判断该划分元素是否包含指定点.
		/// </summary>
		/// <param name="point">输入的点.</param>
		/// <returns>如果包含则返回 <see langword="true"/>，否则返回 <see langword="false"/>.</returns>
		public virtual bool ContainsPoint(Point point)
        {
            return Layout.TotalHitBox.Contains(point);
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Canvas?.Dispose();
                    for (int count = 0; count < Children.Count; count++)
                        Children[count].Dispose();
                    OnDispose?.Invoke();
                }
                _renderer = null;
                disposedValue = true;
            }
        }
        public event Action OnDispose;
        ~Division()
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