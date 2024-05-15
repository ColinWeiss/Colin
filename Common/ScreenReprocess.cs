namespace Colin.Core.Common
{
  public class ScreenReprocess
  {
    public Dictionary<IRenderableISceneModule, Effect> Effects = new Dictionary<IRenderableISceneModule, Effect>();

    public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>();

    public void Add(IRenderableISceneModule iRComponent, Effect e)
    {
      Effects.Add(iRComponent, e);
      TypeCheck.Add(iRComponent.GetType(), e);
    }
  }
}