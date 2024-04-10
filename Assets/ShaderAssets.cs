﻿using Colin.Core.Graphics.Shaders;

namespace Colin.Core.Assets
{
  public class ShaderAssets : IGameAsset
  {
    public string Name => "缓存计算着色器";

    public float Progress { get; set; }

    public static Dictionary<string, ComputeShader> Shaders { get; set; } = new Dictionary<string, ComputeShader>();

    public void CompileShaders()
    {
      string _fileName;
      string[] _csoFileNames =
          Directory.GetFiles(
              Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EngineInfo.Engine.Content.RootDirectory, "Shaders"), "*.hlsl*",
              SearchOption.AllDirectories);
      for (int count = 0; count < _csoFileNames.Length; count++)
      {
        Progress = count / _csoFileNames.Length + 1 / _csoFileNames.Length;
        _fileName = _csoFileNames[count];
        ShaderCompiler.Compile(_fileName, ShaderCompiler.CompileProfile.Compute);
      }
    }
    public void LoadResource()
    {
#if DEBUG
      CompileShaders();
#endif
      ComputeShader _effect;
      string _fileName;
      string[] _csoFileNames =
          Directory.GetFiles(
              Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EngineInfo.Engine.Content.RootDirectory, "Shaders"), "*.hlsl*",
              SearchOption.AllDirectories);

      for (int count = 0; count < _csoFileNames.Length; count++)
      {
        Progress = count / _csoFileNames.Length + 1 / _csoFileNames.Length;
        _fileName = Path.ChangeExtension(_csoFileNames[count], ".cso");
        if (File.Exists(_fileName) is false)
          CompileShaders();
        _effect = new ComputeShader(EngineInfo.Graphics.GraphicsDevice, File.ReadAllBytes(_fileName));
        Shaders.Add(Path.ChangeExtension(IGameAsset.ArrangementPath(_fileName), null), _effect);
      }
    }
    public static ComputeShader Get(string path)
    {
      ComputeShader shader;
      if (Shaders.TryGetValue(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EngineInfo.Engine.Content.RootDirectory, "Shaders", path), out shader))
        return shader;
      else
        return null;
    }
  }
}