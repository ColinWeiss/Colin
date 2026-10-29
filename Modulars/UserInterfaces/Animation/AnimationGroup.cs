using System.Text.Json;
using Leemo.Assets;

namespace Colin.Core.Modulars.UserInterfaces.Animation
{
  /// <summary>
  /// 组合条目: 一个片段在组合里的出场参数 —— 延迟 (错峰编排) 与倍速.
  /// </summary>
  public sealed class AnimationGroupEntry
  {
    public string ClipName { get; set; } = string.Empty;
    /// <summary>起始延迟 (秒): 0 = 与组合同时起播, 正值错峰跟进.</summary>
    public float Delay { get; set; }
    public float Speed { get; set; } = 1f;
  }

  /// <summary>
  /// 动效组合 (页面打包): 一组片段 + 各自的延迟/倍速, 描述"一个页面启动后如何依次/同时播动效".
  /// <br>组合不持有片段数据 —— 条目按 <see cref="AnimationGroupEntry.ClipName"/> 从 <see cref="AnimationClipStore"/> 取用,
  /// 片段可以被多个组合复用. 以 JSON 持久化 (*.group.json), 由 <see cref="AnimationGroupStore"/> 按名取用.</br>
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
        throw new InvalidDataException($"动效组合版本过新: {group.Version} > {FormatVersion}");
      //旧数据修补: 空条目名剔除.
      group.Entries.RemoveAll(entry => string.IsNullOrWhiteSpace(entry.ClipName));
      return group;
    }

    public string ToJson()
      => JsonSerializer.Serialize(this, AnimationJson.Options);

    /// <summary>
    /// 在界面上播整组: 每个条目一个播放器, 按条目延迟/倍速编排; 播放器挂进界面的模块泵.
    /// <br>返回本组实例化的播放器列表, 停整组用 <see cref="Stop"/>. 片段缺失的条目跳过并忽略.</br>
    /// </summary>
    public List<AnimationPlayer> Play(UserInterface ui)
    {
      List<AnimationPlayer> players = new List<AnimationPlayer>();
      for (int index = 0; index < Entries.Count; index++)
      {
        AnimationGroupEntry entry = Entries[index];
        if (AnimationClipStore.TryGet(entry.ClipName, out AnimationClip clip) is false)
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
