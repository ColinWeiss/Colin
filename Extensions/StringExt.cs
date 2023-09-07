using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Colin.Core.Extensions
{
    public static class StringExt
    {
        public static int GetMsnHashCode( this string str )
        {
            int result = 0;
            int len = str.Length;
            for(int i = 0 ; i < len ; i++)
                result += ( result + str[i] * 3737 %1000000007)% 1000000007;
            return result;
        }
    }
}