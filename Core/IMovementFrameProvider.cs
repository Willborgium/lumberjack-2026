using Microsoft.Xna.Framework;

namespace Lumberjack.Core;

public interface IMovementFrameProvider
{
    Vector3 Forward { get; }
    Vector3 Right { get; }
}
