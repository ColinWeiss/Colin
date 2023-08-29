using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.IO
{
    /// <summary>
    /// 数据存储箱.
    /// </summary>
    public class StoreBox
    {
       private Dictionary<string, object> dict = new Dictionary<string, object>( );

        public void Add( string key , object value ) => dict.Add( key , value );

        public object Get( string key )
        {
            if( dict.TryGetValue( key, out object value ) )
                return value;
            else
                return null;
        }

        public T Get<T>( string key )
        {
            return (T)Get( key );
        }

        public void Save( string filePath )
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Dictionary<string, object> ) );
            MemoryStream stream = new MemoryStream( );
            serializer.WriteObject( stream , dict );
            string datas = Encoding.UTF8.GetString( stream.ToArray( ) );
            stream.Close( );
            File.WriteAllText( filePath, datas );
        }

        public void Load( string filePath )
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Dictionary<string, object> ) );
            MemoryStream stream = new MemoryStream( );
            stream.Read( File.ReadAllBytes( filePath ) );
            dict = (Dictionary<string, object>)serializer.ReadObject( stream );
        }
    }
}