﻿using Colin.Core.Resources;
using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses
{
    /// <summary>
    /// 切片.
    /// </summary>
    public class Section
    {
        internal Dictionary<Type, ISectionComponent> _components;
        /// <summary>
        /// 切片组件列表.
        /// </summary>
        public Dictionary<Type, ISectionComponent> Components => _components;
        public bool HasComponent<T>() where T : ISectionComponent => _components.ContainsKey(typeof(T));
        public T GetComponent<T>() where T : ISectionComponent => (T)_components.GetValueOrDefault(typeof(T), null);
        public T RegisterCom<T>() where T : class, ISectionComponent, new() => Register(new T()) as T;
        public bool RemoveComponent<T>() where T : ISectionComponent => _components.Remove(typeof(T));
        public ISectionComponent Register(ISectionComponent component)
        {
            if (component is EcsComScript script)
            {
                script.SetSection(this);
            }
            if (Components.ContainsKey(component.GetType()) is false)
                Components.Add(component.GetType(), component);
            return component;
        }
        public Ecs Ecs { get; internal set; }

        /// <summary>
        /// 获取切片运行时 ID.
        /// </summary>
        public int ID;

        /// <summary>
        /// 指示该切片需要被清除.
        /// </summary>
        public bool NeedClear;

        public EcsComDoc _comDoc;
        public EcsComDoc Document => _comDoc;
        public void AddTag(string tag) => _comDoc.Tags.Add(tag);

        private EcsComTransform _comTransform;
        public EcsComTransform Transform => _comTransform;

        public void DoInitialize()
        {
            _components = new Dictionary<Type, ISectionComponent>();
            _comDoc = RegisterCom<EcsComDoc>();
            _comTransform = RegisterCom<EcsComTransform>();
            SetDefaults();
            for (int count = 0; count < _components.Count; count++)
                _components.Values.ElementAt(count).DoInitialize();
            OnInitializeComplete();
        }
        public virtual void SetDefaults() { }
        public virtual void OnInitializeComplete() { }
    }
}