本项目基于MonoGame进行开发; 旨在提供多样化的针对MonoGame的套件以提升开发效率.  

因本项目抛弃了MGCB-Editor, 所以若需要使用本项目, 请从本库下载源代码后以源码形式, 利用共享项目配置将其整合至你自己的项目.  
并在启动项目中的"生成后事件"命令行中添加如下指令  
xcopy $(SolutionDir)你的项目名称\Assets $(TargetDir)Assets /s/i/e/y  
再于指定路径下创建Asseta文件夹, 将资产文件移至其中.  

前文提到本项目提供了多样化的套件:
· 瓦片地图 Tilemap  
· 场景管理 Scene & SceneManager & SceneModule   
· 基于组合模式的实体组件系统 ECS (Entity-Component-System)   
· 设备输入处理 Mouse & Keyboard (手柄正在路上)  
· 用户交互界面库 UserInterface  
· 本地化 Localization  

在对应命名空间下即可查看对应模块.  
将对应套件的场景模块加入场景即可自由构建具有对应功能的场景.

示例场景正在编写中, 请等待更新.  

-编辑于 2024/11/20
