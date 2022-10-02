namespace LD51;

public struct StoredTile
{
    public int X;
    public int Y;
    public TileMap.Tile Tile;

    public StoredTile(int x, int y, TileMap.Tile tile)
    {
        X = x;
        Y = y;
        Tile = tile;
    }
}