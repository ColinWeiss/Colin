# GPU 粒子系统 (Particle.*)

基于 **MonoGame + ComputeSharp + MonoGame.ImGUI** 的通用 GPU 粒子系统与可视化编辑器。
位于 `Colin/Graphics/Particles/`（随 Colin.Core 共享项目编译进游戏程序集），按模块划分为五个命名空间：

| 命名空间 | 职责 | 依赖 |
|---|---|---|
| `Particle.Core` | 粒子数据、曲线、配置、发射器/效果运行时、管理器、策略接口 | 无 UI / 无渲染依赖 |
| `Particle.Compute` | ComputeSharp 更新着色器、D3D11↔D3D12 共享缓冲区、GPU 策略 | ComputeSharp |
| `Particle.Rendering` | HLSL 着色器、Effect 进程内编译、实例化渲染器、程序化纹理 | MonoGame |
| `Particle.Serialization` | 配置 JSON 保存 / 读取 / 深拷贝 | System.Text.Json |
| `Particle.Effects` | 预设工厂（刀光/爆炸/火焰/烟雾）、刀光代码式发射 API | Core |
| `Particle.Editor` | ImGUI 可视化编辑器（可选，移除后核心不受影响） | ImGuiNET |

---

## 1. 快速开始（纯代码，三行）

```csharp
// 任意时刻（首次使用会自动初始化，也可显式初始化）：
ParticleManager.Instance.Initialize(GraphicsDevice);

// 加载预设并播放 —— 一键刀光：
ParticleEffect slash = ParticleManager.Instance.Play(ParticlePresetFactory.Create("刀光"), position);
```

### 与游戏循环集成

引擎已在 `EcsAdvancedRenderSystem.DoRender` 中挂钩（实体渲染层之上自动更新+绘制），
游戏代码**无需**手动调用 Update/Render：

```csharp
// Colin/Modulars/Ecses/Systems/EcsAdvancedRenderSystem.cs (已内置)
Particle.Core.ParticleManager.Instance.TickAndRender(Time.DeltaTime, Ecs.Scene.Camera.Transform);
```

如需手动驱动（自定义管线/编辑器）：

```csharp
ParticleManager.Instance.UpdateAll(Time.DeltaTime);      // 每帧一次 (帧内守卫, 重复调用安全)
ParticleManager.Instance.RenderAll(cameraTransform);     // 可在不同相机下多次调用
```

---

## 2. 核心类型

### ParticleManager（单例，线程安全）

| 成员 | 说明 |
|---|---|
| `Initialize(GraphicsDevice)` | 初始化渲染器与共享资源（幂等） |
| `Play(config, pos, rot, scale)` | 播放效果，返回 `ParticleEffect` 句柄 |
| `Stop(effect)` / `StopAll()` | 停止并回收 |
| `UpdateAll(dt)` / `RenderAll(matrix)` | 手动驱动入口（帧内守卫） |
| `TickAndRender(dt, matrix)` | 更新+渲染一步完成（引擎挂钩使用） |
| `StrategyPath` | 当前更新策略（诊断：GPU 零拷贝 / GPU 更新+读回 / CPU 回退） |
| `CreateStrategy(capacity)` | 策略工厂：GPU 优先，失败自动降级 |

### ParticleEffect（效果实例）

```csharp
slash.Play();  slash.Pause();  slash.Stop();  slash.Reset(seed);
slash.Update(dt);                     // 手动模式（与管理器二选一）
slash.Draw(cameraTransform);          // 手动绘制
slash.Position = pos;                 // 每帧更新发射器变换（如跟随武器）
slash.EmitCustom(inits, emitterIndex); // 代码式发射（世界坐标，绕过形状/速率采样）
```

- **组合模式**：一个效果 = 多个发射器，共享一块槽位池（各发射器分区环形分配）。
- 非循环效果在「发射结束 + 全部粒子死亡」后自动完成并回收（`IsFinished`）。

### 配置 (`ParticleEffectConfig` / `EmitterConfig`)

可序列化（`Particle.Serialization`）、深拷贝（`Clone()`）、观察者通知
（`NotifyChanged()` → `Changed` 事件 → 运行时实时生效；曲线编辑自增 `Version` 驱动 GPU 缓冲增量重建）。

关键参数：发射率与速率曲线、突发事件（`BurstEvent`）、发射形状（点/线段/圆环/**圆弧**）与速度模式
（径向/切向/固定朝向/随机）、初速、生命、颜色/透明度/尺寸随生命曲线、旋转与角速度、色调抖动、重力、阻尼、速度拉伸。

---

## 3. 刀光预设（重点）

### 编辑器内一键播放
预设菜单选择「刀光」：扇形弧线 + 切向高速发射 + 速度拉伸公告牌 + 加法混合 + 内置梭形渐变纹理
（程序生成 128×32，无需外部资源），生命 0.05~0.12 秒，颜色白热 → 橙红快速衰减。

### 游戏内：`BladeTrailAPI`（Battlefield2 已接入）

```csharp
// 惰性创建常驻效果实例（循环模式，不参与自动回收）：
ParticleEffect trail = BladeTrailAPI.CreateEffect();

// 挥砍期间每帧调用（世界坐标）：沿剑刃线段采样发射，切向速度 = 剑尖挥动速度 × 半径比例：
BladeTrailAPI.EmitSwing(trail, grip, tip, tipVelocity, samples: 6, intensity: 1f);

// 角色销毁时：
BladeTrailAPI.DestroyEffect(trail);
```

`CharacterComBrandishCombo`（三连挥砍组件）已在挥砍阶段逐帧调用 `EmitBladeTrail()`：
剑尖速度取帧间位移，渲染端经速度拉伸呈连续发光的弧形光刃。

---

## 4. 配置保存 / 读取

```csharp
ParticleConfigIO.Save(config, "Assets/Particles/我的刀光.json");
ParticleEffectConfig loaded = ParticleConfigIO.Load("Assets/Particles/我的刀光.json");

ParticleEffectConfig copy = ParticleConfigIO.DeepCopy(config);   // 深拷贝
```

JSON 格式（缩进 UTF-8），向量以 `[x, y]` / `[x, y, z, w]` 存储，可直接手改。

---

## 5. 可视化编辑器（MonoGame.ImGUI）

编辑器实现为**场景模块** `ParticleEditorModule : SceneRenderModule`（挂载于
`NavigationScene.SceneInit`, 各场景可自行 `Modules.Add` 复用）—— 界面绘制到模块 RawRt
后随场景**透明合成**，浮于游戏画面之上而不遮挡；按 **F10** 开关。全中文界面：

- **菜单栏（一级）**：文件（新建/打开/保存/另存为）、预设（刀光/爆炸/火焰/烟雾）、编辑（撤销 Ctrl+Z / 重做 Ctrl+Y）
- **左栏**：预设一键加载、发射器组合管理（添加/删除）
- **中栏**：播放/暂停/停止/重置/随机种子/循环预览、实时预览视口（滚轮缩放、中键平移、右键旋转）、策略与实例数诊断
- **右栏（二级页签）**：「发射参数」（发射/形状/速度与生命/外观/物理/渲染 分组）
  「时间轴」（发射率时间轴 + 透明度/尺寸/速度/颜色 曲线）「刀光 Mesh」（拉刀光条带参数 + 整体坐标系）
- 撤销/重做基于命令模式（`EditorCommandHistory`），滑条松开自动提交快照

---

## 6. 更新策略（策略模式 · 纹理粒子）

粒子状态统一存于**数据纹理**（每粒子一列 × 5 行 RGBA32F 纹素, 逐行对应 `Particle` 的 5 个 float4），
渲染端实例流只携带槽位 ID, 顶点着色器按 ID 做纹素取样展开四边形 —— 即老 GpuParticle 的纹理粒子思路。
所有策略输出同一种数据纹理, 渲染路径完全一致, 运行时自动选择（`ParticleManager.CreateStrategy`）：

| 档位 | 路径 | 适用 |
|---|---|---|
| **GPU 零拷贝** | 数据纹理 = D3D11↔D3D12 **共享纹理**（复用 TinterBridge 的 `SharedTexture` 机制）：ComputeSharp 在 D3D12 侧以 UAV 派发生成/集成着色器, MonoGame 在 D3D11 侧直接取样同一块显存 —— 粒子数据全程驻留显存, CPU 不触碰 | 纹理 NT 共享（D3D11 规范覆盖纹理, 本机已验证可用） |
| **GPU 更新+读回** | ComputeSharp 本地纹理更新, 每帧拷回 CPU 并上传 MonoGame 纹理（只传占用前缀列） | 驱动不支持共享 |
| **CPU 回退** | C# 等价积分, 打包为数据纹理上传 | 无 D3D12 / 设备丢失 |

> 说明: D3D11 的 NT 共享契约**只覆盖纹理**（缓冲区共享会 E_INVALIDARG / 触发设备移除）,
> 因此粒子数据载体从缓冲区改为纹理后, 零拷贝在本机 AMD RX 5600 XT 上也可用。

### 曲线系统（控制点曲线）

关键帧即控制点, 默认 **Catmull-Rom 平滑样条**（曲线过所有控制点）, 另有线性/SmoothStep 可选,
`时间轴` 页签顶部统一切换 —— **CPU 与 GPU 求值公式同源**（`CurveMath.CatmullRom` 与
集成着色器 `CatmullRom` 逐系数一致）, 切换插值模式立即全端生效。
时间轴交互: 双击添加控制点 / 拖拽移动（自动钳制在相邻控制点之间, 保持时间递增）/
右键删除（至少保留两端控制点）。

### 同步模型（零拷贝档）

- 帧首 `ComputeFenceSync` 等待上帧派发完成（几乎从不等待）；
- D3D11 渲染端始终读取**上一帧**写入的奇偶纹理, 与本帧派发写入的另一块无任何跨设备冲突；
- 生成记录（64B/条, 含槽位索引）经每帧一次性小缓冲传入, 槽位由 CPU 环形管理 —— GPU 侧无原子回收；
- 共享纹理由 `SharedTexture` 封装（D3D11 创建 `Shared|SharedNTHANDLE` + D3D12 `OpenSharedHandle`
  + ComputeSharp 包装器描述符重写）, 资源状态恒为 COMMON（隐式提升/衰减）, ComputeSharp 对纹理无屏障。

---

## 7. 着色器与 Effect 编译

`Particle.Rendering` 提供双路径（不触碰 Colin.Core 自有资源加载）：

1. **嵌入 MGFX 主路径**：`ParticleRender.fx` 已由 mgfxc 预编译，Base64 嵌入
   `ParticleCompiledShaders.cs`，运行时零依赖直接 `new Effect(device, bytes)`。
2. **进程内编译特性**：`ParticleEffectCompiler.CompileFromSource(hlsl)` 直接调用 MonoGame
   内容管线 `EffectProcessor.Process`（vendor 于 `Colin/Libs/MonoGame.Framework.Content.Pipeline.dll`，
   内部为 ShaderResult + EffectObject 编译核心），**不经 CLI、不经 MGCB**。
   修改 `ParticleShaderSource.RenderEffectSource`（源码同样嵌入）后调用
   `ParticleRenderEffect.Reload(device)` 即可热更新渲染管线。

渲染着色器（`ParticleRender.fx`）为双流实例化：流 0 四边形角点模板 + 流 1 每实例一个
`Particle`（80 字节，布局与 `ParticleLayouts.InstanceVertexDeclaration` 逐字节对应）；
顶点着色器支持普通公告牌（自旋转）与速度拉伸（沿速度方向，长度 = 尺寸×长宽比 + |v|×系数，
封顶 `MaxStretchLength`）；死亡槽位输出退化四边形。混合模式由配置映射到 MonoGame BlendState。

---

## 8. 扩展指南

**自定义预设（工厂注册表）**

```csharp
ParticlePresetFactory.Register("冰霜新星", () => new ParticleEffectConfig { /* ... */ });
var config = ParticlePresetFactory.Create("冰霜新星");
```

**自定义发射器行为**：继承/替换 `ParticleEmitter`（环形槽位、`Advance` 采样、`Emit` 记录构建均可覆写），
或直接用 `ParticleEffect.EmitCustom` 完全由游戏逻辑驱动发射。

**自定义渲染模式**：实现新的渲染器替换 `ParticleRenderer`（管理器持有，构造注入即可）；
着色器新增渲染模式只需扩展 `ParticleRender.fx` 的 Extra 通道并重编嵌入。

**自定义更新策略**：实现 `IParticleUpdateStrategy`（`Initialize/Submit/ResolveFrame`），
通过 `ParticleManager.CreateStrategy` 注入选择逻辑。

**自定义纹理**：`ParticleRenderer.Shared.RegisterTexture(name, texture)`，或配置纹理名填
游戏资产路径（经 `Asset.GetTexture` 解析）；内置 `white/glow/blade/spark/smoke` 程序化生成。

---

## 9. 无窗口冒烟测试（开发自检）

```shell
dotnet run --project DeltaMachine.Windows -- --particle-smoke [out.ppm]
```

验证项：GPU 策略启用与自动降级、60 帧持续发射模拟、离屏渲染像素点亮、
配置 JSON 往返、播放/停止/回收。返回失败计数（进程退出码）。

## 10. 刀光 Mesh (Particle.Slash —— 拉刀光)

刀光本体不是粒子堆叠, 而是**顶点直接摆在轨迹上的条带 Mesh** (Unity 拉刀光 Trail 同思路):

| 类型 | 说明 |
|---|---|
| `RibbonBuilder` | 点列 → 三角带网格: 每点沿切线法线两侧摆顶点, 宽度/颜色/UV 逐点输入 |
| `SlashArc` + `SlashArcConfig` | **参数化弧刀光**: 圆弧条带从起始角横扫到结束角 (缓动手感), 月牙宽度沿弧长分布, 头亮尾隐, 扫完整体渐隐; 可序列化 |
| `SlashTrail` | **轨迹历史刀光** (游戏内剑光): 挥砍时逐帧 `AddPoint(剑尖位置)`, 头宽尾尖, 停手后随 `TrailTime` 流逝消散 |
| `SlashRenderer` | 共享渲染器: `TickAll(dt)` 推进 + `DrawAll(相机矩阵)` 绘制 (加法混合, `DrawUserIndexedPrimitives`); `DrawOne` 供编辑器预览 |
| `SlashRenderEffect` | 嵌入 MGFX + EffectProcessor 进程内编译双路径 (与粒子渲染着色器同机制) |

引擎挂钩 (EcsAdvancedRenderSystem) 已同时驱动粒子与刀光; 游戏内用法:

```csharp
// 挥砍组件中 (Battlefield2 已接入):
_slashTrail ??= ParticlePresetFactory.CreateBladeTrail();
SlashRenderer.GetOrCreate().Register(_slashTrail);
_slashTrail.AddPoint(剑尖世界坐标);   // 每帧

// 招式预设 (编辑器"刀光"预设同时包含 SlashArc 配置 + 辉光/火花粒子):
SlashArc arc = new SlashArc(ParticlePresetFactory.CreateBladeSlashArc()) { Position = pos };
arc.Update(dt);          // 横扫动画
SlashRenderer.GetOrCreate().DrawOne(arc, cameraMatrix);
```

编辑器按 F10 → 顶部页签「刀光 Mesh」可实时调半径/起止角/扫速/宽度/月牙集中度/头尾颜色,
以及**整体坐标系**（横向缩放决定"长"、纵向缩放决定"宽"——X≠Y 即椭圆弧、整体旋转角）,
"▶ 重新挥砍" 即时预览横扫效果。

**形态**：等宽弯曲长方形条带（拉刀光条）—— 宽度沿弧长恒定, 不做两头收尖;
头亮尾隐完全由顶点色承担。**整体坐标系**：横向缩放决定"长"、纵向缩放决定"宽"（X≠Y 即椭圆弧）、
整体旋转角, 均可在编辑器「刀光 Mesh」页签实时调整。

**纹理映射**：条带 UV 为整条映射（u: 0=尾 → 1=头 覆盖全图, v: 横向截面 0→1），
一张图铺满整条刀光而非逐段重复；内置 `blade` 纹理为 512×128 完整光效条
（尾部拖影渐入 → 主体流光带 → 光锋亮白爆发 → 前端收尖, 纵向高斯截面 + 边缘羽化），
换用美术刀光长条图直接改纹理名即可。

## 11. 已知限制

- 跨设备零拷贝需要驱动支持**缓冲区 NT 共享**；本机 AMD RX 5600 XT 驱动拒绝（E_INVALIDARG），
  传统（非 NT）共享句柄会引发设备移除 —— 均已自动降级为「GPU 更新+读回」，功能无损。
- 非循环效果播完自动回收；循环/常驻效果（如剑用刀光）由持有方 `Stop` 回收。
- 修改发射器**容量**或增删发射器会触发槽位池重建（短暂重置模拟）；参数修改全部实时生效。
