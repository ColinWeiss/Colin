using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Colin.Core.Resources
{
    public class CodeResources<T0> where T0 : ICodeResource
    {
        public static Dictionary<Type, T0> TypeMaps = new Dictionary<Type, T0>();
        public static Dictionary<int, string> NameMaps = new Dictionary<int, string>();
        public static T1 Get<T1>() where T1 : T0 => (T1)TypeMaps.GetValueOrDefault( typeof( T1 ) );
        public void Load()
        {
            TypeMaps.Clear();
            NameMaps.Clear();
            foreach(var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(!item.IsAbstract && item.IsSubclassOf( typeof( T0 ) ))
                {
                    TypeMaps.Add( item, (T0)Activator.CreateInstance( item ) );
                    NameMaps.Add( NameMaps.Count , item.FullName );
                }
            }
        }

        public void SaveMaps( string path )
        {
            using(FileStream fileStream = new FileStream( path , FileMode.Create ) )
            {
                JsonSerializer.Serialize( fileStream , NameMaps );
            }
        }
    }
}