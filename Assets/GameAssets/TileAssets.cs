using Colin.Core.Modulars.Tiles;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Colin.Core.Assets.GameAssets
{
    public class TileAssets : IGameResource
    {
        public string Name => "物块资源";

        public float Progress { get; set; }

        private static Dictionary<string, TileBehavior> _tiles = new Dictionary<string, TileBehavior>( );
        public static Dictionary<string, TileBehavior> Tiles => _tiles;

        public void Load( )
        {
            Type[ ] _types = Assembly.GetExecutingAssembly( ).GetTypes( );
            Type _type;
            TileBehavior _behavior;
            for(int count = 0 ; count < _types.Length ; count++)
            {
                _type = _types[count];
                if( !_type.IsAbstract && _type.IsSubclassOf( typeof( TileBehavior ) ))
                {
                    _behavior = (TileBehavior)Activator.CreateInstance( _type );
                    _tiles.Add( _behavior.Name , _behavior );
                }
            }
        }

        public static TileBehavior Get( string name )
        {
            if(_tiles.TryGetValue( name , out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }
    }
}