/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Graphics;
using Colin.Core.Assets;
在此之后:
using Colin.Core.Common;
using Colin.Core.Graphics;
*/

namespace Colin.Core.Modulars.Meteors
{
    public class MeteorCreator : ISceneComponent, IRenderableSceneComponent
    {
        public Meteor[] Pool { get; set; } = new Meteor[256];

        private List<Meteor> _meteors;

        public List<Meteor> ActiveList
        {
            get
            {
                return _meteors;
            }
            set
            {
                _meteors = value;
            }
        }

        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public bool Visible { get; set; }

        public RenderTarget2D SceneRt { get; set; }

        public Sprite MeteorSprite;

        public void DoInitialize()
        {
            MeteorSprite = new Sprite( TextureAssets.Get( "Extras/Meteor_0" ) );
            Span<Meteor> pool = Pool;
            pool.Fill( new Meteor() );
            ActiveList = new List<Meteor>();
        }

        public void DoUpdate( GameTime time )
        {
            if((int)EngineInfo.Config.PictureQuality <= 0)
                return;
            Meteor meteor;
            for(int count = 0; count < ActiveList.Count; count++)
            {
                meteor = _meteors[count];
                if(meteor.Active)
                {
                    meteor.LeftFrame--;
                    if(meteor.Scale > 0)
                        meteor.Scale -= 0.002f;
                    meteor.Position += meteor.Velocity;
                    if(meteor.LeftFrame <= 0)
                    {
                        meteor.Dormancy();
                        ActiveList.Remove( meteor );
                    }
                }
            }
        }

        public void DoRender( SpriteBatch batch )
        {
            if((int)EngineInfo.Config.PictureQuality <= 0)
                return;
            Meteor meteor;
            for(int count = 0; count < ActiveList.Count; count++)
            {
                meteor = ActiveList[count];
                if(meteor.Active)
                    EngineInfo.SpriteBatch.Draw( MeteorSprite.Source, meteor.Position - new Vector2( 0, 32 ), null,
                        Color.White, meteor.Rotation, new Vector2( 0, 32 ), meteor.Scale, SpriteEffects.None, MeteorSprite.Depth );
            }
        }

        public void New( int leftFrame, Vector2 position, Vector2 velocity, float rotation, float scale )
        {
            if(EngineInfo.Config.PictureQuality == PictureQuality.Low)
                return;
            Meteor meteor;
            for(int count = 0; count < Pool.Length; count++)
            {
                meteor = Pool[count];
                if(!meteor.Active)
                {
                    meteor = new Meteor();
                    meteor.Active = true;
                    meteor.LeftFrame = leftFrame;
                    meteor.Position = position;
                    meteor.Velocity = velocity;
                    meteor.Rotation = rotation;
                    meteor.Scale = scale;
                    meteor.ActiveIndex = ActiveList.Count;
                    ActiveList.Add( meteor );
                    break;
                }
            }
        }

        public void New( int leftFrame, Vector2 position, Vector2 velocity, float rotation )
        {
            New( leftFrame, position, velocity, rotation, 1f );
        }

        public void New( int leftFrame, Vector2 position, Vector2 velocity )
        {
            New( leftFrame, position, velocity, 0f, 1f );
        }

    }
}