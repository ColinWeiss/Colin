namespace Colin.Core.Noises.Berlin
{
    /// <summary>
    /// 柏林噪声生成器.
    /// </summary>
    public class BerlinNoiseCreator
    {
        /// <summary>
        /// 噪声宽度.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// 噪声高度.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 指示点集合.
        /// </summary>
        public byte[] Points { get; private set; }

        /// <summary>
        /// 该生成器使用的随机数生成器.
        /// </summary>
        public Random Random { get; private set; }

        /// <summary>
        /// 该生成器的配置信息.
        /// </summary>
        public BerlinNoiseInfo Info;

        /// <summary>
        /// 指示生成器使用的种子.
        /// </summary>
        public int Seed;

        private static Texture2D Pixel;

        public BerlinNoiseCreator(Point size)
        {
            Width = size.X;
            Height = size.Y;
            Info = new BerlinNoiseInfo();
            Seed = new Random().Next();
            Init();
        }

        public BerlinNoiseCreator(Point size, BerlinNoiseInfo info)
        {
            Width = size.X;
            Height = size.Y;
            Info = info;
            Seed = new Random().Next();
            Init();
        }

        public BerlinNoiseCreator(Point size, BerlinNoiseInfo info, int seed)
        {
            Width = size.X;
            Height = size.Y;
            Info = info;
            Seed = seed;
            Init();
        }

        public BerlinNoiseCreator(int width, int height)
        {
            Width = width;
            Height = height;
            Info = new BerlinNoiseInfo();
            Seed = new Random().Next();
            Init();
        }

        public BerlinNoiseCreator(int width, int height, BerlinNoiseInfo info)
        {
            Width = width;
            Height = height;
            Info = info;
            Seed = new Random().Next();
            Init();
        }

        public BerlinNoiseCreator(int width, int height, BerlinNoiseInfo info, int seed)
        {
            Width = width;
            Height = height;
            Info = info;
            Seed = seed;
            Init();
        }

        private int[] _map1D;

        private Vector2[] _map2D;

        private void Init()
        {
            if (Pixel == null)
            {
                Pixel = new Texture2D(EngineInfo.Graphics.GraphicsDevice, 1, 1);
                Pixel.SetData(new Color[] { Color.White });
            }
            if (Info.Single)
            {
                Random = new Random(Seed);
                Points = new byte[Width * Height];
                Span<byte> points = Points;
                points.Fill(0);
                _map1D = new int[Width * Height];
                for (int count = 0; count < _map1D.Length; count++)
                    _map1D[count] = Random.Next(Info.Amplitude);
            }
            else
            {
                Random = new Random(Seed);
                Points = new byte[Width * Height];
                Span<byte> points = Points;
                points.Fill(0);
                _map2D = new Vector2[Width * Height];
                for (int count = 0; count < _map2D.Length; count++)
                {
                    bool xf = Random.Next(2) == 1;
                    bool yf = Random.Next(2) == 1;
                    _map2D[count] = new Vector2(Random.NextSingle() * (xf ? 1 : -1), Random.NextSingle() * (yf ? 1 : -1));
                }
            }
        }

        private void SetPoint(int x, int y, byte value)
        {
            for (int y0 = y; y0 < Height; y0++)
            {
                Points[x + y0 * Width] = value;
            }
        }

        private float Perlin(float x)
        {
            int x1 = (int)x;
            int x2 = x1 + 1;
            float grad1 = _map1D[x1];
            float grad2 = _map1D[x2];
            float vec1 = x - x1;
            float vec2 = x - x2;
            float t = 3 * MathF.Pow(vec1, 2) - 2 * MathF.Pow(vec1, 3);
            float product1 = grad1 * vec1;
            float product2 = grad2 * vec2;
            return product1 + t * (product2 - product1);
        }

        private float Perlin(float x, float y)
        {
            int p0x = (int)x; // P0坐标
            int p0y = (int)y;
            int p1x = p0x;      // P1坐标
            int p1y = p0y + 1;
            int p2x = p0x + 1;  // P2坐标
            int p2y = p0y + 1;
            int p3x = p0x + 1;  // P3坐标
            int p3y = p0y;
            float g0x = Grad(p0x, p0y)[0];  // P0点的梯度
            float g0y = Grad(p0x, p0y)[1];
            float g1x = Grad(p1x, p1y)[0];  // P1点的梯度
            float g1y = Grad(p1x, p1y)[1];
            float g2x = Grad(p2x, p2y)[0];  // P2点的梯度
            float g2y = Grad(p2x, p2y)[1];
            float g3x = Grad(p3x, p3y)[0];  // P3点的梯度
            float g3y = Grad(p3x, p3y)[1];
            float v0x = x - p0x;  // P0点的方向向量
            float v0y = y - p0y;
            float v1x = x - p1x;  // P1点的方向向量
            float v1y = y - p1y;
            float v2x = x - p2x;  // P2点的方向向量
            float v2y = y - p2y;
            float v3x = x - p3x;  // P3点的方向向量
            float v3y = y - p3y;
            float product0 = g0x * v0x + g0y * v0y;  // P0点梯度向量和方向向量的点乘
            float product1 = g1x * v1x + g1y * v1y;  // P1点梯度向量和方向向量的点乘
            float product2 = g2x * v2x + g2y * v2y;  // P2点梯度向量和方向向量的点乘
            float product3 = g3x * v3x + g3y * v3y;  // P3点梯度向量和方向向量的点乘
            float d0 = x - p0x;
            float t0 = 6f * MathF.Pow(d0, 5f) - 15f * MathF.Pow(d0, 4f) + 10f * MathF.Pow(d0, 3f);
            float n0 = product1 * (1f - t0) + product2 * t0;

            // P0和P3的插值
            float n1 = product0 * (1f - t0) + product3 * t0;

            // P点的插值
            float d1 = y - p0y;
            float t1 = 6f * MathF.Pow(d1, 5f) - 15f * MathF.Pow(d1, 4f) + 10f * MathF.Pow(d1, 3f);
            return n1 * (1f - t1) + n0 * t1;
        }

        private float[] Grad(float x, float y)
        {
            float[] vec = new float[2];
            vec[0] = _map2D[(int)(x + y * Width)].X;
            vec[1] = _map2D[(int)(x + y * Width)].Y;
            return vec;
        }

        public void Create()
        {
            if (Info.Single)
            {
                for (int x = 0; x < Width; x++)
                {
                    float x0 = (float)x / Width * Info.Scale;
                    SetPoint(x, (int)(Height / 2 + Perlin(x0)), 1);
                }
            }
            else
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        float x0 = (float)x / Width * Info.Scale;
                        float y0 = (float)y / Height * Info.Scale;
                        float color = 122.5f + 255 * Perlin(x0, y0);
                        if (color > Info.Limit)
                            color = Info.Limit;
                        if (color < 0)
                            color = 0;
                        SetPoint(x, y, (byte)color);
                    }
                }
            }
        }

        public void Render(SpriteBatch spriteBatch, Vector2 position)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Points[x + y * Width] > 0)
                    {
                        spriteBatch.Draw(Pixel, position + new Vector2(x, y),
                            new Color(Color.White, Info.Single ? 255 : Points[x + y * Width]));
                    }
                }
            }
        }

    }
}