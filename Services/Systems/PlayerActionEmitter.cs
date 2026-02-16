using Microsoft.Xna.Framework;

namespace Lumberjack.Services.Systems;

public class PlayerActionEmitter(InputService input) : IActionEmitter<PlayerAction>
{
    public bool IsActive(PlayerAction action)
    {
        return action switch
        {
            PlayerAction.Interact => input.IsAction(InputAction.Interact),
            _ => false,
        };
    }

    public void Update(GameTime gameTime)
    {
        // reads directly from InputService state
    }
}

public class PlayerActionHandler(IActionEmitter<PlayerAction> emitter, IPlayerState playerState) : IUpdatable
{
    public void Update(GameTime gameTime)
    {
        if (emitter.IsActive(PlayerAction.Interact))
        {
            playerState.WoodCount += 1;
        }
    }
}