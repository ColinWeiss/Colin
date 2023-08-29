using FontStashSharp;

namespace Colin.Core.Assets
{
    public class FontResource : IGameResource
    {
        public static FontSystem Unifont { get; private set; }

        public static FontSystem GlowSans { get; private set; }

        public string Name { get; }

        public float Progress { get; set; }

        public void Load( )
        {
            Unifont = new FontSystem( );
            Unifont.AddFont( DeltaMachine.Assets.Assets.Unifont );

            GlowSans = new FontSystem( );
            GlowSans.AddFont( DeltaMachine.Assets.Assets.GlowSans );
        }
    }
}