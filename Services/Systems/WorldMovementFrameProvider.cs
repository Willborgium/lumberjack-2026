using Microsoft.Xna.Framework;

namespace Lumberjack;

public class WorldMovementFrameProvider : IMovementFrameProvider
{
    public Vector3 Forward => Vector3.Forward;
    public Vector3 Right => Vector3.Right;
}
