using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Modulars.Tiles;

namespace Colin.Core.Modulars.Ecses.Systems
{
  public class EcsPhysicSystem : SectionSystem
  {
    public EnvironmentalController Controller => Ecs.Controller;

    public EcsComTransform ComTransform;
    public EcsComPhysic ComPhysic;

    public const int VelocityStep = 16;

    public override void DoUpdate()
    {
      ComTransform = Current.GetComponent<EcsComTransform>();
      ComPhysic = Current.GetComponent<EcsComPhysic>();

      if (ComPhysic is not null)
      {
        if (!ComPhysic.IgnoreGravity)
          ComTransform.Velocity += Controller.UniGravity.Value * Time.DeltaTime / ComPhysic.UniGravitySpeedAttTime;
        ComTransform.Velocity -= ComPhysic.AirResistance;
      }
      if (ComPhysic is not null && ComTransform is not null)
      {
        ComPhysic.PreviousPosition = ComTransform.Translation;
        Vector2 _v = ComTransform.Velocity * Time.DeltaTime;
        ComPhysic.PreviousCollisionBottom = ComPhysic.CollisionBottom;
        if (_v.Length() > VelocityStep)
        {
          int _vCount = (int)(_v.Length() / VelocityStep);
          float _vRem = _v.Length() % VelocityStep;
          _v.Normalize();
          _v *= VelocityStep;
          while (_vCount - 1 >= 0)
          {
            ComTransform.Translation += _v;
            HandleCollisions(Current, _v);
            _vCount--;
          }
          _v.Normalize();
          _v *= _vRem;
          ComTransform.Translation += _v;
          HandleCollisions(Current, _v);
        }
        else
        {
          ComTransform.Translation += ComTransform.Velocity * Time.DeltaTime;
          HandleCollisions(Current, ComTransform.Velocity * Time.DeltaTime);
        }
        if (ComPhysic.CollisionLeft || ComPhysic.CollisionRight)
          ComTransform.Velocity.X = 0;
        if (ComPhysic.CollisionTop || ComPhysic.CollisionBottom)
          ComTransform.Velocity.Y = 0;
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

      Vector2 depth;
      Vector2 v;
      Vector2 absV;
      TileInfo info;
      for (int x = positiveX ? leftTile : rightTile; positiveX ? x <= rightTile : x >= leftTile; x += positiveX ? 1 : -1)
      {
        for (int y = positiveY ? topTile : bottomTile; positiveY ? y <= bottomTile : y >= topTile; y += positiveY ? 1 : -1)
        {
          info = tile[x, y, comPhysic.Layer];
          if (info.Collision != TileCollision.Passable)
          {
            if (bounds.Intersects(info.HitBox) &&
              !previousBounds.Intersects(info.HitBox))
            {
              depth = GetEmbed(bounds, info.HitBox, comTransform.Velocity * Time.DeltaTime);
              v = depth / comTransform.Velocity * Time.DeltaTime;
              v.X = comTransform.Velocity.X == 0 ? -float.MaxValue : v.X;
              v.Y = comTransform.Velocity.Y == 0 ? -float.MaxValue : v.Y;
              absV = -v;
              if (absV.X < absV.Y)
              {
                if (comTransform.Velocity.X < 0)
                  comPhysic.CollisionLeft = true;
                if (comTransform.Velocity.X > 0)
                  comPhysic.CollisionRight = true;
                if (comPhysic.CollisionRight || comPhysic.CollisionLeft)
                  comTransform.Translation.X += depth.X * 1.001f;
                bounds = GetHitBox(section);
              }
              else if (absV.X > absV.Y)
              {
                if (comTransform.Velocity.Y > 0)
                  comPhysic.CollisionBottom = true;
                if (comTransform.Velocity.Y < 0)
                  comPhysic.CollisionTop = true;
                if (comPhysic.CollisionTop || comPhysic.CollisionBottom)
                  comTransform.Translation.Y += depth.Y * 1.001f;
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
            comTransform.Translation.X + comPhysic.Hitbox.X,
            comTransform.Translation.Y + comPhysic.Hitbox.Y,
            comTransform.Size.X + comPhysic.Hitbox.Width,
            comTransform.Size.Y + comPhysic.Hitbox.Height
            );
      }
      else
        return RectangleF.Empty;
    }
  }
}