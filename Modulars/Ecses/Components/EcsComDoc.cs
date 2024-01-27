using System.Collections;

namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// 切片文档.
    /// </summary>
    public class EcsComDoc : ISectionComponent
    {
        /// <summary>
        /// 指示切片的标识符.
        /// <br>切片判断相等的依据之一.</br>
        /// </summary>
        public string Identifier = "";
        public string Description = "";
        /// <summary>
        /// 指示切片标签.
        /// </summary>
        public HashSet<string> Tags = new HashSet<string>();

        public void DoInitialize() { }

        public bool Equals(ISectionComponent other)
        {
            bool result = false;
            if (other is EcsComDoc doc)
            {
                result = Identifier.Equals(doc.Identifier);
                return result && Tags.SetEquals(doc.Tags);
            }
            return result;
        }
    }
}