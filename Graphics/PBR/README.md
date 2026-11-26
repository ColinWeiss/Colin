# PBR 前向渲染管线 (初期)

`Colin.Core.Graphics.PBR` 是引擎级的 glTF 2.0 / GLB 导入、金属度 PBR 前向渲染与 3D 场景接入层。

## 文件

| 文件 | 职责 |
| --- | --- |
| `PbrModelImporter.cs` | Assimp 导入 glTF/GLB: 节点层次、网格、材质、纹理、骨骼、动画 |
| `PbrModel.cs` | 模型聚合: 节点世界变换、动画驱动、蒙皮刷新、绘制 |
| `PbrNode.cs` / `PbrAnimation.cs` | 节点 TRS 与动画通道采样 (线性/四元数球面插值) |
| `PbrMesh.cs` | 顶点/索引缓冲、骨骼调色板纹理 |
| `PbrMaterial.cs` | 双工作流材质 (MetallicRoughness / SpecularGlossiness) |
| `PbrTextureCache.cs` | 嵌入贴图 (`*0`) 与外部相对路径贴图解码 |
| `PbrTextureFactory.cs` | 给天空盒/环境图补完整 mip 链 (按粗糙度做环境模糊) |
| `PbrShader.fx` | HLSL: GGX/Smith/Schlick 直接光照 + 环境贴图 + 全屏天空盒 + 蒙皮 + 发光 |
| `PbrEffectCompiler.cs` | 嵌入源码的进程内 MGFX 编译 (同粒子/刀光路径) |
| `PbrEffect.cs` / `PbrRenderer.cs` | 参数绑定、逐材质状态切换、天空盒、不透明/透明队列绘制 |
| `PbrCamera.cs` / `PbrLighting.cs` | 透视相机、三方向光与环境配置 |
| `PbrRenderModule.cs` | 通用 3D 模块: 带深度缓冲的渲染目标 + 场景合成 |
| `IPbrRenderScene.cs` | 可渲染场景模块的 3D 能力接口 (Renderer/Camera/Lighting/DrawSkybox) |
| `PbrWorldRenderModule.cs` | 游戏世界 3D 层: 透视相机跟随 2D 锚点, 模型挂在锚点附近 |
| `PbrAttachment.cs` | 挂在 `IPbrWorldAnchor` 上的 3D 模型描述 (偏移/深度/缩放/朝向) |
| `PbrModelLoader.cs` | Leemo 资产管线加载器; 默认不参与目录预加载 |

## 关键约定

- **矩阵**: Assimp 是列向量约定 (平移在第四列), XNA 是行向量约定 (平移在第四行); 导入时转置。
  转置后节点世界 = `local * parent`, 蒙皮矩阵 = `绑定逆矩阵 * 骨骼世界`。
- **UV**: Assimp 会把 glTF 的 UV 统一成左下角原点, 因此导入用 `PostProcessSteps.FlipUVs`;
  翻转后 UV 与 Assimp 计算的切线不匹配, 所以切线在本导入器里按最终 UV 重新计算。
- **剔除**: glTF 正面为 CCW; XNA 右手系经 Y 翻转投影后屏幕上表现为 CW, 所以单面材质用
  `CullMode.CullClockwiseFace` 剔除背面。glTF `doubleSided` 从 `$mat.twosided` 原始字节读取
  (AssimpNetter 6.0.5 的 `Material.IsTwoSided` 对 1 字节 Buffer 返回 0, 不能直接用)。
- **法线贴图**: glTF 是 OpenGL 约定 (+Y 朝图像上方), 与 D3D 采样方向相反, 着色器默认翻转绿通道。
- **蒙皮**: 每根骨骼 4 个 texel 的 float4 纹理 (行向量矩阵的 4 行), 顶点着色器按骨骼下标取样,
  不受 128 根骨骼的常量寄存器上限。每顶点最多 4 个影响 (`LimitBoneWeights`)。
- **天空盒**: `PbrRenderModule.DrawSkybox = true` 时先画全屏天空盒 (反投影出方向, 采样 equirect
  环境图); 没设环境图时退回程序化天空。旧 Process3D 的 `Sky.jpg` 是 1280×495 的等距全景。
- **世界 3D 层**: `PbrWorldRenderModule` 用透视相机跟随一个 2D 锚点 (通常是玩家);
  世界坐标换算 `3D = (worldX, -worldY, depth)`, 相机 up = +Y, 所以模型 local +Y 朝屏幕上方。
  模型通过 `Attach(model, offset, depth, scale, yaw, anchor)` 挂在锚点附近, 每帧随锚点移动。
- **透明**: 着色器输出直通 alpha; 渲染器把 `AlphaMode.Blend` 网格按摄像机距离从远到近排队,
  在 `End` 时绘制 (DepthRead + NonPremultiplied)。实测 `Texture2D.FromStream` 不预乘 alpha。
- **背景**: 游戏原本的 `BackGround` 是 `Loop.fx` 的 16 层视差图, 不是 equirect 环境图;
  世界内 3D 层保持透明背景, 不盖掉它。物体上的环境反射用离线烘焙的环境图 (见下)。

## 用法

```csharp
// 资产管线 (Driver.Load 已注册 PbrModelLoader; 模型默认不参与启动期目录预加载)
PbrModel model = Assets.Manager.Load<PbrModel>("Models/Shark.glb");
model.Play("swimming");

// 独立 3D 演示模块
PbrRenderModule pbr = scene.Modules.Register<PbrRenderModule>();
pbr.DrawSkybox = true;
pbr.Add(model);
pbr.Camera.Frame(model.GetWorldSphere());

// 世界 3D 层: 跟随玩家, 模型挂载
PbrWorldRenderModule world3D = scene.Modules.Register<PbrWorldRenderModule>();
world3D.AnchorProvider = () => playerPosition;
world3D.SetEnvironment(bakedEnvironmentTexture); // 可选: 离线烘焙环境图
world3D.Attach(model, new Vector2(-90f, 0f), depth: 120f, scale: 96f / modelHeight);
```

`PbrRenderModule`/`PbrWorldRenderModule` 都实现 `IPbrRenderScene`; 自定义可渲染模块可以派生
`PbrRenderModule` (或自行实现接口 + 持有 `PbrRenderer`) 获得 3D 能力。ECS 以后可原生实现该接口。

## 背景与天空盒

- 游戏世界内**不画**天空盒, 保留现有 2D 分层视差背景; 3D 层透明叠加。
- 分层背景不能直接当 equirect 天空盒: 它是屏幕空间循环图层, 不是 360° 全景;
  强行转成天空盒会丢失视差/循环语义, 且无法随 3D 视角旋转。
- 如果希望 3D 物体反射游戏世界, 按离线烘焙的环境图走:
  `Textures/Environment/WorldBaked.jpg` (没有时退回 `Sky.jpg`)。
- `PbrLighting.EnvironmentRotation` 可以让环境图绕 Y 轴旋转 (天空盒相对物体转);
  环境图在无限远处, 平移物体只改变反射方向, 不会产生视差。

## 演示 / 自检

- 演示场景: 主菜单 `F3`, 或 `--pbr-demo` (可见天空盒、透视绕行相机、动画切换)。
- 世界 3D 预览: 进入任意 Overworld, 玩家出生点附近会挂瓶子和鲨鱼。
- 自检: `--pbr-smoke`; 视觉截图: `--pbr-visual [输出目录]`。

## 已知限制 / 下一步

- 无 IBL (预过滤环境图 + BRDF LUT)、无阴影、无 bloom/后处理色调映射链; 当前 LDR + ACES 近似。
- 世界 3D 层是全屏独立层: 与 2D 地块/实体的**深度遮挡**尚未接入 (3D 物体会盖在地块上,
  但玩家/实体绘制在 3D 层之上)。
- 世界相机是跟随玩家的透视相机, 不对齐 2D 地块像素 (这是透视近大远小的代价)。
- 模型贴图默认不生成 mip; 天空盒/环境图已带 mip。
- 未读取 glTF sampler 的 wrap/filter, 统一为 Linear + Wrap。
- 透明只按网格排队, 没有逐三角形排序; 双面透明材质可能有自重叠混色。
- 模型热重载尚未接线。
- 不支持 morph target、多套 UV、`KHR_materials_unlit`、`KHR_materials_emissive_strength`。
- 非均匀缩放的节点没有用逆转置矩阵修正法线。
- 材质只有 4 个骨骼影响; 超出部分按权重保留最强 4 个。
