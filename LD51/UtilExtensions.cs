using System;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public static class UtilExtensions
{
    public static float Distance(this Vector2 from, Vector2 to)
    {
        float xDist = to.X - from.X;
        float yDist = to.Y - from.Y;

        return MathF.Sqrt(xDist * xDist + yDist * yDist);
    }

    public static bool IsHovered(this Rectangle rect, Camera camera)
    {
        MouseState mouseState = Mouse.GetState();
        float mouseX = mouseState.X / camera.Scale;
        float mouseY = mouseState.Y / camera.Scale;

        return mouseX < rect.X + rect.Width && mouseX > rect.X && mouseY < rect.Y + rect.Height &&
               mouseY > rect.Y;
    }
}