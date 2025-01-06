using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Collisions
{
  /// <summary>
  /// 碰撞器.
  /// </summary>
  public class Collider
  {
    /// <summary>
    /// 碰撞箱.
    /// </summary>
    public Shape Sensor;

    /// <summary>
    /// 碰撞器的全局唯一标识.
    /// </summary>
    public Guid Guid;

    /// <summary>
    /// 指示所属层级.
    /// </summary>
    public string LayerName;

    /// <summary>
    /// 指示所属层级.
    /// <br>[!] 无需手动设置, 它在被加入 <see cref="Collision"/> 模块时会自动赋值.</br>
    /// <br>[!] 你也可以通过 <see cref="Collision.GetLayer(string)"/> 方法获取它本该有的值.</br>
    /// </summary>
    public byte Layer;

    /// <summary>
    /// 指示与哪些层级发生碰撞.
    /// </summary>
    public List<byte> Mask = new List<byte>();

    public Collider()
    {
      Guid = new Guid();
    }
  }
}