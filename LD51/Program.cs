using System;

namespace LD51;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new LdGame();
        game.Run();
    }
}