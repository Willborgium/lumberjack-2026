using System.Collections.Generic;

namespace Lumberjack.Services.Systems;

public static class DebugLog
{
    private static readonly List<string> _lines = new List<string>();
    public static IReadOnlyList<string> Lines => _lines;

    public static int MaxLines { get; set; } = 10;

    public static void Log(string message)
    {
        _lines.Add(message);
        while (_lines.Count > MaxLines)
        {
            _lines.RemoveAt(0);
        }
    }

    public static void Clear() => _lines.Clear();
}
