# PBR 前向渲染管线 (初期)

`Colin.Core.Graphics.PBR` 是引擎级的 glTF 2.0 / GLB 导入、金属度 PBR 前向渲染与 3D 场景接入层。
世界内的 3D 渲染已经并入 `Ecs` (实体的 3D 组件)，不再依赖独立的 3D 场景模块。

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
| `PbrShader.fx` | HLSL: GGX/Smith/Schlick 直接光照 + 环境贴图 + 全屏天空盒 + 蒙皮 + 发光 + 游戏光照模式 |
| `PbrEffectCompiler.cs` | 嵌入源码的进程内 MGFX 编译 (同粒子/刀光路径) |
| `PbrEffect.cs` / `PbrRenderer.cs` | 参数绑定、逐材质状态切换、天空盒、不透明/透明队列绘制 |
| `PbrCamera.cs` / `PbrLighting.cs` | 透视相机 (`SyncToSceneCamera` 对齐 2D 相机)、三方向光与环境配置 |
| `PbrRenderModule.cs` | 独立 3D 演示模块 (PBR 演示场景使用) |
| `IPbrRenderScene.cs` | 可渲染场景模块的 3D 能力接口 (Renderer/Camera/Lighting/DrawSkybox) |
| `PbrModelLoader.cs` | Leemo 资产管线加载器; 默认不参与目录预加载 |
| `EcsComPbrRender.cs` | ECS 组件: 把 PbrModel 挂在实体 Transform 上 |

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
- **天空盒**: `PbrRenderModule.DrawSkybox = true` 时先画全屏天空盒; 只用于独立 PBR 演示。
  游戏世界内**不画**天空盒, 只保留 2D 分层背景。
- **世界 3D**: `Ecs` 原生实现 `IPbrRenderScene`，3D 实体由 `EcsComPbrRender` 描述，
  由 `LightingAdapter` 在 2D 合成前后分别调用 `Ecs.RenderPbrLayer`。
  3D 世界坐标 `(worldX, -worldY, depth)`，相机正面 +Z；实体 Transform 固定在世界里,
  相机移动不会带走模型。
- **Z=0 分层**: 普通 2D 层在 `Z = 0`。`EcsComPbrRender.Depth > 0` 的 3D 画在所有 2D 之后，
  可以遮挡玩家/物块；`Depth < 0` 的 3D 画在所有 2D 之前，被 2D 遮挡。
  当前瓶子 `Depth = 120` (前景)，鲨鱼 `Depth = -120` (背景)。
- **2px 像素滤镜**: ECS 3D 渲染到 1/2 分辨率带深度 RT，合成时用 `PointClamp` 放大 2 倍，
  形成 2px 像素块；2D 保持原分辨率。`Ecs.PbrPixelSize` 固定为 2。
- **相机**: `PbrCamera.SyncToSceneCamera` 跟随 2D `Scene.Camera` 的 Position / Zoom / Rotation。
  半分辨率渲染时同步传入 `zoomScale = 1/2`，放大后锚点处仍是 1 世界像素 ≈ `Zoom.Y` 屏幕像素；
  模型离开地面 (z≠0) 时得到近大远小。
- **光照**: ECS 世界内不再使用 3D 方向光。`PbrRenderer.GameLighting = true` 时 PBR 只输出
  材质底色 + 环境反射，随后由 Ecs 原本的 `LightingAdapter`/Tile 光照后处理统一上光。
  独立 PBR 演示仍使用 3D 直接光 + ACES。
- **环境反射**: 离线烘焙环境图只用于材质反射，不是可见天空盒；优先读
  `Textures/Backgrounds/<背景名>/Environment.jpg` (没有则 `.png`)，缺失时退回旧 `Sky.jpg`。
  `PbrLighting.EnvironmentRotation` 可让环境图绕 Y 轴旋转。
- **透明**: 着色器输出直通 alpha; 渲染器把 `AlphaMode.Blend` 网格按深度排队 (DepthRead +
  NonPremultiplied)。实测 `Texture2D.FromStream` 不预乘 alpha。

## 用法

```csharp
// 资产管线 (Driver.Load 已注册 PbrModelLoader; 模型默认不参与启动期目录预加载)
PbrModel model = Assets.Manager.Load<PbrModel>("Models/WaterBottle.glb");

// 世界内 3D: 创建一个 ECS 实体并挂组件
Entity entity = ecs.MarkCreate<Entity>();
entity.Transform.Translation = spawn + new Vector2(-90f, 0f);
EcsComPbrRender render = entity.RegisterCom<EcsComPbrRender>();
render.Model = model;
render.Depth = 120f;
render.Scale = 96f / (model.Bounds.Max.Y - model.Bounds.Min.Y);

// 环境图 (可选)
ecs.SetEnvironment(Assets.Manager.Load<Texture2D>("Textures/Backgrounds/Origin/Environment.jpg"));

// 独立 PBR 演示模块 (带可见天空盒)
PbrRenderModule pbr = scene.Modules.Register<PbrRenderModule>();
pbr.DrawSkybox = true;
pbr.Add(model);
pbr.Camera.Frame(model.GetWorldSphere());
```

`Ecs`/`PbrRenderModule` 都实现 `IPbrRenderScene`; ECS 以后可以继续扩展实体组件，
`EcsComPbrRender` 就是把瓶子/鲨鱼换成正式 Entity 的接口。

## 背景与天空盒

- `BackGround` 是 `Loop.fx` 的多层视差图 (Origin 13 层 / Forest 16 层 + Sky/Cloud)，不是 equirect；
  不能直接当天空盒，否则会失去视差和循环语义。
- 游戏世界内保留 2D 分层背景，3D 模型叠在 Ecs 层里；3D 不画天空盒。
- 离线烘焙环境图放在对应背景目录 (`Textures/Backgrounds/Origin/Environment.jpg`)，
  仅供 3D 材质反射。

## 演示 / 自检

- 独立 PBR 演示: 主菜单 `F3` 或 `--pbr-demo` (可见天空盒、透视绕行相机、动画切换)。
- 世界 3D 预览: 进入任意 Overworld，出生点旁有瓶子和鲨鱼实体，固定在世界坐标。
- 自检: `--pbr-smoke`; 视觉截图: `--pbr-visual [输出目录]`。

## 已知限制 / 下一步

- ECS 世界内为了接游戏 2D 光照，3D 只输出底色 + 环境反射，**不做 3D 方向光/阴影**。
- 3D 与 2D 的遮挡按**实体**的 Z 分层，不是逐三角形/逐像素深度；模型本身跨 Z=0 时仍由
  `EcsComPbrRender.Depth` 的符号决定整层。
- 3D 层在 VFX/UI 之下；2D 的 VFX/UI 不会按 Z 和 3D 交错。
- 相机跟随 2D 相机的 Position/Zoom/Rotation，但 2D 相机的 `Translate` 偏移未接入 (通常等于视口中心)。
- 模型贴图默认不生成 mip；环境图已带 mip。
- 透明只按网格排队；双面透明材质有自重叠混色。
- 模型热重载尚未接线。
- 不支持 morph target、多套 UV、`KHR_materials_unlit`、`KHR_materials_emissive_strength`。
- 非均匀缩放的节点没有用逆转置矩阵修正法线；每顶点最多 4 骨骼影响。
