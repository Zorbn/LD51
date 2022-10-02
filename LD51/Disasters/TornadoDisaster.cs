using System;
using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework;

namespace LD51.Disasters;

public class TornadoDisaster : Disaster
{
    public const float TornadoAttackCooldown = 0.25f;
    public const int TornadoDamage = 2;
    public const float TornadoSize = 24f;
    public const float TornadoSpinSpeed = 16f;
    public const float TornadoSpeed = 125f;
    public const int TornadoCount = 10;

    private readonly List<float> attackCooldowns = new();
    private readonly List<Vector2> directions = new();
    private readonly List<bool> hasDirection = new();

    public TornadoDisaster(Random random, TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem) : base("Tornadoes", false)
    {
        float mapWidth = tileMap.Width * tileMap.TileSize;
        float mapHeight = tileMap.Height * tileMap.TileSize;
        float maxX = mapWidth - TornadoSize;
        float maxY = mapHeight - TornadoSize;

        for (var i = 0; i < TornadoCount; i++)
        {
            var position = new Vector2(random.NextSingle() * maxX, random.NextSingle() * maxY);
            sprites.Add(new Sprite(atlas, position, 0f, TornadoSize, TornadoSize, 1f, 3, 22));
            particleSystem.AddParticle(atlas, sprites[i].Center, ParticleType.Spawn);
            attackCooldowns.Add(0f);
            directions.Add(Vector2.Zero);
            hasDirection.Add(false);
        }
    }

    public override void Update(Random random, Player player, TileMap tileMap, List<Sprite> blockingSprites,
        float deltaTime)
    {
        Vector2 playerPosition = player.Sprite.Center;

        for (var i = 0; i < sprites.Count; i++)
        {
            attackCooldowns[i] -= deltaTime;
            Sprite sprite = sprites[i];
            sprite.Rotation += TornadoSpinSpeed * deltaTime;

            if (!hasDirection[i])
            {
                float dirX = playerPosition.X - sprite.Center.X;
                float dirY = playerPosition.Y - sprite.Center.Y;
                directions[i] = new Vector2(dirX, dirY);
                hasDirection[i] = true;
            }

            Sprite.MoveResult moveResult =
                sprite.Move(tileMap, directions[i].X, directions[i].Y, TornadoSpeed, deltaTime);

            Vector2 newDirection = directions[i];

            if (moveResult.XCollision) newDirection.X *= -1f;

            if (moveResult.YCollision) newDirection.Y *= -1f;

            directions[i] = newDirection;

            if (sprite.Center.Distance(playerPosition) < (TornadoSize + player.Sprite.Size) * 0.5f &&
                attackCooldowns[i] < 0f)
            {
                // Player is in attack range and attack cooldown is done.
                player.TakeDamage(TornadoDamage);
                attackCooldowns[i] = TornadoAttackCooldown;
            }
        }
    }
}