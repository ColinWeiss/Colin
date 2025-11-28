# 简介    
本项目基于MonoGame进行开发; 旨在提供多样化的套件以提升开发效率.  

## 使用方式
1.本项目抛弃了MGCB-Editor, 请从本库下载源代码后以源码形式, 利用共享项目配置将其整合至你自己的项目.  
2.在启动项目中的"生成后事件"命令行中添加如下指令:xcopy "$(SolutionDir)你的项目名称\Assets $(TargetDir)Assets /s/i/e/y"    
3.再于指定路径下创建Asseta文件夹, 将资产文件移至其中.  

## 套件 
前文提到本项目提供了多样化的套件:    
· 瓦片 Tile    
· 场景管理 Scene & SceneManager & SceneModule   
· 基于组合模式的实体组件系统 ECS (Entity-Component-System)   
· 设备输入处理 Mouse & Keyboard (手柄正在路上)  
· 用户交互界面库 UserInterface  

## Tile
请优先查看 `TileInfo.cs`、`TileKernel.cs`、`TileChunk.cs`类。    
TileInfo是物块的最小组成单位, TileKernel则通过享元模式提供了可定制化的物块行为，而TileChunk代表一个区块。    

## Scene
直接查看`Scene.cs`，它被作为`SceneModule.cs`的集合以利用模块化制作一个场景。

## ECS
直接查看`Ecs.cs`。

## 设备输入
查看`MouseResponder.cs`与`KeyboardResponder.cs`。

## UI
查看`Div.cs`。

## 示例
将对应套件的场景模块加入场景即可自由构建具有对应功能的场景.
示例场景正在编写中, 请等待更新.  

-编辑于 2025/11/28
