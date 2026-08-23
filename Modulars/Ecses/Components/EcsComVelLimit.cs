using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses.Components
{
  public class EcsComVelLimit : IEcsCom
  {
    private float _xLimit;
    public float XLimit
    {
      get => _xLimit;
      set
      {
   //     if (value > 0 && (value < _xLimit || _xLimit <= 0))
          _xLimit = value;
      }
    }

    private float _yLimit;
    public float YLimit
    {
      get => _yLimit;
      set
      {
    //    if (value > 0 && (value < _xLimit || _yLimit <= 0))
          _yLimit = value;
      }
    }

    public bool ResetEnable { get; set; }

    public void DoInitialize()
    {

    }
  }
}