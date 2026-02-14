using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public interface ICamera
{
    Matrix GetViewMatrix();
}

public class Camera : ICamera, IUpdatable, ITranslatable, IMovementFrameProvider
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; }
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
        var mouseDelta = _input.MouseDelta;

        var deltaX = mouseDelta.X;
        var deltaY = mouseDelta.Y;

        _yaw -= deltaX * MouseSensitivity;
        _pitch -= deltaY * MouseSensitivity;
        _pitch = MathHelper.Clamp(_pitch, -MathF.PI * 0.5f + 0.001f, MathF.PI * 0.5f - 0.001f);

        RefreshTarget();

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

    public void Translate(Vector3 delta)
    {
        Position += delta;
        RefreshTarget();
    }

    public Vector3 Forward
    {
        get
        {
            var forward = new Vector3(MathF.Sin(_yaw), 0f, MathF.Cos(_yaw));
            if (forward.LengthSquared() > 0.000001f)
            {
                forward.Normalize();
            }

            return forward;
        }
    }

    public Vector3 Right
    {
        get
        {
            var right = Vector3.Cross(Forward, Vector3.Up);
            if (right.LengthSquared() > 0.000001f)
            {
                right.Normalize();
            }

            return right;
        }
    }

    private void RefreshTarget()
    {
        Vector3 forward = new Vector3(MathF.Sin(_yaw) * MathF.Cos(_pitch), MathF.Sin(_pitch), MathF.Cos(_yaw) * MathF.Cos(_pitch));
        Target = Position + forward;
    }
}
