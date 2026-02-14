using Microsoft.Xna.Framework;

namespace Lumberjack;

public class GroundMovementTranslator(IMovementActionEmitter emitter, IMovementFrameProvider frameProvider) : IUpdatable
{
    public float MoveSpeed { get; set; } = 4f;
    public float RunMultiplier { get; set; } = 2.5f;

    public Vector3 Translation { get; private set; }

    public void Update(GameTime gameTime)
    {
        var move = Vector3.Zero;

        if (emitter.IsActive(MovementAction.Forward)) move += frameProvider.Forward;
        if (emitter.IsActive(MovementAction.Backward)) move -= frameProvider.Forward;
        if (emitter.IsActive(MovementAction.Left)) move -= frameProvider.Right;
        if (emitter.IsActive(MovementAction.Right)) move += frameProvider.Right;

        if (move.LengthSquared() > 0.000001f)
        {
            move.Normalize();
        }

        var speed = MoveSpeed * (emitter.IsActive(MovementAction.Run) ? RunMultiplier : 1f);
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Translation = move * speed * dt;
    }
}
