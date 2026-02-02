using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public interface ICamera
{
    Matrix GetViewMatrix();
}

public class Camera : ICamera, IUpdatable
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; }
    public float MoveSpeed { get; set; } = 4f;
    public float RunMultiplier { get; set; } = 2.5f;
    public float MouseSensitivity { get; set; } = .005f;

    private readonly InputService _input;
    private float _yaw;
    private float _pitch;
    private Viewport _viewport;

    public Camera(Vector3 position, Vector3 target, InputService input)
    {
        _input = input;

        Position = position;
        Target = target;

        var forward = Vector3.Normalize(target - position);
        if (float.IsNaN(forward.X)) forward = Vector3.Forward;

        _yaw = MathF.Atan2(forward.X, forward.Z);
        _pitch = MathF.Asin(MathHelper.Clamp(forward.Y, -1f, 1f));
    }

    public void SetViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Update(GameTime gameTime)
    {
        var kb = _input.CurrentKeyboard;
        var mouseDelta = _input.MouseDelta;

        var deltaX = mouseDelta.X;
        var deltaY = mouseDelta.Y;

        _yaw -= deltaX * MouseSensitivity;
        _pitch -= deltaY * MouseSensitivity;
        _pitch = MathHelper.Clamp(_pitch, -MathF.PI * 0.5f + 0.001f, MathF.PI * 0.5f - 0.001f);

        // Ground-relative movement (yaw only) for FPS-style controls
        Vector3 forwardYaw = new Vector3(MathF.Sin(_yaw), 0f, MathF.Cos(_yaw));
        Vector3 rightYaw = Vector3.Normalize(Vector3.Cross(Vector3.Up, forwardYaw));

        Vector3 move = Vector3.Zero;
        if (kb.IsKeyDown(Keys.W)) move += forwardYaw;
        if (kb.IsKeyDown(Keys.S)) move -= forwardYaw;
        if (kb.IsKeyDown(Keys.A)) move += rightYaw;
        if (kb.IsKeyDown(Keys.D)) move -= rightYaw;

        if (move != Vector3.Zero)
        {
            move.Normalize();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float speed = MoveSpeed * (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift) ? RunMultiplier : 1f);
            Position += move * speed * dt;
        }

        Vector3 forward = new Vector3(MathF.Sin(_yaw) * MathF.Cos(_pitch), MathF.Sin(_pitch), MathF.Cos(_yaw) * MathF.Cos(_pitch));
        Target = Position + forward;

        // recenter when close to window edges to allow indefinite turning
        var mouse = _input.CurrentMouse;
        if (_viewport.Width > 0 && _viewport.Height > 0)
        {
            const int edgeThreshold = 8;
            if (mouse.X < edgeThreshold || mouse.X > _viewport.Width - edgeThreshold ||
                mouse.Y < edgeThreshold || mouse.Y > _viewport.Height - edgeThreshold)
            {
                int centerX = _viewport.Width / 2;
                int centerY = _viewport.Height / 2;
                _input.WarpMouse(new Point(centerX, centerY));
            }
        }
    }

    public Matrix GetViewMatrix() => Matrix.CreateLookAt(Position, Target, Vector3.Up);
}
