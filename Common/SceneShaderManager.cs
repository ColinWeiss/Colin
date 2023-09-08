namespace Colin.Core.Common
{
    public class SceneShaderManager
    {

        /* 项目“DeltaMachine.Desktop”的未合并的更改
        在此之前:
                public Dictionary<IRenderableSceneComponent, Effect> Effects = new Dictionary<IRenderableSceneComponent, Effect>( );

                public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>( );
        在此之后:
                public Dictionary<IRenderableSceneComponent, Effect> Effects = new Dictionary<IRenderableSceneComponent, Effect>( );

                public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>( );
        */
        public Dictionary<IRenderableSceneComponent, Effect> Effects = new Dictionary<IRenderableSceneComponent, Effect>();

        public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>();

        public void Add( IRenderableSceneComponent iRComponent, Effect e )
        {
            Effects.Add( iRComponent, e );
            TypeCheck.Add( iRComponent.GetType(), e );
        }
    }
}