﻿namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块帧格.
  /// </summary>
  public struct TileFrame
  {
    public int X;

    public int Y;

    public int Width;

    public int Height;

    public Point Location => new Point(X, Y);

    public Vector2 LocationF => new Vector2(X, Y);

    /// <summary>
    /// 指示该帧格是否为有效选帧.
    /// <br>[!] 将 <see cref="X"/> 或 <see cref="Y"/> 设为负数即可令帧格失效.</br>
    /// <br>[!] 同理, 若 <see cref="X"/> 和 <see cref="Y"/> 均大于等于 0, 帧格生效.</br>
    /// </summary>
    public bool Effective => X >= 0 && Y >= 0;

    private bool _loadComplete;
    /// <summary>
    /// 表示该边框格是否已执行过 <see cref="LoadStep(BinaryReader)"/>.
    /// </summary>
    public bool LoadComplete => _loadComplete;

    public Rectangle GetRect(ITileContext tileOption)
    {
      return new Rectangle(
            X * tileOption.TileLength,
            Y * tileOption.TileLength,
            Width * tileOption.TileLength,
            Height * tileOption.TileLength);
    }

    public TileFrame(int x, int y, int width = 1, int height = 1)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    internal void LoadStep(BinaryReader reader)
    {
      _loadComplete = true;
      X = reader.ReadInt32();
      Y = reader.ReadInt32();
      Width = reader.ReadInt32();
      Height = reader.ReadInt32();
    }

    internal void SaveStep(BinaryWriter writer)
    {
      writer.Write(X);
      writer.Write(Y);
      writer.Write(Width);
      writer.Write(Height);
    }
  }
}