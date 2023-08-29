using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Events
{
    /// <summary>
    /// 弱引用事件管理器.
    /// </summary>
    public class WeakEventManager
    {
        private readonly List<WeakReferenceDelegate<Delegate>> _delegateList;
        public WeakEventManager( )
        {
            _delegateList = new List<WeakReferenceDelegate<Delegate>>( );
        }

        /// <summary>
        /// 订阅事件.
        /// </summary>
        public void AddHandler( Delegate handler )
        {
            if( handler != null )
                _delegateList.Add( new WeakReferenceDelegate<Delegate>( handler ) );
        }

        /// <summary>
        /// 取消订阅.
        /// </summary>
        public void RemoveHandler( Delegate handler )
        {
            if( handler == null ) return;
            //由于我实现了IEquatable<Delegate>，这里能够很方便的比较
            var sameHandler = _delegateList.FirstOrDefault( e => e.Equals( handler ) );
            if( sameHandler != null )
                _delegateList.Remove( sameHandler );
        }

        /// <summary>
        /// 引发事件.
        /// </summary>
        public void Raise( object sender, EventArgs e )
        {
            foreach( var d in _delegateList.ToList( ) )
            {
                if( d.Active )
                    d.Target.DynamicInvoke( sender, e );
                else
                    _delegateList.Remove( d );
            }
        }

    }
}