namespace Colin.Core.Modulars.Ecses
{
    public struct Entry<T> where T : IEquatable<T>
    {
        public T Value;
        public T Default;
        public Entry(T defaultValue)
        {
            Value = defaultValue;
            Default = defaultValue;
        }
        public void Reset()
        {
            Value = Default;
        }
    }
}