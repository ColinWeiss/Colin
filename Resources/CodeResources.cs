using System.Reflection;
using System.Text.Json;

namespace Colin.Core.Resources
{
    public class CodeResources<T0> where T0 : ICodeResource
    {
        public static Dictionary<Type, T0> Resources = new Dictionary<Type, T0>();
        public static Dictionary<string, Type> IdentifierTable = new Dictionary<string, Type>();
        public static Dictionary<string, int> SerializeTable = new Dictionary<string, int>();
        public static Dictionary<int, string> HashMap = new Dictionary<int, string>();

        public static T1 Get<T1>() where T1 : T0 => (T1)Resources.GetValueOrDefault(typeof(T1));
        public static T0 Get(Type type)
        {
            if (Resources.TryGetValue(type, out T0 value))
                return value;
            else return default;
        }
        public static T0 Get(string identifier)
        {
            if (IdentifierTable.TryGetValue(identifier, out Type type))
                return Get(type);
            else return default;
        }

        public void Load()
        {
            Resources.Clear();
            SerializeTable.Clear();
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!item.IsAbstract && item.IsSubclassOf(typeof(T0)))
                {
                    Resources.Add(item, (T0)Activator.CreateInstance(item));
                    IdentifierTable.Add(item.FullName, item);
                    SerializeTable.Add(item.FullName, item.FullName.GetMsnHashCode());
                }
            }
            HashMap.Clear();
            foreach (var item in SerializeTable)
                HashMap.Add(item.Value, item.Key);
        }
        public static void SaveTable(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                JsonSerializer.Serialize(fileStream, SerializeTable, SerializeTable.GetType(), options);
            }
        }
        public static void LoadTable(string path)
        {
            HashMap.Clear();
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                SerializeTable = (Dictionary<string, int>)JsonSerializer.Deserialize(fileStream, SerializeTable.GetType());
                foreach (var item in SerializeTable)
                    HashMap.Add(item.Value, item.Key);
            }
        }
    }
}