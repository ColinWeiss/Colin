﻿using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Modulars.Tiles;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 用以处理切片与物块碰撞的系统.
  /// </summary>
  public class EcsTileCollisionSystem : SectionSystem
  {
    private EnvironmentalController controller;
    private EcsComTransform comTransform;
    private EcsComPhysic comPhysic;

    public override void DoInitialize()
    {
      controller = Ecs.Controller;
      base.DoInitialize();
    }
    public override void DoUpdate()
    {
      Section _current;
      for (int count = 0; count < Ecs.Sections.Length; count++)
      {
        _current = Ecs.Sections[count];
        if (_current is null)
          continue;
        comTransform = _current.GetComponent<EcsComTransform>();
        comPhysic = _current.GetComponent<EcsComPhysic>();
        if (comTransform is null || comPhysic is null)
          continue;
        //安全性检查.
        if (comPhysic is not null)
        {
          if (!comPhysic.IgnoreGravity)
            comTransform.Velocity += controller.UniGravity.Value * Time.DeltaTime / comPhysic.UniGravitySpeedAttTime;
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
    public void HandleCollision(Section section)
    {
      Tile tile = Ecs.Scene.GetModule<Tile>();
      if (tile is null)
        return;
      if (comPhysic.IgnoreTile.Value)
        return;

      RectangleF bounds = GetHitBox(section);
      RectangleF previousBounds = bounds;
      previousBounds.Offset(-comTransform.DeltaVelocity);

      int leftTile = (int)Math.Floor((float)bounds.Left / TileOption.TileSize.X);
      int rightTile = (int)Math.Ceiling(((float)bounds.Right / TileOption.TileSize.X));
      int topTile = (int)Math.Floor((float)bounds.Top / TileOption.TileSize.Y);
      int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / TileOption.TileSizeF.Y);

      Vector2 vel = comTransform.DeltaVelocity;

      comPhysic.CollisionLeft = false;
      comPhysic.CollisionRight = false;
      comPhysic.CollisionBottom = false;
      comPhysic.CollisionTop = false;

       if (vel.X > 0)
         rightTile += (int)(vel.X / 16);
       else if (vel.X < 0)
         leftTile += (int)(vel.X / 16);

       if (vel.Y > 0)
         bottomTile += (int)(vel.Y / 16);
       else if (vel.Y < 0)
         topTile += (int)(vel.Y / 16);

      Vector2 depth;
      Vector2 v;
      Vector2 absV;
      TileInfo info;
      RectangleF target;
      RectangleF next = bounds;
      next.Location += comTransform.DeltaVelocity;

      float xt = 0;
      float yt = 0;


      bool positiveX = vel.X > 0;
      bool positiveY = vel.Y > 0;

      Vector2 AVel = comTransform.DeltaVelocity;
      for (int x = positiveX ? leftTile : rightTile; positiveX ? x <= rightTile : x >= leftTile; x += positiveX ? 1 : -1)
      {
        for (int y = positiveY ? topTile : bottomTile; positiveY ? y <= bottomTile : y >= topTile; y += positiveY ? 1 : -1)
        {
          info = tile[x, y, comPhysic.Layer];
          target = info.HitBox;
          if (next.Intersects(target) && info.Collision != Tiles.TileCollision.Passable &&
              !previousBounds.Intersects(target))
          {
            depth = GetEmbed(next, target, comTransform.DeltaVelocity);
            v = depth / comTransform.DeltaVelocity;
            v.X = AVel.X == 0 ? -float.MaxValue : v.X;
            v.Y = AVel.Y == 0 ? -float.MaxValue : v.Y;
            absV = -v;

            if (absV.X < absV.Y)
            {
              if (AVel.X < 0 && next.Left < target.Right)
              {
                xt = (bounds.Left - target.Right) / Math.Abs(AVel.X);
                comPhysic.CollisionLeft = true;
              }
              else if (AVel.X > 0 && next.Right > target.Left)
              {
                xt = (target.Left - bounds.Right) / Math.Abs(AVel.X);
                comPhysic.CollisionRight = true;
              }
              AVel.X *= xt;
              next = GetHitBox(section);
              next.Location += AVel;
              break;
            }
            else if (absV.X > absV.Y)
            {
              if (AVel.Y < 0 && next.Top < target.Bottom)
              {
                yt = (bounds.Top - target.Bottom) / Math.Abs(AVel.Y);
                comPhysic.CollisionTop = true;
              }
              else if (AVel.Y > 0 && next.Bottom > target.Top)
              {
                comPhysic.CollisionBottom = true;
                yt = (target.Top - bounds.Bottom) / AVel.Y;
              }
              AVel.Y *= yt;
              next = GetHitBox(section);
              next.Location += AVel;
              break;
            }
          }
        }
      }
      comTransform.Velocity = AVel / Time.DeltaTime;
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