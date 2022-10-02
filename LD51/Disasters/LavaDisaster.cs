using System;
using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework;

namespace LD51.Disasters;

public class LavaDisaster : Disaster
{
    public const int LavaDamage = 1;
    public const float LavaSpreadTime = 1f;
    public const int LavaMaxCount = 36;
    private readonly int dirX;
    private readonly int dirY;

    private readonly List<StoredTile> lavaTiles = new();
    private float lavaSpreadTimer = LavaSpreadTime;

    public LavaDisaster(Random random, TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem) : base("Lava", false)
    {
        int startX = random.Next(tileMap.Width);
        int startY = random.Next(tileMap.Height);
        dirX = random.Next(2) * 2 - 1;
        dirY = random.Next(2) * 2 - 1;

        particleSystem.AddParticle(atlas,
            new Vector2((startX + 0.5f) * tileMap.TileSize, (startY + 0.5f) * tileMap.TileSize), ParticleType.Spawn);
        lavaTiles.Add(new StoredTile(startX, startY, tileMap.GetTile(startX, startY)));
        tileMap.SetTile(startX, startY, TileMap.Tile.Lava);
    }

    public override void Update(Random random, Player player, TileMap tileMap, List<Sprite> blockingSprites,
        float deltaTime)
    {
        lavaSpreadTimer -= deltaTime;

        int lavaTileCount = lavaTiles.Count;
        if (lavaSpreadTimer < 0f && lavaTileCount < LavaMaxCount)
        {
            lavaSpreadTimer = LavaSpreadTime;

            for (var i = 0; i < lavaTileCount; i++)
            {
                StoredTile storedTile = lavaTiles[i];

                var neighborXPos = new Point(storedTile.X + dirX, storedTile.Y);
                var neighborYPos = new Point(storedTile.X, storedTile.Y + dirY);

                SpreadLava(tileMap, neighborXPos);
                SpreadLava(tileMap, neighborYPos);
            }
        }
    }

    private void SpreadLava(TileMap tileMap, Point pos)
    {
        if (lavaTiles.Count >= LavaMaxCount) return;

        TileMap.Tile neighborTile = tileMap.GetTile(pos.X, pos.Y);
        if (neighborTile != TileMap.Tile.Air && neighborTile != TileMap.Tile.Lava)
        {
            lavaTiles.Add(new StoredTile(pos.X, pos.Y, neighborTile));
            tileMap.SetTile(pos.X, pos.Y, TileMap.Tile.Lava);
        }
    }

    public override void Vanish(TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem)
    {
        foreach (StoredTile storedTile in lavaTiles)
        {
            particleSystem.AddParticle(atlas,
                new Vector2((storedTile.X + 0.5f) * tileMap.TileSize, (storedTile.Y + 0.5f) * tileMap.TileSize), ParticleType.DeSpawn);
            tileMap.SetTile(storedTile.X, storedTile.Y, storedTile.Tile);
        }

        base.Vanish(atlas, tileMap, particleSystem);
    }
}