namespace Colin.Core.Noises.Berlin
{
    /// <summary>
    /// <see cref="BerlinNoiseCreator"/> 的配置信息.
    /// </summary>
    public struct BerlinNoiseInfo
    {
        /// <summary>
        /// 指示是否单维生成.
        /// </summary>
        public bool Single = true;

        /// <summary>
        /// 缩放.
        /// <br>该设置在单维生成下指横轴缩放.</br>
        /// <br>在二维生成下指视野缩放.</br>
        /// </summary>
        public float Scale = 1.0f;

        /// <summary>
        /// 振幅.
        /// <br>[!] 该设置仅在单维生成下生效. </br>
        /// </summary>
        public int Amplitude = 32;

        /// <summary>
        /// 限制.
        /// <br>[!] 该设置仅在二维生成下生效.</br>
        /// </summary>
        public int Limit = 255;

        public BerlinNoiseInfo() { }

    }
}
