using Colin.Core.Modulars.UserInterfaces.Prefabs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.UserInterfaces.Forms
{
  public class DefaultForm : Canvas
  {
    public DefaultForm(string name) : base(name) { }

    /// <summary>
    /// 指示窗体的基底.
    /// </summary>
    public Div Substrate;

    /// <summary>
    /// 指示窗体的边框.
    /// </summary>
    public FormBorder Border;

    public override void DivInit()
    {

      base.DivInit();
    }
  }
}
