using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51.Rendering;

public class TextRenderer
{
    public static readonly char[] characters =
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
        'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '?', '!', ',', '$', '<', '>', '[',
        ']', ' '
    };

    public readonly int AtlasWidth;

    public readonly int TexX;
    public readonly int TexY;

    public TextRenderer(int texX, int texY, int atlasWidth)
    {
        TexX = texX;
        TexY = texY;
        AtlasWidth = atlasWidth;
    }

    public Vector2 GetTextAlignment(TextureAtlas atlas, Vector2 position, string text, bool centeredX = false,
        bool centeredY = false)
    {
        float alignmentX = centeredX ? atlas.TileSize * text.Length * 0.5f : 0f;
        float alignmentY = centeredY ? atlas.TileSize * 0.5f : 0f;
        return new Vector2(alignmentX, alignmentY);
    }

    public Rectangle GetTextRect(TextureAtlas atlas, Vector2 position, string text, bool centeredX = false,
        bool centeredY = false)
    {
        Vector2 alignment = GetTextAlignment(atlas, position, text, centeredX, centeredY);

        return new Rectangle((position - alignment).ToPoint(),
            new Point(text.Length * atlas.TileSize, atlas.TileSize));
    }

    public void DrawText(SpriteBatch spriteBatch, Camera camera, TextureAtlas atlas, Vector2 position, string text,
        Color color, bool centeredX = false, bool centeredY = false)
    {
        string lowerText = text.ToLower();
        Vector2 alignment = GetTextAlignment(atlas, position, text, centeredX, centeredY);

        var i = 0;
        foreach (char c in lowerText)
        {
            int ci = Array.IndexOf(characters, c);
            Vector2 offset = new Vector2(i * atlas.TileSize, 0f) - alignment;
            int texXOff = ci % AtlasWidth;
            int texYOff = ci / AtlasWidth;
            atlas.Draw(spriteBatch, camera, position + offset, TexX + texXOff, TexY + texYOff, 1, 1, color);

            i++;
        }
    }

    public void DrawTextWithShadow(SpriteBatch spriteBatch, Camera camera, TextureAtlas atlas, Vector2 position,
        string text, Color color, Color shadowColor, bool centeredX = false, bool centeredY = false)
    {
        DrawText(spriteBatch, camera, atlas, position + Vector2.One, text, shadowColor, centeredX, centeredY);
        DrawText(spriteBatch, camera, atlas, position, text, color, centeredX, centeredY);
    }
}