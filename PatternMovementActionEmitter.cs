using Microsoft.Xna.Framework;

namespace Lumberjack;

public class PatternMovementActionEmitter(float segmentDurationSeconds = 2f) : IMovementActionEmitter
{
    private float _timer;

    public bool IsActive(MovementAction action)
    {
        var phase = (int)(_timer / segmentDurationSeconds) % 4;

        return (phase, action) switch
        {
            (0, MovementAction.Right) => true,
            (1, MovementAction.Forward) => true,
            (2, MovementAction.Left) => true,
            (3, MovementAction.Backward) => true,
            _ => false,
        };
    }

    public void Update(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}
