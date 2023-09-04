using Colin.Core.ModLoaders;

namespace Colin.Core.Assets
{
    public interface IModResource
    {
        /// <summary>
        /// 从已加载的模组加载资源.
        /// </summary>
        public void LoadModResources( IMod mod );
    }
}