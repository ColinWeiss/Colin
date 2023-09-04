using Colin.Core.IO;
using Colin.Core.ModLoaders;
using Colin.Core.Modulars.Tiles;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Colin.Core.Assets.GameAssets
{
    public class TileAssets : IGameResource, IModResource
    {
        public string Name => "物块资源";

        public float Progress { get; set; }

        private static Dictionary<string, TileBehavior> _behaviors = new Dictionary<string, TileBehavior>( );
        public static Dictionary<string, TileBehavior> Behaviors => _behaviors;

        public void LoadResource( )
        {
            Type[ ] _types = Assembly.GetExecutingAssembly( ).GetTypes( );
            Type _type;
            TileBehavior _behavior;
            string _resourceName;
            for(int count = 0 ; count < _types.Length ; count++)
            {
                _type = _types[count];
                if(!_type.IsAbstract && _type.IsSubclassOf( typeof( TileBehavior ) ))
                {
                    _behavior = (TileBehavior)Activator.CreateInstance( _type );
                    _resourceName = string.Concat( ModContent.GetModDomain( EngineInfo.Engine ), _behavior.Name );
                    _behaviors.Add( _resourceName, _behavior );
                }
            }
        }

        public void LoadModResources( IMod mod )
        {
            Type[ ] _types = ModContent.GetCode( mod.GetType( ).Name ).GetTypes( );
            Type _type;
            TileBehavior _behavior;
            string _resourceName;
            for(int count = 0 ; count < _types.Length ; count++)
            {
                _type = _types[count];
                if(!_type.IsAbstract && _type.IsSubclassOf( typeof( TileBehavior ) ))
                {
                    _behavior = (TileBehavior)Activator.CreateInstance( _type );
                    _resourceName = string.Concat( ModContent.GetModDomain( mod ), _behavior.Name );
                    _behaviors.Add( _resourceName, _behavior );
                }
            }
        }

        public static TileBehavior Get( string name )
        {
            if(_behaviors.TryGetValue( string.Concat( ModContent.GetModDomain( EngineInfo.Engine ), name ), out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }

        public static TileBehavior Get( IMod mod, string name )
        {
            if(_behaviors.TryGetValue( string.Concat( ModContent.GetModDomain( mod ) , name ), out TileBehavior behavior ))
                return behavior;
            else
                return null;
        }
    }
}