using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using AssimpModel = Assimp.Scene;

namespace Colin.Core.Graphics.Models
{
  public class GlbModel
  {
    private AssimpModel _model;

    public void OuputForeach()
    {
      ForeachOutput(_model.RootNode, 0);
      MaterialProperty[] mats;
      Console.WriteLine(_model.Textures.Count);
      Texture2D t;
      using (var stream = new MemoryStream(_model.Textures[0].CompressedData))
      {
        t = Texture2D.FromStream(CoreInfo.Graphics.GraphicsDevice, stream);
      }
      using (FileStream fs = new FileStream("test.png", FileMode.Create))
      {
        t.SaveAsPng(fs, t.Width, t.Height);
      }

      for (int i = 0; i < _model.Materials.Count; i++)
      {
        mats = _model.Materials[i].GetAllProperties();
        for (int j = 0; j < mats.Length; j++)
        {
          Console.WriteLine(mats[j].FullyQualifiedName + " " + mats[j].RawData.Length);
          if (mats[j].Name == "$mat.gltf.pbrMetallicRoughness.baseColorFactor")
          {
          }
        }
      }
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
      model._model = importer.ImportFile(path,
        PostProcessSteps.Triangulate |
        PostProcessSteps.GenerateNormals |
        PostProcessSteps.FlipUVs |
        PostProcessSteps.MakeLeftHanded);
      return model;
    }
  }
}