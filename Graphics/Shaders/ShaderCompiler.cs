﻿using SharpDX.D3DCompiler;

namespace Colin.Core.Graphics.Shaders
{
  public class ShaderCompiler
  {
    public enum CompileProfile
    {
      Vertex,
      Pixel,
      Compute
    }
    public static void Compile(string path, CompileProfile profile)
    {
      string profilePar = "";
      switch (profile)
      {
        case CompileProfile.Vertex:
          profilePar = "vs_4_0";
          break;
        case CompileProfile.Pixel:
          profilePar = "ps_4_0";
          break;
        case CompileProfile.Compute:
          profilePar = "cs_5_0";
          break;
      }
      ;
      string resultPath = Path.Combine(Path.ChangeExtension(path, ComputeShader.FileExtension));
      CompilationResult result = ShaderBytecode.CompileFromFile(path, "Main", profilePar, ShaderFlags.Debug);
      try
      {
        if (!result.HasErrors)
        {
          FileStream fs = new FileStream(resultPath, FileMode.Create);
          result.Bytecode.Save(fs);
          fs.Flush();
          fs.Close();
        }
      }
      catch
      {
        if (result.HasErrors)
        {
          Console.WriteLine("Error", result.HasErrors);
        }
      }
    }
  }
}