using Assimp;
using Assimp.Configs;
using System;
using System.Collections.Generic;
using System.Text;
using AssimpModel = Assimp.Scene;

namespace Colin.Core.Graphics.Models
{
  public class GlbModel
  {
    private AssimpModel _assimpModel;

    public void OuputForeach()
    {
      Console.WriteLine("输出模型 Node:");
      ForeachOutput(_assimpModel.RootNode, 0);
    }
    private void ForeachOutput(Assimp.Node node, int depth)
    {
      Assimp.Node child;
      string n = "";
      for (int i = 0; i < depth; i++)
      {
        n += "﹂";
      }
      for (int i = 0; i < node.ChildCount; i++)
      {
        child = node.Children[i];
        Console.WriteLine(n + child.Name);
        ForeachOutput(child, depth++);
      }
    }

    public static GlbModel FormFile(string path)
    {
      var importer = new Assimp.AssimpContext();
      importer.SetConfig(new GlobalScaleConfig(1f));
      GlbModel model = new GlbModel();
      model._assimpModel = importer.ImportFile(path);
      return model;
    }
  }
}