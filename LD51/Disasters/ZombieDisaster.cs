using System;
using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework;

namespace LD51.Disasters;

public class ZombieDisaster : Disaster
{
    public const float ZombieAttackCooldown = 0.5f;
    public const int ZombieDamage = 1;
    public const int ZombieCount = 10;
    public const float ZombieSize = 16f;
    public const float ZombieSpeed = 50f;

    private readonly List<float> attackCooldowns = new();
    private readonly List<Vector2> accelerations = new();

    public ZombieDisaster(Random random, TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem) : base("Zombies", true)
    {
        float mapWidth = tileMap.Width * tileMap.TileSize;
        float mapHeight = tileMap.Height * tileMap.TileSize;
        float maxX = mapWidth - ZombieSize;
        float maxY = mapHeight - ZombieSize;

        for (var i = 0; i < ZombieCount; i++)
        {
            var position = new Vector2(random.NextSingle() * maxX, random.NextSingle() * maxY);
            sprites.Add(new Sprite(atlas, position, 0f, ZombieSize, ZombieSize, 1f, 0, 28));
            particleSystem.AddParticle(atlas, sprites[i].Center, ParticleType.Spawn);
            attackCooldowns.Add(0f);
            accelerations.Add(Vector2.Zero);
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

            sprite.LookAt(playerPosition);

            if (sprite.Center.Distance(playerPosition) > 16f)
            {
                float dirX = playerPosition.X - sprite.Center.X;
                float dirY = playerPosition.Y - sprite.Center.Y;
                Vector2 acceleration = accelerations[i];
                acceleration.X = MathF.Min(acceleration.X + deltaTime, 1f);
                acceleration.Y = MathF.Min(acceleration.Y + deltaTime, 1f);
                
                Sprite.MoveResult result = sprite.Move(tileMap, dirX * acceleration.X, dirY * acceleration.Y, ZombieSpeed, deltaTime,
                    blockingSprites);

                if (result.XCollision)
                {
                    acceleration.X = 0f;
                }
                
                if (result.YCollision)
                {
                    acceleration.Y = 0f;
                }

                accelerations[i] = acceleration;
            }
            else if (attackCooldowns[i] < 0f)
            {
                // Player is in attack range and attack cooldown is done.
                player.TakeDamage(ZombieDamage);
                attackCooldowns[i] = ZombieAttackCooldown;
            }
        }
    }
}