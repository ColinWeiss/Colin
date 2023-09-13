using System.Runtime.InteropServices;

namespace Colin.Core.Events
{
    /// <summary>
    /// 弱引用委托.
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    public class WeakReferenceDelegate<TDelegate> : IEquatable<Delegate>
    {
        private GCHandle _handle;
        public WeakReferenceDelegate( Delegate obj )
        {
            if(obj == null)
                return;
            _handle = GCHandle.Alloc( obj, GCHandleType.Weak );
        }

        /// <summary>
        /// 获取引用的目标是否还没有被GC回收的值.
        /// </summary>
        public bool Active => _handle != default && _handle.Target != null;

        /// <summary>
        /// 获取引用的目标.
        /// </summary>
        public TDelegate Target
        {
            get
            {
                if(_handle == default)
                    return default;
                return (TDelegate)_handle.Target;
            }
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否与指定的对象相等.
        /// </summary>
        /// <param name="other">与此实例进行比较的对象, 或为 <see langword="null"/>.</param>
        /// <returns></returns>
        public bool Equals( Delegate other )
        {
            return _handle != default && other != null &&
                ((Delegate)_handle.Target).Method.Equals( other.Method );
        }

        /// <summary>
        /// 释放弱引用.
        /// </summary>
        ~WeakReferenceDelegate()
        {
            _handle.Free();
        }
    }
}