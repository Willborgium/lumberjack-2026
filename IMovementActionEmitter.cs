using Microsoft.Xna.Framework;

namespace Lumberjack;

public interface IMovementActionEmitter : IUpdatable
{
    bool IsActive(MovementAction action);
}
