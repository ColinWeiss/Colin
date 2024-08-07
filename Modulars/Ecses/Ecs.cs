﻿using Colin.Core.Events;
using Colin.Core.Modulars.Tiles;

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

    [Obsolete()]
    public void AddSystem(SectionSystem system)
    {
      system._ecs = this;
      system.DoInitialize();
      _systems.Add(system.GetType(), system);
    }
    public T RegisterSystem<T>() where T : SectionSystem, new()
    {
      T system = new T();
      system._ecs = this;
      system.DoInitialize();
      _systems.Add(typeof(T), system);
      return system;
    }
    public T GetSystem<T>() where T : SectionSystem => (T)_systems.GetValueOrDefault(typeof(T));

    public Section[] Sections;

    public KeysEventResponder KeysEvent;

    public void DoInitialize()
    {
      Controller = new EnvironmentalController();
      Controller.DoInitialize();
      KeysEvent = new KeysEventResponder("Ecs.KeysEvent");
      Scene.Events.KeysEvent.Register(KeysEvent);
      Sections = new Section[2000];
      _systems = new Dictionary<Type, SectionSystem>();
    }
    public void Start()
    {
      SectionSystem _system;
      for (int sCount = 0; sCount < _systems.Values.Count; sCount++)
      {
        _system = _systems.Values.ElementAt(sCount);
        _system.Start();
      }
    }

    public void DoUpdate(GameTime time)
    {
      Controller.Reset();
      Section _section;
      SectionSystem _currentSystem;
      for (int count = 0; count < Systems.Count; count++)
      {
        _currentSystem = Systems.ElementAt(count).Value;
        _currentSystem.Reset();
      }
      for (int count = 0; count < Systems.Count; count++)
      {
        _currentSystem = Systems.ElementAt(count).Value;
        _currentSystem.DoUpdate();
      }
      for (int count = 0; count < Sections.Length; count++)
      {
        _section = Sections[count];
        if (_section is null)
          continue;
        if (_section.NeedClear)
        {
          Sections[count] = null;
          continue;
        }
      }
    }
    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      device.Clear(Color.Transparent);
      SectionSystem _system;
      for (int count = 0; count < _systems.Values.Count; count++)
      {
        _system = _systems.Values.ElementAt(count);
        _system.DoRender(device, batch);
      }
    }
    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) 
    {
     // device.SetRenderTarget(LightingAdpter.RawRt);
    }

    /// <summary>
    /// 将指定对象格清空.
    /// </summary>
    /// <param name="index"></param>
    public void SetNull(int index)
    {
      Sections[index].NeedClear = true;
    }
    public T CreateSection<T>() where T : Section, new()
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
    }
  }
}