using System;
using Microsoft.Xna.Framework;

namespace Lumberjack;

public class CameraDebugger(ICamera camera, Action<string, string> SetDebugStat) : IUpdatable
{
    public void Update(GameTime gameTime)
    {
        SetDebugStat($"Camera Position", $"{camera.Position.X:F2}, {camera.Position.Y:F2}, {camera.Position.Z:F2}");
        SetDebugStat($"Camera Target", $"{camera.Target.X:F2}, {camera.Target.Y:F2}, {camera.Target.Z:F2}");
    }
}
