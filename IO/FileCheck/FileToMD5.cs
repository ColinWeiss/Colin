using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Colin.Core.IO.FileCheck
{
  /// <summary>
  /// 提供对文件进行 MD5 操作的方法.
  /// </summary>
  public class FileToMD5
  {
    private static MD5 _md5;
    public static MD5 MD5 => _md5 ??= MD5.Create();

    public static void Load()
    {
      List<string> paths = new List<string>();
      bool flag = true; ;
      bool[] results = CheckFiles(paths).ToArray();
      for (int count = 0; count < results.Length; count++)
      {
        flag = results[count];
        if (flag is false)
        {
          //TODO: 抛出, 并予以备选流.
        }
      }
    }

    public static IEnumerable<bool> CheckFiles(List<string> filePaths)
    {
      string path;
      string md5;
      bool result;
      for (int count = 0; count < filePaths.Count; count++)
      {
        path = filePaths[count];
        md5 = GetFileMd5Hash(path);
        result = true;//TODO: 校验
        yield return result;
      }
    }

    public static string GetFileMd5Hash(string filePath)
    {
      using (Task<byte[]> stream = File.ReadAllBytesAsync(filePath))
      {
        MD5 md5 = MD5.Create();
        byte[] hashValue = md5.ComputeHash(stream.Result);
        StringBuilder hex = new StringBuilder(hashValue.Length * 2);
        foreach (byte b in hashValue)
        {
          hex.Append(b);
        }
        return hex.ToString();
      }
    }
  }
}