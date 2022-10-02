using System;
using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51.Disasters;

public class AnvilDisaster : Disaster
{
    public const int AnvilDamage = 10;
    public const float AnvilSize = 16f;
    public const int AnvilCount = 20;

    private readonly List<Sprite> shadowSprites = new();
    private bool haveAnvilsSpawned;
    private bool primed = true;
    private float scale = 2f;

    public AnvilDisaster(Random random, TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem) : base("Falling Anvils", false)
    {
        float mapWidth = tileMap.Width * tileMap.TileSize;
        float mapHeight = tileMap.Height * tileMap.TileSize;
        float maxX = mapWidth - AnvilSize;
        float maxY = mapHeight - AnvilSize;

        for (var i = 0; i < AnvilCount; i++)
        {
            var position = new Vector2(random.NextSingle() * maxX, random.NextSingle() * maxY);
            shadowSprites.Add(new Sprite(atlas, position, 0f, AnvilSize, AnvilSize, 1f, 2, 20));
            particleSystem.AddParticle(atlas, shadowSprites[i].Center, ParticleType.Spawn);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, TextureAtlas atlas, Camera camera)
    {
        bool shouldSpawnAnvils = !haveAnvilsSpawned && !primed;

        foreach (Sprite shadowSprite in shadowSprites)
        {
            shadowSprite.Scale = scale;
            shadowSprite.Draw(spriteBatch, atlas, camera);

            if (shouldSpawnAnvils)
                sprites.Add(new Sprite(atlas, shadowSprite.Position, 0f, AnvilSize, AnvilSize, 1f, 0, 20));
        }

        if (shouldSpawnAnvils)
        {
            shadowSprites.Clear();
            haveAnvilsSpawned = true;
        }

        base.Draw(spriteBatch, atlas, camera);
    }

    public override void Update(Random random, Player player, TileMap tileMap, List<Sprite> blockingSprites,
        float deltaTime)
    {
        Vector2 playerPosition = player.Sprite.Center;
        scale -= deltaTime;

        if (scale < 0.25f && primed)
        {
            foreach (Sprite shadowSprite in shadowSprites)
                if (shadowSprite.Center.Distance(playerPosition) < (AnvilSize + player.Sprite.Size) * 0.5f)
                    // Hit player with anvil once.
                    player.TakeDamage(AnvilDamage);

            primed = false;
        }
    }
}