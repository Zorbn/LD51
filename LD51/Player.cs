using LD51.Disasters;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class Player
{
    public const float MoveSpeed = 75f;
    public const float StepDamageCooldown = 0.25f;
    public readonly Sprite Sprite;

    private float stepDamageTimer = StepDamageCooldown;

    public Player(TextureAtlas atlas, Vector2 position, float rotation, float width, float height)
    {
        Sprite = new Sprite(atlas, position, rotation, width, height, 1f, 0, 30);
        Health = 20;
    }

    public int Health { get; private set; }

    public void Draw(SpriteBatch spriteBatch, TextureAtlas atlas, Camera camera)
    {
        Sprite.Draw(spriteBatch, atlas, camera);
    }

    public void Move(TileMap tileMap, int dirX, int dirY, float deltaTime)
    {
        Sprite.Move(tileMap, dirX, dirY, MoveSpeed, deltaTime);
    }

    public void Update(TileMap tileMap, float deltaTime)
    {
        stepDamageTimer -= deltaTime;

        if (stepDamageTimer < 0f)
        {
            stepDamageTimer = StepDamageCooldown;

            Vector2 playerPosition = Sprite.Center;
            int playerTileX = tileMap.PosToTilePos(playerPosition.X);
            int playerTileY = tileMap.PosToTilePos(playerPosition.Y);

            if (tileMap.GetTile(playerTileX, playerTileY) == TileMap.Tile.Lava) TakeDamage(LavaDisaster.LavaDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
}