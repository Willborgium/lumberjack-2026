using Microsoft.Xna.Framework;

namespace Lumberjack;

public interface IActionEmitter<TAction> : IUpdatable
{
    bool IsActive(TAction action);
}
