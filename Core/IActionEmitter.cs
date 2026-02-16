using Microsoft.Xna.Framework;

namespace Lumberjack.Core;

public interface IActionEmitter<TAction> : IUpdatable
{
    bool IsActive(TAction action);
}
