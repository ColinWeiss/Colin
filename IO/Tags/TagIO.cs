using System.IO.Compression;

namespace Colin.Core.IO
{
  /// <summary>
  /// 键值组的文件级门面: 一个路径进, 一棵键值组出, 反之亦然.
  /// <br>读的时候看文件头两个字节认 gzip 魔数, 压没压缩都吃得下; 写的时候想压就开个开关.</br>
  /// </summary>
  public static class TagIO
  {
    /// <summary>把键值组落成档; <paramref name="compress"/> 开了就 gzip, 物块这类高冗余数据能小一大圈.</summary>
    public static void Write(string path, TagCompound root, bool compress = false)
    {
      using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024);
      Write(stream, root, compress);
    }
    public static void Write(Stream stream, TagCompound root, bool compress = false)
    {
      if (compress)
      {
        using GZipStream gzip = new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true);
        new TagWriter(gzip).WriteRoot(root);
      }
      else
      {
        new TagWriter(stream).WriteRoot(root);
      }
    }
    /// <summary>从档里整棵读出键值组; 文件不存在或者档坏了会直接抛, 调用方自己决定兜不兜底.</summary>
    public static TagCompound Read(string path)
    {
      using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
      return Read(stream);
    }
    /// <summary>流要可定位: 前面偷看魔数那两字节要退回去.</summary>
    public static TagCompound Read(Stream stream)
    {
      int first = stream.ReadByte();
      int second = stream.ReadByte();
      stream.Position = 0;
      if (first == 0x1F && second == 0x8B)
      {
        using GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress);
        return new TagReader(gzip).ReadRoot();
      }
      return new TagReader(stream).ReadRoot();
    }
  }
}
