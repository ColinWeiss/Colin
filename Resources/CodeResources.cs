using DeltaMachine.Core.GameContents.Sections.Items;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Colin.Core.Resources
{
    public class CodeResources<T0> where T0 : ICodeResource
    {
        public static Dictionary<Type, T0> Resources = new Dictionary<Type, T0>();
        public static Dictionary<string, Type> IdentifierMaps = new Dictionary<string, Type>();
        public static Dictionary<string, int> SerializeMaps = new Dictionary<string, int>();
        public static Dictionary<int, string> HashMaps = new Dictionary<int, string>();

        public static T1 Get<T1>() where T1 : T0 => (T1)Resources.GetValueOrDefault( typeof( T1 ) );
        public static T0 Get( Type type )
        {
            if(Resources.TryGetValue( type, out T0 value ))
                return value;
            else return default;
        }
        public static T0 Get( string identifier )
        {
            if(IdentifierMaps.TryGetValue( identifier, out Type type ))
                return Get( type );
            else return default;
        }

        public void Load()
        {
            Resources.Clear();
            SerializeMaps.Clear();
            foreach(var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(!item.IsAbstract && item.IsSubclassOf( typeof( T0 ) ))
                {
                    Resources.Add( item, (T0)Activator.CreateInstance( item ) );
                    IdentifierMaps.Add( item.FullName, item );
                    SerializeMaps.Add( item.FullName, item.FullName.GetMsnHashCode() );
                }
            }
        }
        public static void SaveMaps( string path )
        {
            using(FileStream fileStream = new FileStream( path, FileMode.Create ))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                JsonSerializer.Serialize( fileStream, SerializeMaps, SerializeMaps.GetType(), options );
            }
        }
        public static void LoadMaps( string path )
        {
            HashMaps.Clear();
            using(FileStream fileStream = new FileStream( path, FileMode.Open ))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                SerializeMaps = (Dictionary<string, int>)JsonSerializer.Deserialize( fileStream, SerializeMaps.GetType() );
                foreach(var item in SerializeMaps)
                    HashMaps.Add( item.Value , item.Key );
            }
        }
    }
}