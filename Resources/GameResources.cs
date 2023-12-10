using Colin.Core.Modulars.Tiles;
using DeltaMachine.Core.GameContents.Sections.Items;

namespace Colin.Core.Resources
{
    public class GameResources
    {
        public static T GetItem<T>() where T : Item
        {
            return CodeResources<Item>.Get<T>();
        }
        public static T GetTileBehavior<T>() where T : TileBehavior
        {
            return CodeResources<TileBehavior>.Get<T>();
        }
    }
}