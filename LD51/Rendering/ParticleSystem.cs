using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51.Rendering;

public enum ParticleType
{
    Spawn = 6,
    DeSpawn = 0,
}

public class ParticleSystem
{
    public const float MinScale = 0.05f;
    public const float RotationSpeed = 4f;
    public const int TexY = 22;
    public const int TexW = 3;
    public const int TexH = 3;

    private readonly List<Sprite> sprites = new();

    public void AddParticle(TextureAtlas atlas, Vector2 position, ParticleType type)
    {
        float width = atlas.TileSize * TexW;
        float height = atlas.TileSize * TexH;
        var centerOffset = new Vector2(width * 0.5f, height * 0.5f);
        sprites.Add(new Sprite(atlas, position - centerOffset, 0f, width, height, 1f, (int)type, TexY));
    }

    public void Update(float deltaTime)
    {
        for (int i = sprites.Count - 1; i >= 0; i--)
        {
            if (sprites[i].TexX == (int)ParticleType.DeSpawn)
            {
                sprites[i].Rotation += deltaTime * RotationSpeed;
            }

            sprites[i].Scale -= deltaTime;

            if (sprites[i].Scale < MinScale) sprites.RemoveAt(i);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera, TextureAtlas atlas)
    {
        for (var i = 0; i < sprites.Count; i++) sprites[i].Draw(spriteBatch, atlas, camera);
    }
}