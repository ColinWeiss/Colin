namespace Colin.Core.Modulars.Ecses
{
  public interface IEcsComBindable : IEcsCom
  {
    public Entity Entity { get; set; }
  }
}
