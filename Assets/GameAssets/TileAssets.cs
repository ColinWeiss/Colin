using Colin.Core.ModLoaders;
using Colin.Core.Modulars.Tiles;
using System.Reflection;

namespace Colin.Core.Assets.GameAssets
{
    public class TileAssets : IGameAsset
    {
        public string Name => "物块资源";

        public float Progress { get; set; }

        private static Dictionary<string, TileBehavior> identDic = new Dictionary<string, TileBehavior>();
        private static Dictionary<Type, TileBehavior> typeDic = new Dictionary<Type, TileBehavior>();
        public static Dictionary<string, TileBehavior> IdentDic => identDic;
        public static Dictionary<Type, TileBehavior> TypeDic => typeDic;

        public void LoadResource()
        {
            Type[] _types = Assembly.GetExecutingAssembly().GetTypes();
            Type _type;
            TileBehavior _behavior;
            for(int count = 0; count < _types.Length; count++)
            {
                _type = _types[count];
                if(!_type.IsAbstract && _type.IsSubclassOf( typeof( TileBehavior ) ))
                {
                    _behavior = (TileBehavior)Activator.CreateInstance( _type );
                    typeDic.Add( _behavior.GetType(), _behavior );
                    identDic.Add( _behavior.Identifier, _behavior );
                }
            }
        }
        public static TileBehavior Get( string ident )
        {
            if(identDic.TryGetValue( ident, out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }
        public static TileBehavior Get( Type type )
        {
            if(typeDic.TryGetValue( type, out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }
        public static T Get<T>() where T : TileBehavior
        {
            if(typeDic.TryGetValue( typeof( T ), out TileBehavior behavior ))
                return behavior as T;
            else
                return null;
        }
    }
}