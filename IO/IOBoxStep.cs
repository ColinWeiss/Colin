using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.IO
{
  public interface IOBoxStep
  {
    public List<IOBoxStep> IOSteps { get; }
    void DoLoad(StoreBox box)
    {
      LoadStep(box);
      for (int i = 0; i< IOSteps.Count; i++)
      {
        IOSteps[i].DoLoad(box); 
      }
    }
    void DoSave(StoreBox box)
    {
      SaveStep(box);
      for (int i = 0; i < IOSteps.Count; i++)
      {
        IOSteps[i].DoSave(box);
      }
    }
    void LoadStep(StoreBox box);
    void SaveStep(StoreBox box);
  }
}
