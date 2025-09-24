namespace Colin.Core.Modulars.Ecses
{
  public interface IEcsComBindable : IEcsCom
  {
    Entity Entity { get; set; }
  }
}
