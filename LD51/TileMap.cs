using System;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class TileMap
{
    public enum Tile
    {
        Air,
        Dirt,
        Grass,
        Water,
        Lava
    }

    // Increase the size slightly to account for scaling/rounding.
    public const float SizePadding = 1.002f;

    public const float TerrainPeriod = 0.2f;
    public const int TileTexW = 5;
    private readonly Camera preCamera;

    private readonly RenderTarget2D renderTarget;
    private readonly Tile[] tiles;

    public readonly int TileSize;

    public TileMap(GraphicsDevice graphicsDevice, int width, int height, int tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;

        tiles = new Tile[width * height];

        int renderWidth = Width * TileSize;
        int renderHeight = Height * TileSize;
        renderTarget = new RenderTarget2D(graphicsDevice, renderWidth, renderHeight, false,
            graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        preCamera = new Camera(renderWidth, renderHeight, false);
    }

    public int Width { get; }
    public int Height { get; }

    public void Generate(Random random)
    {
        float xOffset = random.NextSingle() * MathF.PI * 2f;
        float yOffset = random.NextSingle() * MathF.PI * 2f;

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            float genXValue = MathF.Sin(x * TerrainPeriod + xOffset);
            float genYValue = MathF.Sin(y * TerrainPeriod + yOffset);
            float genValue = (genXValue + genYValue + 2 + random.NextSingle()) * 0.25f;

            // if (MathF.Min(random.NextSingle(), 0.75f) > lakePos.Distance(pos) / lakeRadius)
            // {
            //     SetTile(x, y, Tile.Water);
            //     continue;
            // }
            //
            // int type = random.Next(2);
            // var tile = (Tile)(type + 1);

            var tile = Tile.Water;

            if (genValue < 0.33f)
                tile = Tile.Grass;
            else if (genValue < 0.66f) tile = Tile.Dirt;

            SetTile(x, y, tile);
        }
    }

    public int PosToTilePos(float p)
    {
        return (int)MathF.Floor(p / TileSize);
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        spriteBatch.Draw(renderTarget, -camera.Position * camera.Scale, null, GameColors.White, 0f, Vector2.Zero,
            camera.Scale * SizePadding, SpriteEffects.None, 0f);
    }

    public void PreDraw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, TextureAtlas atlas)
    {
        graphicsDevice.SetRenderTarget(renderTarget);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawTiles(spriteBatch, atlas, preCamera);
        spriteBatch.End();

        graphicsDevice.SetRenderTarget(null);
    }

    private void DrawTiles(SpriteBatch spriteBatch, TextureAtlas atlas, Camera camera)
    {
        int minX = Math.Max(PosToTilePos(camera.Position.X), 0);
        int maxX = Math.Min(PosToTilePos(camera.Position.X + camera.ViewWidth) + 2, Width);
        int minY = Math.Max(PosToTilePos(camera.Position.Y), 0);
        int maxY = Math.Min(PosToTilePos(camera.Position.Y + camera.ViewHeight) + 2, Height);

        for (int x = minX; x < maxX; x++)
        for (int y = minY; y < maxY; y++)
        {
            Tile tile = GetTile(x, y);

            if (tile == Tile.Air) continue;

            int texX = (int)tile * TileTexW;
            const int texY = 0;

            for (var seg = 0; seg < 4; seg++)
            {
                // Pointers to sub-tile in texture atlas.
                var pX = 1;
                var pY = 1;

                int offX = seg % 2;
                int offY = seg >> 1;

                int xDir = offX * 2 - 1;
                int yDir = offY * 2 - 1;

                bool isXNeighborSame = CompareTile(x + xDir, y, tile);
                bool isYNeighborSame = CompareTile(x, y + yDir, tile);
                bool isXyNeighborSame = CompareTile(x + xDir, y + yDir, tile);

                if (!isXNeighborSame) pX += xDir;
                if (!isYNeighborSame) pY += yDir;

                if (isXNeighborSame && isYNeighborSame && !isXyNeighborSame)
                {
                    pX = 4 - offX;
                    pY = 1 - offY;
                }

                atlas.Draw(spriteBatch, camera,
                    new Vector2(x * TileSize + offX * atlas.TileSize, y * TileSize + offY * atlas.TileSize),
                    texX + pX, texY + pY, 1, 1, GameColors.White);
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return Tile.Air;

        return tiles[x + y * Width];
    }

    public void SetTile(int x, int y, Tile tile)
    {
        tiles[x + y * Width] = tile;
    }

    public bool CompareTile(int x, int y, Tile tile)
    {
        return GetTile(x, y) == tile;
    }
}