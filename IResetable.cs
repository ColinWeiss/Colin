﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core
{
    /// <summary>
    /// 表示允许进行重置操作的对象.
    /// </summary>
    public interface IResetable
    {
        void Reset();
    }
}