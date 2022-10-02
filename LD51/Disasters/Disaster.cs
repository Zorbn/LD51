using System;
using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace LD51.Disasters;

public class Disaster
{
    public readonly bool AreSpritesBlocking;
    public readonly string Name;
    protected readonly List<Sprite> sprites = new();

    public Disaster(string name, bool areSpritesBlocking)
    {
        Name = name;
        AreSpritesBlocking = areSpritesBlocking;
    }

    public virtual void Update(Random random, Player player, TileMap tileMap, List<Sprite> blockingSprites,
        float deltaTime)
    {
    }

    public virtual void Draw(SpriteBatch spriteBatch, TextureAtlas atlas, Camera camera)
    {
        foreach (Sprite sprite in sprites) sprite.Draw(spriteBatch, atlas, camera);
    }

    public virtual void Vanish(TextureAtlas atlas, TileMap tileMap, ParticleSystem particleSystem)
    {
        foreach (Sprite sprite in sprites) particleSystem.AddParticle(atlas, sprite.Center, ParticleType.DeSpawn);

        sprites.Clear();
    }

    public virtual List<Sprite> GetBlockingSprites()
    {
        if (AreSpritesBlocking) return sprites;

        return new List<Sprite>();
    }
}