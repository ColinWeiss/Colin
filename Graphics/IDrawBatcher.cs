namespace Colin.Core.Graphics
{
    public interface IDrawBatcher<T> where T : unmanaged, IVertexType
    {
        public void DrawQuad(Texture2D texture, T vul, T vur, T vdr, T vdl, int sortingKey = -1);
        public void Flush(EffectPass? pass, bool clear = true);
        public void Flush(Effect effect, bool clear = true);
    }
}