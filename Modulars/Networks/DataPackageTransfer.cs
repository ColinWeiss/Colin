namespace Colin.Core.Modulars.Networks
{
    /// <summary>
    /// 数据包中转站 !
    /// </summary>
    public class DataPackageTransfer : INetworkMode
    {
        /// <summary>
        /// 接收到的数据包缓存.
        /// </summary>
        public Dictionary<string, DataPackage> Receives { get; private set; } = new Dictionary<string, DataPackage>();

        /// <summary>
        /// 要发送的数据包.
        /// </summary>
        public Dictionary<string, DataPackage> Pending { get; private set; } = new Dictionary<string, DataPackage>();

        public void ReceiveDatas(BinaryReader reader, NetModeState state)
        {
            DataPackage _package = new DataPackage();
            int packageCount = reader.ReadInt32(); //接收数据包个数.
            for (int count = 0; count < packageCount; count++)
            {
                _package.Identifier = reader.ReadString();
                int packageSize = reader.ReadInt32();
                _package.Datas = reader.ReadBytes(packageSize);
                if (Receives.ContainsKey(_package.Identifier))
                {
                    Receives.Remove(_package.Identifier);
                    Receives.Add(_package.Identifier, _package);
                }
                else
                    Receives.Add(_package.Identifier, _package);
            }
        }

        /// <summary>
        /// 按顺序发送 <see cref="Pending"/> 中的所有数据包.
        /// </summary>
        /// <param name="writer">写入流.</param>
        /// <param name="state">
        /// 模块状态.
        /// <br>[!] 该对象的该方法仅接受 <see cref="NetModeState.Conduct"/> 与 <see cref="NetModeState.Over"/> 参数.</br>
        /// </param>
        public void SendDatas(BinaryWriter writer, NetModeState state)
        {
            DataPackage _send = new DataPackage();
            if (Pending.Count > 0)
            {
                int packageCount = Pending.Count;
                writer.Write(packageCount);
                for (int count = 0; count < packageCount; count++)
                {
                    _send = Pending.ElementAt(count).Value;
                    writer.Write(_send.Identifier);
                    writer.Write(_send.Datas.Length);
                    writer.Write(_send.Datas);
                }
            }
            if (state == NetModeState.Over)
                Pending.Clear();
        }

        /// <summary>
        /// 朝待处理区添加新数据包.
        /// <br>[!] 若 <see cref="Pending"/> 中包含同版本的数据包, 则选择最新.</br>
        /// </summary>
        /// <param name="dataPackage">数据包.</param>
        public void AddDataPackageToPending(DataPackage dataPackage)
        {
            if (Pending.ContainsKey(dataPackage.Identifier))
            {
                Pending.Remove(dataPackage.Identifier);
                Pending.Add(dataPackage.Identifier, dataPackage);
            }
            else
                Pending.Add(dataPackage.Identifier, dataPackage);
        }

        /// <summary>
        /// 弹出 <see cref="Receives"/> 中的第一个数据包.
        /// <br>[!] 在弹出后数据包将会从 <see cref="Receives"/> 中删除.</br>
        /// </summary>
        /// <returns>弹出的数据包.</returns>
        public DataPackage EjectDataPackage()
        {
            DataPackage _result = Receives.First().Value;
            Receives.Remove(_result.Identifier, out _result);
            return _result;
        }

        /// <summary>
        /// 弹出 <see cref="Receives"/> 中的第一个数据包.
        /// </summary>
        /// <param name="result"></param>
        /// <returns>若成功找到并弹出, 返回 true, 否则返回 false.</returns>
        public bool EjectDataPackage(out DataPackage result) => Receives.Remove(Receives.First().Key, out result);

    }
}