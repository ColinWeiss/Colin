using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.UI
{
    public class ResponseEvent
    {
        public Action? Result { get; private set; }

        public ResponseEvent( Action result )
        {
            Result = result;
        }

        /// <summary>
        /// 根据指定 Bool 值决定是否执行绑定的方法.
        /// </summary>
        /// <param name="flags"></param>
        public void Monitor( bool flags )
        {
            if( flags )
                Result.Invoke( );
        }
    }
}