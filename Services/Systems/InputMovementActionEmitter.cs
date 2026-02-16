using Microsoft.Xna.Framework;

namespace Lumberjack;

public class InputMovementActionEmitter(InputService input) : IMovementActionEmitter
{
    public bool IsActive(MovementAction action)
    {
        return action switch
        {
            MovementAction.Forward => input.IsAction(InputAction.MoveForward),
            MovementAction.Backward => input.IsAction(InputAction.MoveBackward),
            MovementAction.Left => input.IsAction(InputAction.MoveLeft),
            MovementAction.Right => input.IsAction(InputAction.MoveRight),
            MovementAction.Run => input.IsAction(InputAction.Run),
            _ => false,
        };
    }

    public void Update(GameTime gameTime)
    {
        // reads directly from InputService state
    }
}
