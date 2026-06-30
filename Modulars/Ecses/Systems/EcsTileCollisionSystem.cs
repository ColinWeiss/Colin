using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Modulars.Tiles;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 用以处理实体与物块碰撞的系统.
  /// </summary>
  public class EcsTileCollisionSystem : Entitiesystem
  {
    private EcsComTransform comTransform;
    private EcsComTileInteract comPhysic;

    public Tile Tile => Ecs.Scene.GetModule<Tile>();

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
        if (comPhysic is not null && comTransform is not null)
        {
          comPhysic.PreviousCollisionLeft = comPhysic.CollisionLeft;
          comPhysic.PreviousCollisionRight = comPhysic.CollisionRight;
          comPhysic.PreviousCollisionTop = comPhysic.CollisionTop;
          comPhysic.PreviousCollisionBottom = comPhysic.CollisionBottom;
          comPhysic.PreviousSlopeCollision = comPhysic.SlopeCollision;
          HandleCollision(_current);
        }
      }
      base.DoUpdate();
    }
    public void HandleCollision(Entity Entity)
    {
      if (Tile is null)
        return;
      if (comPhysic.IgnoreTile)
        return;

      RectangleF bounds = GetHitBox(Entity);
      RectangleF previousBounds = bounds;
      previousBounds.Offset(-comTransform.DeltaVelocity);

      int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Context.TileSize.X);
      int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Context.TileSize.X));
      int topTile = (int)Math.Floor((float)bounds.Top / Tile.Context.TileSize.Y);
      int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Context.TileSizeF.Y);

      Vector2 deltaVel = comTransform.DeltaVelocity;

      comPhysic.CollisionLeft = false;
      comPhysic.CollisionRight = false;
      comPhysic.CollisionBottom = false;
      comPhysic.CollisionTop = false;
      comPhysic.SlopeCollision = false;

      if (deltaVel.X > 0)
        rightTile += (int)(deltaVel.X / 24);
      else if (deltaVel.X < 0)
        leftTile += (int)(deltaVel.X / 24) - 1;

      if (deltaVel.Y > 0)
        bottomTile += (int)(deltaVel.Y / 24);
      else if (deltaVel.Y < 0)
        topTile += (int)(deltaVel.Y / 24) - 1;

      Vector2 depth;
      Vector2 v;
      Vector2 absV;
      ref TileInfo info = ref TileInfo.Null;
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
          info = ref Tile[x, y, comPhysic.Layer];

          if (info.IsNull)
            continue;

          target = GetTileBounds(ref info);

          if (!next.Intersects(target))
            continue;

          bool isSlope = info.Collision == TileSolid.SlopeLeftUp ||
                         info.Collision == TileSolid.SlopeRightUp ||
                         info.Collision == TileSolid.SlopeLeftDown ||
                         info.Collision == TileSolid.SlopeRightDown;

          if (isSlope)
          {
            bool firstContact = !previousBounds.Intersects(target);
            if (HandleSlopeCollision(ref info, ref deltaVel, ref next, bounds, previousBounds, target, firstContact, Entity))
            {
              comTransform.Vel = deltaVel / Time.DeltaTime;
              continue;
            }
            // 斜坡条件不满足, 回退到方块碰撞处理（仅首帧接触时）
            if (!firstContact)
              continue;
          }

          if ((isSlope || info.Collision == TileSolid.Sturdy || info.Loading) &&
              !previousBounds.Intersects(target))
          {
            depth = GetEmbed(next, target, comTransform.DeltaVelocity);
            v = depth / comTransform.DeltaVelocity;
            v.X = deltaVel.X == 0 ? -float.MaxValue : v.X;
            v.Y = deltaVel.Y == 0 ? -float.MaxValue : v.Y;
            absV = -v;
            if (absV.X < absV.Y)
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
        comTransform.Vel = deltaVel / Time.DeltaTime;
      }
    }

    /// <summary>
    /// 处理斜坡碰撞.
    /// </summary>
    /// <param name="firstContact">是否为首帧接触（previousBounds 不与 target 相交）.</param>
    /// <returns>如果斜坡碰撞成功处理则返回 <c>true</c>；否则返回 <c>false</c> 表示应回退到方块碰撞处理.</returns>
    private bool HandleSlopeCollision(ref TileInfo info, ref Vector2 deltaVel, ref RectangleF next,
      RectangleF bounds, RectangleF previousBounds, RectangleF target, bool firstContact, Entity Entity)
    {
      TileSolid slopeType = info.Collision;
      // 使用 next（预计下帧位置）计算坡面高度
      float slopeSurfaceY = GetSlopeSurfaceY(slopeType, target, next);

      
      if (firstContact)
      {
        // 首帧接触：做实心侧和上坡起点检测（用当前 bounds 和上一帧 previousBounds 判定来向）
        if (IsOnSolidSideOfSlope(slopeType, target, bounds, previousBounds, deltaVel))
          return false;

        if (!CanStartClimbingSlope(slopeType, target, bounds, previousBounds, deltaVel))
          return false;
      }
      else
      {
        // 非首帧接触：检测实体是否本就处于斜坡实心侧（即被直接放入体内，而非从前一帧骑坡而来）
        // 仅用 previousBounds 判定，避免重力微弱穿透导致的误判
        if (WasPreviousInSolidSideOfSlope(slopeType, target, previousBounds))
          return false;
      }

      // 斜坡碰撞：根据斜坡表面高度调整实体Y位置
      if (slopeType == TileSolid.SlopeLeftUp || slopeType == TileSolid.SlopeRightUp)
      {
        // 地面斜坡
        float penetration = next.Bottom - slopeSurfaceY;
        // 实体在坡面上方且正在向上移动（跳跃）→ 不贴合, 允许跳离
        if (penetration <= 0)
        {
          comPhysic.IsOnSlope = false;
          return true;
        }
        // 否则始终贴合坡面：穿透时推上去, 悬空时拉下来（下坡跟随）
        if (penetration != 0)
        {
          deltaVel.Y -= penetration;
          next = GetHitBox(Entity);
          next.Location += deltaVel;
          PreventCeilingPenetration(ref deltaVel, ref next, bounds, Entity);
        }
        comPhysic.SlopeCollision = true;
        comPhysic.CollisionBottom = true;
        comPhysic.IsOnSlope = true;
        comPhysic.SlopeNormal = GetSlopeNormal(slopeType);
        return true;
      }
      else if (slopeType == TileSolid.SlopeLeftDown || slopeType == TileSolid.SlopeRightDown)
      {
        // 天花板斜坡
        float penetration = slopeSurfaceY - next.Top;

        // 实体在坡面下方且正在向下移动 → 不贴合, 允许脱离
        if (penetration <= 0 && deltaVel.Y >= 0)
        {
          comPhysic.IsOnSlope = false;
          return true;
        }

        // 始终贴合坡面
        if (penetration != 0)
        {
          deltaVel.Y += penetration;
          next = bounds;
          next.Location += deltaVel;
        }
        comPhysic.SlopeCollision = true;
        comPhysic.CollisionTop = true;
        comPhysic.IsOnSlope = true;
        comPhysic.SlopeNormal = GetSlopeNormal(slopeType);
        return true;
      }

      return false;
    }

    /// <summary>
    /// 获取斜坡表面在实体水平位置处的 Y 坐标.
    /// </summary>
    private float GetSlopeSurfaceY(TileSolid slopeType, RectangleF target, RectangleF bounds)
    {
      float tileLeft = target.Left;
      float tileRight = target.Right;
      float tileTop = target.Top;
      float tileBottom = target.Bottom;

      switch (slopeType)
      {
        case TileSolid.SlopeLeftUp:
          // '/' 左下→右上: y = tileBottom - (x - tileLeft)
          return tileBottom - (bounds.Right - tileLeft);

        case TileSolid.SlopeRightUp:
          // '\' 右下→左上: y = tileBottom - (tileRight - x)
          return tileBottom - (tileRight - bounds.Left);

        case TileSolid.SlopeLeftDown:
          // 天花板 '\' 左上→右下: y = tileTop + (x - tileLeft)
          return tileTop + (bounds.Right - tileLeft);

        case TileSolid.SlopeRightDown:
          // 天花板 '/' 右上→左下: y = tileTop + (tileRight - x)
          return tileTop + (tileRight - bounds.Left);

        default:
          return 0;
      }
    }

    /// <summary>
    /// 判定实体是否从斜坡的实心侧试图通过.
    /// 实心侧即斜坡三角形填充的一侧（地面斜坡为线下方, 天花板斜坡为线上方）.
    /// </summary>
    private bool IsOnSolidSideOfSlope(TileSolid slopeType, RectangleF target,
      RectangleF bounds, RectangleF previousBounds, Vector2 deltaVel)
    {
      float tileLeft = target.Left;
      float tileRight = target.Right;
      float tileTop = target.Top;
      float tileBottom = target.Bottom;

      switch (slopeType)
      {
        case TileSolid.SlopeLeftUp:
          {
            // '/' 实心侧在左下三角（线下方）
            // 判断前一刻实体底部是否在实心侧, 即线下方
            float slopeYAtPrevBottom = tileBottom - (previousBounds.Right - tileLeft);
            // 前一刻底部在实心侧（线下方）→ 从实心侧来
            if (previousBounds.Bottom > slopeYAtPrevBottom)
              return true;
            // 当前底部深陷实心侧 → 实心侧
            float slopeYAtBoundsBottom = tileBottom - (bounds.Right - tileLeft);
            if (bounds.Bottom > slopeYAtBoundsBottom)
              return true;
            return false;
          }

        case TileSolid.SlopeRightUp:
          {
            // '\' 实心侧在右下三角（线下方）
            float slopeYAtPrevBottom = tileBottom - (tileRight - previousBounds.Left);
            if (previousBounds.Bottom > slopeYAtPrevBottom)
              return true;
            float slopeYAtBoundsBottom = tileBottom - (tileRight - bounds.Left);
            if (bounds.Bottom > slopeYAtBoundsBottom)
              return true;
            return false;
          }

        case TileSolid.SlopeLeftDown:
          {
            // 天花板 '\' 实心侧在左上三角（线上方）
            float slopeYAtPrevTop = tileTop + (previousBounds.Right - tileLeft);
            if (previousBounds.Top < slopeYAtPrevTop)
              return true;
            float slopeYAtBoundsTop = tileTop + (bounds.Right - tileLeft);
            if (bounds.Top < slopeYAtBoundsTop)
              return true;
            return false;
          }

        case TileSolid.SlopeRightDown:
          {
            // 天花板 '/' 实心侧在右上三角（线上方）
            float slopeYAtPrevTop = tileTop + (tileRight - previousBounds.Left);
            if (previousBounds.Top < slopeYAtPrevTop)
              return true;
            float slopeYAtBoundsTop = tileTop + (tileRight - bounds.Left);
            if (bounds.Top < slopeYAtBoundsTop)
              return true;
            return false;
          }

        default:
          return false;
      }
    }

    /// <summary>
    /// 判定实体上一帧位置是否处于斜坡的实心侧.
    /// <br>仅使用 previousBounds 判定, 不参考当前帧 bounds.
    /// 用于在非首帧接触时区分"被直接放入体内"与"合法骑坡".</br>
    /// </summary>
    private bool WasPreviousInSolidSideOfSlope(TileSolid slopeType, RectangleF target, RectangleF previousBounds)
    {
      float tileLeft = target.Left;
      float tileRight = target.Right;
      float tileTop = target.Top;
      float tileBottom = target.Bottom;

      switch (slopeType)
      {
        case TileSolid.SlopeLeftUp:
          {
            // '/' 实心侧在线下方
            float slopeYAtPrevBottom = tileBottom - (previousBounds.Right - tileLeft);
            return previousBounds.Bottom > slopeYAtPrevBottom;
          }

        case TileSolid.SlopeRightUp:
          {
            // '\\' 实心侧在线下方
            float slopeYAtPrevBottom = tileBottom - (tileRight - previousBounds.Left);
            return previousBounds.Bottom > slopeYAtPrevBottom;
          }

        case TileSolid.SlopeLeftDown:
          {
            // 天花板 '\\' 实心侧在线上方
            float slopeYAtPrevTop = tileTop + (previousBounds.Right - tileLeft);
            return previousBounds.Top < slopeYAtPrevTop;
          }

        case TileSolid.SlopeRightDown:
          {
            // 天花板 '/' 实心侧在线上方
            float slopeYAtPrevTop = tileTop + (tileRight - previousBounds.Left);
            return previousBounds.Top < slopeYAtPrevTop;
          }

        default:
          return false;
      }
    }

    /// <summary>
    /// 判定实体是否能从斜坡底部开始上坡.
    /// 首帧接触时：实体必须处于斜坡起点（底端）且沿正确方向移动, 否则无法上坡.
    /// 如果实体正在下落（deltaVel.Y > 0）则允许落在斜面上.
    /// </summary>
    private bool CanStartClimbingSlope(TileSolid slopeType, RectangleF target,
      RectangleF bounds, RectangleF previousBounds, Vector2 deltaVel)
    {
      float tileLeft = target.Left;
      float tileRight = target.Right;
      float tileTop = target.Top;
      float tileBottom = target.Bottom;

      const float startThreshold = 0;

      switch (slopeType)
      {
        case TileSolid.SlopeLeftUp:
          {
            // '/' 起点在左下角 (tileLeft, tileBottom)
            // 下落中 → 允许落在斜面上
            if (deltaVel.Y > 0)
              return true;
            return previousBounds.Right <= tileLeft + startThreshold;
          }

        case TileSolid.SlopeRightUp:
          {
            // '\' 起点在右下角 (tileRight, tileBottom)
            if (deltaVel.Y > 0)
              return true;
            float entryX = previousBounds.Left;
            return entryX >= tileRight - startThreshold;
          }

        case TileSolid.SlopeLeftDown:
          {
            // 天花板 '\' 起点在左上角
            if (deltaVel.Y < 0)
              return true;
            float entryX = previousBounds.Right;
            return entryX <= tileLeft + startThreshold;
          }

        case TileSolid.SlopeRightDown:
          {
            // 天花板 '/' 起点在右上角
            if (deltaVel.Y < 0)
              return true;
            float entryX = previousBounds.Left;
            return entryX >= tileRight - startThreshold;
          }

        default:
          return false;
      }
    }

    /// <summary>
    /// 防头顶穿透：贴合地面坡面后, 检查并修正头部是否嵌入上方物块.
    /// 上坡时 deltaVel.Y 被斜坡调整后, next.Top 可能落入 topTile 扫描范围之外的 tile 行.
    /// </summary>
    private void PreventCeilingPenetration(ref Vector2 deltaVel, ref RectangleF next, RectangleF bounds, Entity Entity)
    {
      int headTopTile = (int)Math.Floor((float)next.Top / Tile.Context.TileSize.Y);
      int headLeftTile = (int)Math.Floor((float)next.Left / Tile.Context.TileSize.X);
      int headRightTile = (int)Math.Floor(((float)next.Right - 0.001f) / Tile.Context.TileSize.X);

      for (int rx = headLeftTile; rx <= headRightTile; rx++)
      {
        ref TileInfo roofInfo = ref Tile[rx, headTopTile, comPhysic.Layer];
        if (roofInfo.IsNull)
          continue;

        bool isCollidable = roofInfo.Collision == TileSolid.Sturdy ||
                            roofInfo.Collision == TileSolid.SlopeLeftUp ||
                            roofInfo.Collision == TileSolid.SlopeRightUp ||
                            roofInfo.Collision == TileSolid.SlopeLeftDown ||
                            roofInfo.Collision == TileSolid.SlopeRightDown ||
                            roofInfo.Loading;
        if (!isCollidable)
          continue;

        RectangleF roofTarget = GetTileBounds(ref roofInfo);
        if (!next.Intersects(roofTarget))
          continue;

        if (roofInfo.Collision == TileSolid.SlopeLeftDown || roofInfo.Collision == TileSolid.SlopeRightDown)
        {
          // 天花板斜坡：实心侧在线之上（Y更小）
          float roofY = GetSlopeSurfaceY(roofInfo.Collision, roofTarget, next);
          if (next.Top >= roofY)
            continue; // 头部在线之下 → 空侧 → 未穿透
          float pen = roofY - next.Top; // >0：头部在实心侧
          if (pen > 0)
          {
            deltaVel.Y += pen;
            next = GetHitBox(Entity);
            next.Location += deltaVel;
            comPhysic.CollisionTop = true;
            break;
          }
        }
        else
        {
          float roofY = roofTarget.Bottom;
          float pen = roofY - next.Top;
          if (pen > 0)
          {
            deltaVel.Y += pen;
            next = bounds;
            next.Location += deltaVel;
            comPhysic.CollisionTop = true;
            break;
          }
        }
      }
    }

    /// <summary>
    /// 获取斜坡的法线方向.
    /// </summary>
    private Vector2 GetSlopeNormal(TileSolid slopeType)
    {
      switch (slopeType)
      {
        case TileSolid.SlopeLeftUp:
          return Vector2.Normalize(new Vector2(1, 1));    // 法线指向左上
        case TileSolid.SlopeRightUp:
          return Vector2.Normalize(new Vector2(-1, 1));   // 法线指向右上
        case TileSolid.SlopeLeftDown:
          return Vector2.Normalize(new Vector2(-1, -1));  // 法线指向左下
        case TileSolid.SlopeRightDown:
          return Vector2.Normalize(new Vector2(1, -1));   // 法线指向右下
        default:
          return Vector2.UnitY;
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

    public RectangleF GetTileBounds(ref TileInfo info)
    {
      return new RectangleF(info.GetWCoord2().ToVector2() * Tile.Context.TileSizeF, Tile.Context.TileSizeF);
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