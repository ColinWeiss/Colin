using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Modulars.Tiles;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 用以处理实体与物块碰撞的系统.
  /// </summary>
  public class EcsTileCollisionSystem : Entitiesystem
  {
    private EnvironmentalController controller;
    private EcsComTransform comTransform;
    private EcsComTileInteract comPhysic;

    public override void DoInitialize()
    {
      controller = Ecs.Controller;
      base.DoInitialize();
    }
    public override void DoUpdate()
    {
      Entity _current;
      for (int count = 0; count < Ecs.Entities.Length; count++)
      {
        _current = Ecs.Entities[count];
        if (_current is null)
          continue;
        comTransform = _current.GetCom<EcsComTransform>();
        comPhysic = _current.GetCom<EcsComTileInteract>();
        if (comTransform is null || comPhysic is null)
          continue;
        //安全性检查.
        if (comPhysic is not null)
        {
          if (!comPhysic.IgnoreGravity)
            comTransform.Velocity += controller.UniGravity * Time.DeltaTime / comPhysic.UniGravitySpeedAttTime;
        }
        //添加重力.
        if (comPhysic is not null && comTransform is not null)
        {
          comPhysic.PreviousCollisionLeft = comPhysic.CollisionLeft;
          comPhysic.PreviousCollisionRight = comPhysic.CollisionRight;
          comPhysic.PreviousCollisionTop = comPhysic.CollisionTop;
          comPhysic.PreviousCollisionBottom = comPhysic.CollisionBottom;
          HandleCollision(_current);
          //处理速度.
        }
      }
      base.DoUpdate();
    }
    public void HandleCollision(Entity Entity)
    {
      Tile tile = Ecs.Scene.GetModule<Tile>();
      if (tile is null)
        return;
      if (comPhysic.IgnoreTile)
        return;

      RectangleF bounds = GetHitBox(Entity);
      RectangleF previousBounds = bounds;
      previousBounds.Offset(-comTransform.DeltaVelocity);

      int leftTile = (int)Math.Floor((float)bounds.Left / tile.Option.TileSize.X);
      int rightTile = (int)Math.Ceiling(((float)bounds.Right / tile.Option.TileSize.X));
      int topTile = (int)Math.Floor((float)bounds.Top / tile.Option.TileSize.Y);
      int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / tile.Option.TileSizeF.Y);

      Vector2 deltaVel = comTransform.DeltaVelocity;

      comPhysic.CollisionLeft = false;
      comPhysic.CollisionRight = false;
      comPhysic.CollisionBottom = false;
      comPhysic.CollisionTop = false;

      if (deltaVel.X > 0)
        rightTile += (int)(deltaVel.X / 16);
      else if (deltaVel.X < 0)
        leftTile += (int)(deltaVel.X / 16) - 1;

      if (deltaVel.Y > 0)
        bottomTile += (int)(deltaVel.Y / 16);
      else if (deltaVel.Y < 0)
        topTile += (int)(deltaVel.Y / 16) - 1;

      Vector2 depth;
      Vector2 v;
      Vector2 absV;
      TileInfo info;
      RectangleF target;
      RectangleF next = bounds;
      next.Location += comTransform.DeltaVelocity;

      float xt = 0;
      float yt = 0;

      bool positiveX = deltaVel.X > 0;
      bool positiveY = deltaVel.Y > 0;

      for (int x = positiveX ? leftTile : rightTile; positiveX ? x <= rightTile : x >= leftTile; x += positiveX ? 1 : -1)
      {
        for (int y = positiveY ? topTile : bottomTile; positiveY ? y <= bottomTile : y >= topTile; y += positiveY ? 1 : -1)
        {
          info = tile[x, y, comPhysic.Layer];
          if (info.IsNull)
            return;
          target = info.HitBox;
          if (
            next.Intersects(target) &&
            info.Collision != Tiles.TileCollision.Passable &&
            !previousBounds.Intersects(target))
          {
            depth = GetEmbed(next, target, comTransform.DeltaVelocity);
            v = depth / comTransform.DeltaVelocity;
            v.X = deltaVel.X == 0 ? -float.MaxValue : v.X;
            v.Y = deltaVel.Y == 0 ? -float.MaxValue : v.Y;
            absV = -v;
            if (absV.X < absV.Y)
            {
              if (info.Collision != TileCollision.Platform)
              {
                if (deltaVel.X < 0 && next.Left < target.Right)
                {
                  xt = (bounds.Left - target.Right) / Math.Abs(deltaVel.X);
                  comPhysic.CollisionLeft = true;
                }
                else if (deltaVel.X > 0 && next.Right > target.Left)
                {
                  xt = (target.Left - bounds.Right) / Math.Abs(deltaVel.X);
                  comPhysic.CollisionRight = true;
                }
                deltaVel.X *= xt;
              }
              next = GetHitBox(Entity);
              next.Location += deltaVel;
              break;
            }
            else if (absV.X > absV.Y)
            {
              if (deltaVel.Y > 0 && next.Bottom > target.Top)
              {
                yt = (target.Top - bounds.Bottom) / Math.Abs(deltaVel.Y);
                comPhysic.CollisionBottom = true;
              }
              else if (deltaVel.Y < 0 && next.Top < target.Bottom)
              {
                yt = (bounds.Top - target.Bottom) / Math.Abs(deltaVel.Y);
                comPhysic.CollisionTop = true;
              }
              deltaVel.Y *= yt;
              next = GetHitBox(Entity);
              next.Location += deltaVel;
              break;
            }
          }
        }

        comTransform.Velocity = deltaVel / Time.DeltaTime;
      }
    }

    public RectangleF GetHitBox(Entity Entity)
    {
      EcsComTransform comTransform = Entity?.GetCom<EcsComTransform>();
      EcsComTileInteract comPhysic = Entity?.GetCom<EcsComTileInteract>();
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

    private Vector2 GetEmbed(RectangleF rectA, RectangleF rectB, Vector2 velocity)
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
  }
}