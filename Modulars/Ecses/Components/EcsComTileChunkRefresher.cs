using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaMachine.Core.GameContents.Ecses.Components
{
    /// <summary>
    /// 用于令实体向物块区块发送一系列关于加载/卸载的操作.
    /// </summary>
    public class EcsComTileChunkRefresher : EcsComScript
    {
        /// <summary>
        /// 指示该实体在进入未加载区块时是否直接消失.
        /// <br>[!] 若该值为 <see langword="true"/>, 则不允许实体进入未加载区块.</br>
        /// </summary>
        public bool Unimportant = true;

        /// <summary>
        /// 指示该实体是否需要跟随区块卸载一同保存.
        /// </summary>
        public bool NeedSave = false;

        public override void DoUpdate()
        {

            base.DoUpdate();
        }
    }
}