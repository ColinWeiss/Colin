using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    /// <summary>
    /// 弱引用事件管理器.
    /// </summary>
    public class WeakEvent
    {
        private WeakReferenceDelegate<Delegate> _delegate;

        /// <summary>
        /// 订阅事件.
        /// </summary>
        public void SetHandler( Delegate handler )
        {
            if(handler != null)
                _delegate = new WeakReferenceDelegate<Delegate>( handler );
        }
        /// <summary>
        /// 引发事件.
        /// </summary>
        public void Raise()
        {
            if(_delegate.Active)
                _delegate.Target.DynamicInvoke();
        }
    }
}