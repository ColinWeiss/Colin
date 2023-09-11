using Colin.Core.ModLoaders;
using Colin.Core.Modulars.Tiles;
using System.Reflection;

namespace Colin.Core.Assets.GameAssets
{
    public class TileAssets : IGameAsset, IModResource
    {
        public string Name => "物块资源";

        public float Progress { get; set; }

        private static Dictionary<string, TileBehavior> _behaviors = new Dictionary<string, TileBehavior>();
        public static Dictionary<string, TileBehavior> Behaviors => _behaviors;

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
                    _behaviors.Add( _behavior.Name, _behavior );
                }
            }
        }

        public void LoadModResources( IMod mod )
        {
            Type[] _types = ModContent.GetCode( mod.GetType().Name ).GetTypes();
            Type _type;
            TileBehavior _behavior;
            for(int count = 0; count < _types.Length; count++)
            {
                _type = _types[count];
                if(!_type.IsAbstract && _type.IsSubclassOf( typeof( TileBehavior ) ))
                {
                    _behavior = (TileBehavior)Activator.CreateInstance( _type );
                    _behaviors.Add( _behavior.Name, _behavior );
                }
            }
        }

        public static TileBehavior Get( string name )
        {
            if(_behaviors.TryGetValue( name, out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }
    }
}