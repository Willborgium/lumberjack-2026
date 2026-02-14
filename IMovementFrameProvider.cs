using Microsoft.Xna.Framework;

namespace Lumberjack;

public interface IMovementFrameProvider
{
    Vector3 Forward { get; }
    Vector3 Right { get; }
}
