using System.Collections.Generic;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Input
{
    private readonly Dictionary<string, Rectangle> buttonRects;
    private KeyboardState previousState, currentState;

    public Input()
    {
        buttonRects = new Dictionary<string, Rectangle>();
        currentState = Keyboard.GetState();
        previousState = currentState;
    }

    public void Update()
    {
        previousState = currentState;
        currentState = Keyboard.GetState();
    }

    public void ResetButtons()
    {
        buttonRects.Clear();
    }

    public bool IsKeyDown(Keys key)
    {
        return currentState.IsKeyDown(key);
    }

    public bool WasKeyPressed(Keys key)
    {
        return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);
    }

    public void AddButtonRect(string name, Rectangle rectangle)
    {
        buttonRects.TryAdd(name, rectangle);
    }

    public bool WasButtonPressed(Camera camera, string name)
    {
        MouseState mouseState = Mouse.GetState();

        if (buttonRects.ContainsKey(name))
            return buttonRects[name].IsHovered(camera) && mouseState.LeftButton == ButtonState.Pressed;

        return false;
    }
}