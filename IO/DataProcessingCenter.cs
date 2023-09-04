﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.IO
{
    public class DataProcessingCenter : GameComponent, ISingleton
    {
        public static DataProcessingCenter Instance => Singleton<DataProcessingCenter>.Instance;

        public DataProcessingCenter( ) : base( EngineInfo.Engine ) { }


    }
}