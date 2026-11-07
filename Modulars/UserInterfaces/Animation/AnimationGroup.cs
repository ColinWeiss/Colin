using System.Text.Json;
using Leemo.Assets;

namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>
  /// 动效条目: 一段时间轴在动效里的出场参数 —— 段数据内嵌 (<see cref="Clip"/>), 附加延迟 (错峰编排) 与倍速.
  /// <br>旧文件只有 <see cref="ClipName"/>: 播放时回退 <see cref="AnimationClipStore"/> 按名取用, 编辑器载入后会物化进 Clip.</br>
  /// </summary>
  public sealed class AnimationGroupEntry
  {
    public string ClipName { get; set; } = string.Empty;
    /// <summary>内嵌的段时间轴 (布局是布局, 行为是行为: 动效文件自包含, 不再依赖外部片段文件).</summary>
    public AnimationClip Clip { get; set; }
    /// <summary>起始延迟 (秒): 0 = 与动效同时起播, 正值错峰跟进.</summary>
    public float Delay { get; set; }
    public float Speed { get; set; } = 1f;
  }

  /// <summary>
  /// 动效 (页面打包): 一组段时间轴 + 各自的延迟/倍速, 描述"一个页面启动后如何依次/同时播动效".
  /// <br>段数据内嵌在条目里, 动效文件 (*.group.json) 自包含; 旧文件的按名引用在播放时回退
  /// <see cref="AnimationClipStore"/> 取用. 由 <see cref="AnimationGroupStore"/> 按名取用.</br>
  /// </summary>
  public sealed class AnimationGroup
  {
    public const int FormatVersion = 1;

    public int Version { get; set; } = FormatVersion;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "NewGroup";
    public List<AnimationGroupEntry> Entries { get; set; } = new List<AnimationGroupEntry>();

    public static AnimationGroup FromJson(string json)
    {
      AnimationGroup group = JsonSerializer.Deserialize<AnimationGroup>(json, AnimationJson.Options);
      if (group is null)
        return null;
      if (group.Version > FormatVersion)
        throw new InvalidDataException($"动效版本过新: {group.Version} > {FormatVersion}");
      //旧数据修补: 既没内嵌段又没名字的空条目剔除.
      group.Entries.RemoveAll(entry => string.IsNullOrWhiteSpace(entry.ClipName) && entry.Clip is null);
      //条目名与内嵌段名对齐 (旧文件只有名字, 新文件以段为准)
      for (int index = 0; index < group.Entries.Count; index++)
      {
        AnimationClip clip = group.Entries[index].Clip;
        if (clip is not null && string.IsNullOrWhiteSpace(clip.Name) is false)
          group.Entries[index].ClipName = clip.Name;
      }
      return group;
    }

    public string ToJson()
      => JsonSerializer.Serialize(this, AnimationJson.Options);

    /// <summary>
    /// 在界面上播整组: 每个条目一个播放器, 按条目延迟/倍速编排; 播放器挂进界面的模块泵.
    /// <br>返回本组实例化的播放器列表, 停整组用 <see cref="Stop"/>. 段缺失的条目跳过并忽略.</br>
    /// </summary>
    public List<AnimationPlayer> Play(UserInterface ui)
    {
      List<AnimationPlayer> players = new List<AnimationPlayer>();
      for (int index = 0; index < Entries.Count; index++)
      {
        AnimationGroupEntry entry = Entries[index];
        AnimationClip clip = entry.Clip;
        if (clip is null && AnimationClipStore.TryGet(entry.ClipName, out clip) is false)
          continue;
        AnimationPlayer player = new AnimationPlayer { Clip = clip, Delay = MathF.Max(0f, entry.Delay), Speed = entry.Speed <= 0f ? 1f : entry.Speed };
        player.Play(ui);
        ui.AnimationPlayers.Add(player);
        players.Add(player);
      }
      return players;
    }

    /// <summary>停掉一组播放器: 还原编辑值并撤出模块泵 (与 <see cref="AnimationGroup.Play"/> 成对).</summary>
    public static void Stop(UserInterface ui, List<AnimationPlayer> players)
    {
      if (players is null)
        return;
      for (int index = 0; index < players.Count; index++)
      {
        players[index].Stop(ui);
        ui.AnimationPlayers.Remove(players[index]);
      }
      players.Clear();
    }
  }

  /// <summary>片段仓库 (键 = 片段名): 预加载器与编辑器共同写入, 组合播放按名取用.</summary>
  public static class AnimationClipStore
  {
    private static readonly Dictionary<string, AnimationClip> _clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);

    public static IEnumerable<string> Names => _clips.Keys;

    public static bool Has(string name) => _clips.ContainsKey(name);

    public static bool TryGet(string name, out AnimationClip clip)
      => _clips.TryGetValue(name, out clip);

    /// <summary>登记/覆盖 (键 = 片段名): 重名以最后登记者为准.</summary>
    public static void Register(AnimationClip clip)
    {
      if (clip is null || string.IsNullOrWhiteSpace(clip.Name))
        return;
      _clips[clip.Name] = clip;
    }

    public static bool Remove(string name)
      => _clips.Remove(name);
  }

  /// <summary>动效组合仓库 (键 = 组合名): 预加载器与编辑器共同写入.</summary>
  public static class AnimationGroupStore
  {
    private static readonly Dictionary<string, AnimationGroup> _groups = new Dictionary<string, AnimationGroup>(StringComparer.Ordinal);

    public static IEnumerable<string> Names => _groups.Keys;

    public static bool Has(string name) => _groups.ContainsKey(name);

    public static bool TryGet(string name, out AnimationGroup group)
      => _groups.TryGetValue(name, out group);

    public static void Register(AnimationGroup group)
    {
      if (group is null || string.IsNullOrWhiteSpace(group.Name))
        return;
      _groups[group.Name] = group;
    }

    public static bool Remove(string name)
      => _groups.Remove(name);
  }

  /// <summary>动画资产目录: 片段 (*.anim.json) 与组合 (*.group.json) 统一落内容根 UserInterfaces/Animations.</summary>
  public static class AnimationAssets
  {
    public static string Directory => Path.Combine(Assets.Manager.RootDir, "UserInterfaces", "Animations");
  }
}
