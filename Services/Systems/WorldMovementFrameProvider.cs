using Microsoft.Xna.Framework;

namespace Lumberjack.Services.Systems;

public class WorldMovementFrameProvider : IMovementFrameProvider
{
    public Vector3 Forward => Vector3.Forward;
    public Vector3 Right => Vector3.Right;
}
