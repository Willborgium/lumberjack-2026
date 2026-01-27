using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public class Camera : IUpdatable
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; }
    public float MoveSpeed { get; set; } = 4f;

    public Camera(Vector3 position, Vector3 target)
    {
        Position = position;
        Target = target;
    }

    public void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        Vector3 move = Vector3.Zero;

        if (kb.IsKeyDown(Keys.W)) move += Vector3.Forward;  // +Z
        if (kb.IsKeyDown(Keys.S)) move += Vector3.Backward; // -Z
        if (kb.IsKeyDown(Keys.A)) move += Vector3.Left;     // -X
        if (kb.IsKeyDown(Keys.D)) move += Vector3.Right;    // +X
        if (kb.IsKeyDown(Keys.Q)) move += Vector3.Up;       // +Y
        if (kb.IsKeyDown(Keys.E)) move += Vector3.Down;     // -Y

        if (move != Vector3.Zero)
        {
            move.Normalize();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift))
            {
                Target += move * MoveSpeed * dt;
            }
            else
            {
                Position += move * MoveSpeed * dt;
            }
        }
    }

    public Matrix GetViewMatrix() => Matrix.CreateLookAt(Position, Target, Vector3.Up);
}
