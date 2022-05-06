using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin
{
    internal class TestLauncher
    {
        public static void Main()
        {
            using Engine game = new Engine( );
            game.Run( );
        }
    }
}
