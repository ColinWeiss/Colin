using System.Runtime.CompilerServices;

namespace Colin.Core.Common
{
    public class Component
    {

        public virtual void LoadDatas( ) { }

        public virtual void SaveDatas( ) { }

        /// <summary>
        /// 发生在组件启用时.
        /// </summary>
        public virtual void OnEnabled( ) { }

        /// <summary>
        /// 发生在组件禁用时.
        /// </summary>
        public virtual void OnDisabled( ) { }

        public Component SetEnabled( bool isEnabled )
        {

            return this;
        }

    }
}