using Colin.Core.Events;
using SharpDX.XInput;

namespace Colin.Core.Modulars.Ecses
{
    /// <summary>
    /// ECS: Entity Component System.
    /// <br>但我将称它为 Environmental Control Section.</br>
    /// <br>「环境控制切片」</br>
    /// <br>这一系列会影响到环境的游戏元素, 我们统称为 <see cref="Section"/>.</br>
    /// </summary>
    public class Ecs : ISceneModule, IRenderableISceneModule
    {
        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public bool RawRtVisible { get; set; }

        public bool Presentation { get; set; }

        public RenderTarget2D RawRt { get; set; }

        public EnvironmentalController Controller;

        private Dictionary<Type, SectionSystem> _systems;
        public Dictionary<Type, SectionSystem> Systems => _systems;
        public void AddSystem(SectionSystem system)
        {
            system._ecs = this;
            system.DoInitialize();
            _systems.Add(system.GetType(), system);
        }
        public T GetSystem<T>() where T : SectionSystem => (T)_systems.GetValueOrDefault(typeof(T));

        public Section[] Sections;

        public KeysEventResponder KeysEvent;

        /// <summary>
        /// 事件: 于初始化时添加游戏系统.
        /// </summary>
        public event Action<Ecs> AddSystems;

        public void DoInitialize()
        {
            Controller = new EnvironmentalController();
            Controller.DoInitialize();
            KeysEvent = new KeysEventResponder("Ecs.KeysEvent");
            Scene.Events.KeysEvent.Register(KeysEvent);
            Sections = new Section[2000];

            _systems = new Dictionary<Type, SectionSystem>();
            AddSystems?.Invoke(this);

            //     _localPlayer.GetComponent<EcsComTransform>().Position = new Vector2( 500, 500 );
            //在此处添加切片系统.
        }
        public void Start()
        {
            Section _section;
            SectionSystem _system;
            for (int count = 0; count < Sections.Length; count++)
            {
                _section = Sections[count];
                if (_section is null)
                    continue;
                for (int sCount = 0; sCount < _systems.Values.Count; sCount++)
                {
                    _system = _systems.Values.ElementAt(sCount);
                    _system._current = _section;
                    _system.Start();
                }
            }
        }
        public void DoUpdate(GameTime time)
        {
            Perfmon.Start();
            Controller.Reset();

            Section _section;
            SectionSystem _system;
            for (int count = 0; count < Sections.Length; count++)
            {
                _section = Sections[count];
                if (_section is null)
                    continue;
                if (Sections[count].NeedClear)
                {
                    Sections[count] = null;
                    continue;
                }
                for (int rCount = 0; rCount < _systems.Values.Count; rCount++)
                {
                    _system = _systems.Values.ElementAt(rCount);
                    _system._current = _section;
                    _system.Reset();
                }
                for (int uCount = 0; uCount < _systems.Values.Count; uCount++)
                {
                    _system = _systems.Values.ElementAt(uCount);
                    _system._current = _section;
                    _system.DoUpdate();
                }
            }
            Perfmon.End("Ecs.DoUpdate");
        }
        public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
        {
            Perfmon.Start();
            Section _section;
            SectionSystem _system;
            for (int count = 0; count < Sections.Length; count++)
            {
                _section = Sections[count];
                if (_section is null)
                    continue;
                for (int rCount = 0; rCount < _systems.Values.Count; rCount++)
                {
                    _system = _systems.Values.ElementAt(rCount);
                    _system._current = _section;
                    _system.DoRender(device, batch);
                }
            }
            Perfmon.End("Ecs.DoRender");
        }
        public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) { }

        /// <summary>
        /// 将指定对象格清空.
        /// </summary>
        /// <param name="index"></param>
        public void SetNull(int index)
        {
            Sections[index] = null;
        }
        public T Create<T>() where T : Section, new()
        {
            T result;
            Section section;
            for (int count = 0; count < Sections.Length; count++)
            {
                section = Sections[count];
                if (section is null)
                {
                    result = new T();
                    result.Ecs = this;
                    result.ID = count;
                    result.DoInitialize();
                    Sections[count] = result;
                    return result;
                }
            }
            return null;
        }
        public Section Create(Section section)
        {
            for (int count = 0; count < Sections.Length; count++)
            {
                if (Sections[count] is null)
                {
                    Sections[count] = section;
                    section.ID = count;
                    section.Ecs = this;
                    section.DoInitialize();
                    return section;
                }
            }
            return null;
        }

        public void Dispose()
        {
            Scene = null;
            _systems = null;
            Sections = null;
        }
    }
}