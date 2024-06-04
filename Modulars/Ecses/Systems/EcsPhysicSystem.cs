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
      Section _current;
      Vector2 _sectionVel;
      int _sectionVelStepCount;
      float _sectionVelRem;
      for (int count = 0; count < Ecs.Sections.Length; count++)
      {
        _current = Ecs.Sections[count];
        if (_current is null)
          continue;
        ComTransform = _current.GetComponent<EcsComTransform>();
        ComPhysic = _current.GetComponent<EcsComPhysic>();
        if (ComTransform is null || ComPhysic is null)
          continue;
        if (ComPhysic is not null)
        {
          if (!ComPhysic.IgnoreGravity)
            ComTransform.Velocity += Controller.UniGravity.Value * Time.DeltaTime / ComPhysic.UniGravitySpeedAttTime;
          ComTransform.Velocity -= ComPhysic.AirResistance;
        }
        if (ComPhysic is not null && ComTransform is not null)
        {
          ComPhysic.PreviousPosition = ComTransform.Translation;
          _sectionVel = ComTransform.Velocity * Time.DeltaTime;
          ComPhysic.PreviousCollisionBottom = ComPhysic.CollisionBottom;
          {
            if (_sectionVel.Length() > VelocityStep)
            {
              _sectionVelStepCount = (int)(_sectionVel.Length() / VelocityStep);
              _sectionVelRem = _sectionVel.Length() % VelocityStep;
              _sectionVel.Normalize();
              _sectionVel *= VelocityStep;
              while (_sectionVelStepCount - 1 >= 0)
              {
                ComTransform.Translation += _sectionVel;
                HandleCollisions(_current, _sectionVel);
                _sectionVelStepCount--;
              }
              _sectionVel.Normalize();
              _sectionVel *= _sectionVelRem;
              ComTransform.Translation += _sectionVel;
              HandleCollisions(_current, _sectionVel);
            }
            else
            {
              ComTransform.Translation += ComTransform.Velocity * Time.DeltaTime;
              HandleCollisions(_current, ComTransform.Velocity * Time.DeltaTime);
            }
            if (ComPhysic.CollisionLeft || ComPhysic.CollisionRight)
              ComTransform.Velocity.X = 0;
            if (ComPhysic.CollisionTop || ComPhysic.CollisionBottom)
              ComTransform.Velocity.Y = 0;
          }
        }
      }
      base.DoUpdate();
    }
    public void HandleCollisions(Section section, Vector2 deltaPosition)
    {
      Tile tile = Ecs.Scene.GetModule<Tile>();
      if (tile is null)
        return;
      if (ComPhysic.IgnoreTile.Value)
        return;
      RectangleF bounds = GetHitBox(section);
      RectangleF previousBounds = bounds;
      previousBounds.Offset(-deltaPosition);

      int leftTile = (int)Math.Floor((float)bounds.Left / TileOption.TileSize.X);
      int rightTile = (int)Math.Floor(((float)bounds.Right / TileOption.TileSize.X));
      int topTile = (int)Math.Floor((float)bounds.Top / TileOption.TileSize.Y);
      int bottomTile = (int)Math.Floor((float)bounds.Bottom / TileOption.TileSizeF.Y);

      bool positiveX = ComTransform.Velocity.X > 0;
      bool positiveY = ComTransform.Velocity.Y > 0;
      ComPhysic.CollisionLeft = false;
      ComPhysic.CollisionRight = false;
      ComPhysic.CollisionBottom = false;
      ComPhysic.CollisionTop = false;

      Vector2 depth;
      Vector2 v;
      Vector2 absV;
      TileInfo info;
      for (int x = positiveX ? leftTile : rightTile; positiveX ? x <= rightTile : x >= leftTile; x += positiveX ? 1 : -1)
      {
        for (int y = positiveY ? topTile : bottomTile; positiveY ? y <= bottomTile : y >= topTile; y += positiveY ? 1 : -1)
        {
          info = tile[x, y, ComPhysic.Layer];
          if (info.Collision != TileCollision.Passable)
          {
            if (bounds.Intersects(info.HitBox) &&
              !previousBounds.Intersects(info.HitBox))
            {
              depth = GetEmbed(bounds, info.HitBox, ComTransform.Velocity * Time.DeltaTime);
              v = depth / ComTransform.Velocity * Time.DeltaTime;
              v.X = ComTransform.Velocity.X == 0 ? -float.MaxValue : v.X;
              v.Y = ComTransform.Velocity.Y == 0 ? -float.MaxValue : v.Y;
              absV = -v;
              if (absV.X < absV.Y)
              {
                if (ComTransform.Velocity.X < 0)
                  ComPhysic.CollisionLeft = true;
                if (ComTransform.Velocity.X > 0)
                  ComPhysic.CollisionRight = true;
                if (ComPhysic.CollisionRight || ComPhysic.CollisionLeft)
                  ComTransform.Translation.X += depth.X * 1.001f;
                bounds = GetHitBox(section);
              }
              else if (absV.X > absV.Y)
              {
                if (ComTransform.Velocity.Y > 0)
                  ComPhysic.CollisionBottom = true;
                if (ComTransform.Velocity.Y < 0)
                  ComPhysic.CollisionTop = true;
                if (ComPhysic.CollisionTop || ComPhysic.CollisionBottom)
                  ComTransform.Translation.Y += depth.Y * 1.001f;
                bounds = GetHitBox(section);
              }
            }
          }
        }
      }
      ComPhysic.PreviousBottom = bounds.Bottom;
      ComPhysic.PreviousRight = bounds.Right;
      ComPhysic.PreviousTop = bounds.Top;
      ComPhysic.PreviousLeft = bounds.Left;
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