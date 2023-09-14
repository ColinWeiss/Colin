using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Graphics
{
    public interface IDrawBatcher<T> where T: unmanaged, IVertexType
    {
        public void DrawQuad(Texture2D texture, T vul, T vur, T vdr, T vdl, int sortingKey = -1);
    }
}