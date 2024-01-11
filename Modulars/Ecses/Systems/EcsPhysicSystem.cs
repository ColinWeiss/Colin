using Colin.Core.Modulars.Tiles;
using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
    public class EcsPhysicSystem : SectionSystem
    {
        public EcsComTransform comTransform => Current.GetComponent<EcsComTransform>();
        public EcsComPhysic comPhysic => Current.GetComponent<EcsComPhysic>();
     //   public PlayerComMove comMove => Current.GetComponent<PlayerComMove>();
        public EnvironmentalController Controller => Ecs.Controller;
        public const int VelocityStep = 16;
        public event Action<Section> OnVelocityStep;
        public override void DoUpdate()
        {
            if (comPhysic is not null)
            {
                if (!comPhysic.IgnoreGravity)
                {
                    comTransform.Velocity += Controller.UniGravity * Time.DeltaTime;
                }
            }
            if (comPhysic is not null && comTransform is not null)
            {
     //           if (comMove is not null)
                {
     //              comTransform.Velocity.Y += comMove.FallSpeedInc.Value * Time.DeltaTime;
                }
                comPhysic.PreviousPosition = comTransform.Position;
                Vector2 _v = comTransform.Velocity;
                comPhysic.PreviousCollisionBottom = comPhysic.CollisionBottom;
                if (_v.Length() > VelocityStep)
                {
                    int _vCount = (int)(_v.Length() / VelocityStep);
                    float _vRem = _v.Length() % VelocityStep;
                    _v.Normalize();
                    _v *= VelocityStep;
                    while (_vCount - 1 >= 0)
                    {
                        comTransform.Position += _v;
                        OnVelocityStep?.Invoke(Current);
                        HandleCollisions(Current, _v);
                        _vCount--;
                    }
                    _v.Normalize();
                    _v *= _vRem;
                    comTransform.Position += _v;
                    HandleCollisions(Current, _v);
                }
                else
                {
                    comTransform.Position += comTransform.Velocity;
                    HandleCollisions(Current, comTransform.Velocity);
                }
                if (comPhysic.CollisionLeft || comPhysic.CollisionRight)
                    comTransform.Velocity.X = 0;
                if (comPhysic.CollisionTop || comPhysic.CollisionBottom)
                    comTransform.Velocity.Y = 0;
            }
            base.DoUpdate();
        }
        public static Vector2 GetEmbed(RectangleF rectA, RectangleF rectB, Vector2 velocity)
        {
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;
            float depthX = velocity.X < 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = velocity.Y < 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }
        public void HandleCollisions(Section section, Vector2 deltaPosition)
        {
            Tile tile = Ecs.Scene.GetModule<Tile>();
            EcsComPhysic comPhysic = section?.GetComponent<EcsComPhysic>();
            if (comPhysic.IgnoreTile.Value)
                return;
            EcsComTransform comTransform = section?.GetComponent<EcsComTransform>();
            RectangleF bounds = GetHitBox(section);
            RectangleF previousBounds = bounds;
            previousBounds.Offset(-deltaPosition);

            int leftTile = (int)Math.Floor((float)bounds.Left / TileOption.TileSize.X);
            int rightTile = (int)Math.Floor(((float)bounds.Right / TileOption.TileSize.X)) + 1;
            int topTile = (int)Math.Floor((float)bounds.Top / TileOption.TileSize.Y);
            int bottomTile = (int)Math.Floor((float)bounds.Bottom / TileOption.TileSizeF.Y) + 1;

            bool positiveX = comTransform.Velocity.X > 0;
            bool positiveY = comTransform.Velocity.Y > 0;
            comPhysic.CollisionLeft = false;
            comPhysic.CollisionRight = false;
            comPhysic.CollisionBottom = false;
            comPhysic.CollisionTop = false;
            for (int x = positiveX ? leftTile : rightTile; positiveX ? x <= rightTile : x >= leftTile; x += positiveX ? 1 : -1)
            {
                for (int y = positiveY ? topTile : bottomTile; positiveY ? y <= bottomTile : y >= topTile; y += positiveY ? 1 : -1)
                {
                    if (tile[x, y, comPhysic.Layer].Collision != TileCollision.Passable)
                    {
                        if (bounds.Intersects(tile[x, y, comPhysic.Layer].HitBox) &&
                          !previousBounds.Intersects(tile[x, y, comPhysic.Layer].HitBox))
                        {
                            Vector2 depth = GetEmbed(bounds, tile[x, y, comPhysic.Layer].HitBox, comTransform.Velocity);
                            Vector2 v = depth / comTransform.Velocity;
                            v.X = comTransform.Velocity.X == 0 ? -float.MaxValue : v.X;
                            v.Y = comTransform.Velocity.Y == 0 ? -float.MaxValue : v.Y;
                            Vector2 absV = -v;
                            if (absV.X < absV.Y && tile[x, y, comPhysic.Layer].Collision != TileCollision.Platform)
                            {
                                if (comTransform.Velocity.X < 0)
                                    comPhysic.CollisionLeft = true;
                                if (comTransform.Velocity.X > 0)
                                    comPhysic.CollisionRight = true;
                                if (comPhysic.CollisionRight || comPhysic.CollisionLeft)
                                    comTransform.Position.X += depth.X * 1.0001f;
                                bounds = GetHitBox(section);
                            }
                            else if (absV.X > absV.Y)
                            {
                                if (comTransform.Velocity.Y > 0)
                                    comPhysic.CollisionBottom = true;
                                if (comTransform.Velocity.Y < 0)
                                    comPhysic.CollisionTop = true;
                                if (comPhysic.CollisionTop || comPhysic.CollisionBottom)
                                    comTransform.Position.Y += depth.Y * 1.0001f;
                                bounds = GetHitBox(section);
                            }
                        }
                    }
                }
            }
            comPhysic.PreviousBottom = bounds.Bottom;
            comPhysic.PreviousRight = bounds.Right;
            comPhysic.PreviousTop = bounds.Top;
            comPhysic.PreviousLeft = bounds.Left;
        }
        public RectangleF GetHitBox(Section section)
        {
            EcsComTransform comTransform = section?.GetComponent<EcsComTransform>();
            EcsComPhysic comPhysic = section?.GetComponent<EcsComPhysic>();
            if (comTransform is not null && comPhysic is not null)
            {
                return new RectangleF(
                    comTransform.Position.X + comPhysic.Hitbox.X,
                    comTransform.Position.Y + comPhysic.Hitbox.Y,
                    comTransform.Size.X + comPhysic.Hitbox.Width,
                    comTransform.Size.Y + comPhysic.Hitbox.Height
                    );
            }
            else
                return RectangleF.Empty;
        }
    }
}