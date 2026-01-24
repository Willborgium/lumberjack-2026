using System;

namespace Lumberjack;

static class Program
{
    [STAThread]
    static void Main()
    {
        using (var game = new Driver())
        {
            game.Run();
        }
    }
}
