namespace Colin.Core.Graphics
{
    public interface IDrawBatcher<T> where T : unmanaged, IVertexType
    {
        public void DrawQuad(Texture2D texture, T vul, T vur, T vdr, T vdl, int sortingKey = -1);
#pragma warning disable CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。
        public void Flush(EffectPass? pass, bool clear = true);
#pragma warning restore CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。
        public void Flush(Effect effect, bool clear = true);
    }
}