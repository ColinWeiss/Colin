public class StrangeTreeGenerator
{
  private Random random;

  public StrangeTreeGenerator()
  {
    random = new Random();
  }

  public StrangeTreeGenerator(int seed)
  {
    random = new Random(seed);
  }

  /// <summary>
  /// 生成紫颂树样式的树
  /// </summary>
  public int[,] GenerateStrangeTree(int maxIterations, int width, int height)
  {
    int[,] map = new int[width, height];

    // 树的起始位置（底部中间）
    int startX = width / 2;
    int startY = height - 2;

    // 先生成主干
    List<Point> mainTrunk = GenerateMainTrunk(startX, startY, maxIterations);

    // 在主干上绘制树干
    foreach (Point point in mainTrunk)
    {
      if (IsInBounds(point.X, point.Y, width, height))
      {
        map[point.X, point.Y] = 1;
      }
    }

    // 为主干底部加厚
    ThickenTrunkBase(map, mainTrunk, width, height);

    // 收集所有分支点（包括主干末端和分支末端）
    List<Point> branchEnds = new List<Point>();

    // 从主干生成分支
    foreach (Point branchPoint in mainTrunk)
    {
      // 随机决定是否在此点生成分支
      if (random.NextDouble() < 0.4 && branchPoint.Y < height - 5)
      {
        var branchPoints = GenerateBranch(map, branchPoint.X, branchPoint.Y, mainTrunk, maxIterations / 2);
        if (branchPoints.Count > 0)
        {
          branchEnds.Add(branchPoints[branchPoints.Count - 1]); // 添加分支末端
        }
      }
    }

    // 添加主干末端
    if (mainTrunk.Count > 0)
    {
      branchEnds.Add(mainTrunk[mainTrunk.Count - 1]);
    }

    // 在分支末端添加树叶
    AddLeaves(map, branchEnds);

    // 添加垂下样式的树叶
    AddHangingLeaves(map, branchEnds);

    return map;
  }

  /// <summary>
  /// 为主干底部加厚
  /// </summary>
  private void ThickenTrunkBase(int[,] map, List<Point> mainTrunk, int width, int height)
  {
    if (mainTrunk.Count == 0) return;

    // 确定要加厚的底部区域（最下面的20%左右的主干）
    int thickenHeight = Math.Max(3, mainTrunk.Count / 5);

    for (int i = 0; i < thickenHeight && i < mainTrunk.Count; i++)
    {
      Point point = mainTrunk[i];
      int thickness = 0;

      // 底部最厚，向上逐渐变细
      if (i == 0) thickness = 2; // 最底部最厚
      else if (i == 1) thickness = 1; // 第二层中等厚度
      else if (i == 2) thickness = 1; // 第三层较薄
      // 更高层保持原样

      // 应用加厚
      for (int dx = -thickness; dx <= thickness; dx++)
      {
        int x = point.X + dx;
        int y = point.Y;

        if (IsInBounds(x, y, width, height) && map[x, y] == 0)
        {
          map[x, y] = 1;
        }
      }
    }
  }

  /// <summary>
  /// 指示主干高度波动值.
  /// </summary>
  public Point TrunkHeightFluc = new Point(5, 12);

  /// <summary>
  /// 指示主干弯曲的概率.
  /// </summary>
  public float TrunkBendProb = 0.2f;

  /// <summary>
  /// 生成更弯曲的主干
  /// </summary>
  private List<Point> GenerateMainTrunk(int startX, int startY, int maxIterations)
  {
    List<Point> trunk = new List<Point>();
    int x = startX;
    int y = startY;

    // 主干高度
    int trunkHeight = maxIterations + random.Next(TrunkHeightFluc.X, TrunkHeightFluc.Y);

    // 弯曲控制变量
    int bendDirection = 0; // 当前弯曲方向：-1=左, 0=直, 1=右
    int bendDuration = 0;  // 当前方向持续次数

    for (int i = 0; i < trunkHeight; i++)
    {
      // 主要向上生长
      y--;
      if (random.NextDouble() < TrunkBendProb || bendDuration <= 0)
      {
        // 改变弯曲方向
        bendDirection = random.Next(-1, 2); // -1, 0, 1
        bendDuration = random.Next(2, 5);   // 持续2-4次
      }
      else
      {
        bendDuration--;
      }

      // 应用弯曲
      if (bendDirection != 0)
      {
        x += bendDirection;
      }

      // 确保不会超出边界（简单处理）
      if (x < 1) x = 1;
      // 宽度边界在后续绘制时检查

      trunk.Add(new Point(x, y));

    }

    return trunk;
  }

  /// <summary>
  /// 生成分支并返回分支点列表
  /// </summary>
  private List<Point> GenerateBranch(int[,] map, int startX, int startY, List<Point> mainTrunk, int branchLength)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);

    int x = startX;
    int y = startY;
    List<Point> branchPoints = new List<Point>();

    // 随机选择分支方向（更多方向选择）
    int direction = random.Next(0, 5); // 0=左上, 1=上, 2=右上, 3=左, 4=右

    // 分支长度随机变化
    branchLength = random.Next(branchLength / 2, branchLength + 2);

    for (int i = 0; i < branchLength; i++)
    {
      // 根据方向移动
      switch (direction)
      {
        case 0: // 左上
          x--;
          y--;
          break;
        case 1: // 上
          y--;
          break;
        case 2: // 右上
          x++;
          y--;
          break;
        case 3: // 左
          x--;
          break;
        case 4: // 右
          x++;
          break;
      }

      // 检查边界
      if (!IsInBounds(x, y, width, height))
        break;

      // 避免与主干重叠
      bool overlapsMainTrunk = false;
      foreach (Point trunkPoint in mainTrunk)
      {
        if (trunkPoint.X == x && trunkPoint.Y == y)
        {
          overlapsMainTrunk = true;
          break;
        }
      }

      if (overlapsMainTrunk)
        continue;

      // 放置分支树干
      map[x, y] = 1;
      branchPoints.Add(new Point(x, y));

      // 更频繁地改变方向（紫颂树特点）
      if (random.NextDouble() < 0.4)
      {
        direction = random.Next(0, 5);
      }

      // 随机终止分支
      if (random.NextDouble() < 0.15)
        break;
    }

    return branchPoints;
  }

  /// <summary>
  /// 在分支末端添加树叶
  /// </summary>
  private void AddLeaves(int[,] map, List<Point> branchEnds)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);

    // 在每个分支末端添加树叶簇
    foreach (Point endPoint in branchEnds)
    {
      // 根据位置调整树叶簇大小（高处树叶更大）
      int clusterSize = 2 + (height - endPoint.Y) / 10;
      clusterSize = Math.Min(clusterSize, 6); // 最大为6

      AddLeafCluster(map, endPoint.X, endPoint.Y, clusterSize);

      // 在分支末端周围也添加一些树叶
      for (int i = 0; i < 2; i++)
      {
        int offsetX = random.Next(-1, 2);
        int offsetY = random.Next(-1, 2);
        AddLeafCluster(map, endPoint.X + offsetX, endPoint.Y + offsetY, clusterSize - 1);
      }
    }
  }

  /// <summary>
  /// 在指定位置周围添加树叶簇
  /// </summary>
  private void AddLeafCluster(int[,] map, int centerX, int centerY, int clusterSize)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);

    // 确保簇大小合理
    clusterSize = Math.Max(1, Math.Min(clusterSize, 6));

    for (int dx = -clusterSize; dx <= clusterSize; dx++)
    {
      for (int dy = -clusterSize; dy <= clusterSize; dy++)
      {
        int x = centerX + dx;
        int y = centerY + dy;

        if (IsInBounds(x, y, width, height) && map[x, y] == 0)
        {
          // 根据距离决定添加树叶的概率（圆形分布）
          double distance = Math.Sqrt(dx * dx + dy * dy);
          if (distance <= clusterSize)
          {
            double probability = Math.Max(0, 1.0 - distance / clusterSize);

            // 边缘概率较低，中心概率较高
            if (random.NextDouble() < probability * 0.8)
            {
              map[x, y] = 2;
            }
          }
        }
      }
    }
  }

  /// <summary>
  /// 添加垂下样式的树叶（类似柳叶）
  /// </summary>
  private void AddHangingLeaves(int[,] map, List<Point> branchEnds)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);

    foreach (Point endPoint in branchEnds)
    {
      // 只有较高的分支才有垂下树叶
      if (endPoint.Y < height && random.NextDouble() < 0.9)
      {
        GenerateHangingLeafStrand(map, endPoint.X, endPoint.Y);
      }
    }
  }

  /// <summary>
  /// 生成单个垂下树叶串
  /// </summary>
  private void GenerateHangingLeafStrand(int[,] map, int startX, int startY)
  {
    int width = map.GetLength(0);
    int height = map.GetLength(1);

    int x = startX;
    int y = startY;

    // 垂下串的长度
    int strandLength = random.Next(5, 12);
    int swingDirection = random.Next(0, 2) == 0 ? -1 : 1; // 左或右摆动

    for (int i = 0; i < strandLength; i++)
    {
      // 主要向下生长
      y++;

      // 检查边界
      if (!IsInBounds(x, y, width, height) || y >= height - 2)
        break;

      // 放置树叶
      if (map[x, y] == 0)
      {
        map[x, y] = 3; // 使用3表示垂下树叶
      }

      // 随机终止垂下串
      if (random.NextDouble() < 0.05)
        break;
    }
  }

  private bool IsInBounds(int x, int y, int width, int height)
  {
    return x >= 0 && x < width && y >= 0 && y < height;
  }

  /// <summary>
  /// 创建纹理（调试用）
  /// </summary>
  public Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int[,] array, int blockSize = 8)
  {
    int width = array.GetLength(0);
    int height = array.GetLength(1);

    Texture2D texture = new Texture2D(graphicsDevice, width * blockSize, height * blockSize);
    Color[] data = new Color[width * blockSize * height * blockSize];

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        Color color = array[x, y] switch
        {
          1 => Color.SaddleBrown,        // 树干
          2 => Color.Green,              // 普通树叶
          3 => Color.LightGreen,         // 垂下树叶
          _ => Color.Transparent         // 空白
        };

        // 填充块区域
        for (int by = 0; by < blockSize; by++)
        {
          for (int bx = 0; bx < blockSize; bx++)
          {
            int dataX = x * blockSize + bx;
            int dataY = y * blockSize + by;
            int index = dataY * (width * blockSize) + dataX;
            data[index] = color;
          }
        }
      }
    }

    texture.SetData(data);
    return texture;
  }
}