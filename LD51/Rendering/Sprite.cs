using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51.Rendering;

public class Sprite
{
    public readonly float Height;

    public readonly int TexH;
    public readonly int TexW;
    public readonly int TexX;
    public readonly int TexY;
    public readonly float Width;

    public Vector2 Position;
    public float Rotation;
    public float Scale;

    public Sprite(TextureAtlas atlas, Vector2 position, float rotation, float width, float height, float scale,
        int texX, int texY)
    {
        Position = position;
        Rotation = rotation;
        Width = width;
        Height = height;
        Scale = scale;
        TexW = atlas.GetAtlasSize(Width);
        TexH = atlas.GetAtlasSize(Height);
        TexX = texX;
        TexY = texY;
    }

    public float Size => (Width + Height) * 0.5f;

    public Vector2 Center => Position + new Vector2(Width, Height) / 2f;

    public void Draw(SpriteBatch spriteBatch, TextureAtlas atlas, Camera camera)
    {
        atlas.Draw(spriteBatch, camera, Position, TexX, TexY, TexW, TexH, GameColors.White, Scale, Rotation);
    }

    public void LookAt(Vector2 target)
    {
        float centerY = Position.Y + Width * 0.5f;
        float centerX = Position.X + Height * 0.5f;
        Rotation = MathF.Atan2(target.Y - centerY, target.X - centerX);
    }

    public MoveResult Move(TileMap tileMap, float dirX, float dirY, float speed, float deltaTime,
        List<Sprite> stopAt = null)
    {
        var movement = new Vector2(dirX, dirY);
        if (movement != Vector2.Zero) movement.Normalize();

        var postMoveX = new Vector2(Position.X + speed * deltaTime * movement.X, Position.Y);
        var postMoveY = new Vector2(Position.X, Position.Y + speed * deltaTime * movement.Y);
        var centerOffset = new Vector2(Width / 2f, Height / 2f);
        bool canMoveX = true, canMoveY = true;

        if (stopAt is not null)
            foreach (Sprite otherSprite in stopAt)
            {
                if (otherSprite == this) continue;

                Vector2 otherZombiePos = otherSprite.Center;

                float minXDist = (otherSprite.Width + Width) * 0.5f;
                float minYDist = (otherSprite.Height + Height) * 0.5f;

                if ((postMoveX + centerOffset).Distance(otherZombiePos) < minXDist) canMoveX = false;

                if ((postMoveY + centerOffset).Distance(otherZombiePos) < minYDist) canMoveY = false;

                if (!canMoveX && !canMoveY) break;
            }

        if (canMoveX) Position.X = postMoveX.X;
        if (canMoveY) Position.Y = postMoveY.Y;

        float maxX = tileMap.Width * tileMap.TileSize - Width;
        float limitedX = Position.X;
        float maxY = tileMap.Height * tileMap.TileSize - Height;
        float limitedY = Position.Y;

        if (limitedX < 0f)
        {
            limitedX = 0f;
            canMoveX = false;
        }

        if (limitedX > maxX)
        {
            limitedX = maxX;
            canMoveX = false;
        }

        if (limitedY < 0f)
        {
            limitedY = 0f;
            canMoveY = false;
        }

        if (limitedY > maxY)
        {
            limitedY = maxY;
            canMoveY = false;
        }

        Position = new Vector2(limitedX, limitedY);

        return new MoveResult(!canMoveX, !canMoveY);
    }

    public struct MoveResult
    {
        public bool XCollision;
        public bool YCollision;

        public MoveResult(bool xCollision, bool yCollision)
        {
            XCollision = xCollision;
            YCollision = yCollision;
        }
    }
}