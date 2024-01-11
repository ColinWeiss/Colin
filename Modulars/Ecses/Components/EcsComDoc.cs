using System.Collections;

namespace Colin.Core.Modulars.Ecses.Components
{
    public class EcsComDoc : ISectionComponent
    {
        public string Name = "";
        public string DisplayName = "";
        public string Description = "";
        private List<string> tags;
        public List<string> Tags
        {
            get
            {
                if (tags is null)
                    tags = new List<string>();
                return tags;
            }
        }

        public void DoInitialize() { }

        public bool Equals(ISectionComponent other)
        {
            bool result = false;
            if (other is EcsComDoc doc)
            {
                result =
                    Name.Equals(doc.Name) &&
                    DisplayName.Equals(doc.DisplayName) &&
                    Description.Equals(doc.Description);
                Tags.Sort();
                doc.Tags.Sort();
                return result && Tags.SequenceEqual(doc.Tags);
            }
            return result;
        }
    }
}