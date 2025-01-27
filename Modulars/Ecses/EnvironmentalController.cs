namespace Colin.Core.Modulars.Ecses
{
  public class EnvironmentalController
  {
    /// <summary>
    /// 重力.
    /// </summary>
    public Vector2 UniGravity;

    public void DoInitialize()
    {
      UniGravity = Vector2.UnitY * 400;
    }
    public void Reset()
    {
      UniGravity = Vector2.UnitY * 400;
    }
  }
}