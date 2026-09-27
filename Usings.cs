global using Colin.Core;
global using Colin.Core.Common;
global using Colin.Core.Extensions;
global using Colin.Core.Graphics;
global using Colin.Core.Graphics.Visual.Particle;
global using Colin.Core.Graphics.Visual.Particle.Rendering;
global using Colin.Core.Graphics.Visual.Serialization;
global using Colin.Core.Graphics.Visual.Slash;
// Colin.Editor 是可选手柄 (仅宿主游戏导入), 引擎单独编译 (如 Colin.NUnit) 时不存在,
// 这行全局 using 由宿主工程定义的 COLIN_EDITOR 门控.
#if COLIN_EDITOR
global using Colin.Editor.Particle;
#endif
global using Colin.Core.Inputs;
global using Colin.Core.Preparation;
global using Leemo.Assets;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using MonoGame.IMEHelper;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using Color = Microsoft.Xna.Framework.Color;
global using Console = Colin.Core.Console;
global using Keys = Microsoft.Xna.Framework.Input.Keys;
global using Rectangle = Microsoft.Xna.Framework.Rectangle;
global using RectangleF = Colin.Core.RectangleF;
global using Point = Microsoft.Xna.Framework.Point;
global using Vector2 = Microsoft.Xna.Framework.Vector2;
global using Vector3 = Microsoft.Xna.Framework.Vector3;
global using Vector4 = Microsoft.Xna.Framework.Vector4;
global using Quaternion = Microsoft.Xna.Framework.Quaternion;