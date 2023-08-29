using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Common
{
    public class SceneShaderManager
    {
        public Dictionary<IRenderableSceneComponent, Effect> Effects = new Dictionary<IRenderableSceneComponent, Effect>( );
        
        public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>( );

        public void Add( IRenderableSceneComponent iRComponent , Effect e )
        {
            Effects.Add( iRComponent , e );
            TypeCheck.Add( iRComponent.GetType( ) , e );
        }
    }
}