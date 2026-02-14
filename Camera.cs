using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public interface ICamera
{
    Vector3 Position { get; }
    Vector3 Target { get; }
    Matrix GetViewMatrix();
}

public class POVCamera : ICamera, IUpdatable, ITranslatable, IMovementFrameProvider
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; }
    public float MouseSensitivity { get; set; } = .005f;

    private readonly InputService _input;
    private float _yaw;
    private float _pitch;
    private Viewport _viewport;

    public POVCamera(Vector3 position, Vector3 target, InputService input)
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

public class ThirdPersonCamera : ICamera, IUpdatable, IMovementFrameProvider, ITranslatable
{
    public Vector3 Position { get; private set; }
    public Vector3 Target { get; private set; }

    public float OrbitSensitivity { get; set; } = 0.005f;
    public float Distance { get; set; } = 6f;
    public float HeightOffset { get; set; } = 1.7f;
    public float SmoothSpeed { get; set; } = 12f;

    private readonly ITranslatable _followTarget;
    private readonly InputService _input;
    private float _yaw;
    private float _pitch = -0.2f;
    private Viewport _viewport;

    public ThirdPersonCamera(ITranslatable followTarget, InputService input)
    {
        _followTarget = followTarget;
        _input = input;

        var initialFocus = _followTarget.Position + Vector3.Up * HeightOffset;
        Position = initialFocus - Vector3.Forward * Distance;
        Target = initialFocus;
    }

    public void SetViewport(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Update(GameTime gameTime)
    {
        var mouseDelta = _input.MouseDelta;
        _yaw -= mouseDelta.X * OrbitSensitivity;
        _pitch -= mouseDelta.Y * OrbitSensitivity;
        _pitch = MathHelper.Clamp(_pitch, -1.2f, 0.85f);

        var focus = _followTarget.Position + Vector3.Up * HeightOffset;

        var orbitDirection = new Vector3(
            MathF.Sin(_yaw) * MathF.Cos(_pitch),
            MathF.Sin(_pitch),
            MathF.Cos(_yaw) * MathF.Cos(_pitch));

        if (orbitDirection.LengthSquared() > 0.000001f)
        {
            orbitDirection.Normalize();
        }

        var desiredPosition = focus - orbitDirection * Distance;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var alpha = 1f - MathF.Exp(-SmoothSpeed * dt);

        Position = Vector3.Lerp(Position, desiredPosition, alpha);
        Target = Vector3.Lerp(Target, focus, alpha);

        // Keep cursor near center to support continuous orbiting.
        var mouse = _input.CurrentMouse;
        if (_viewport.Width > 0 && _viewport.Height > 0)
        {
            const int edgeThreshold = 8;
            if (mouse.X < edgeThreshold || mouse.X > _viewport.Width - edgeThreshold ||
                mouse.Y < edgeThreshold || mouse.Y > _viewport.Height - edgeThreshold)
            {
                _input.WarpMouse(new Point(_viewport.Width / 2, _viewport.Height / 2));
            }
        }
    }

    public Matrix GetViewMatrix() => Matrix.CreateLookAt(Position, Target, Vector3.Up);

    public void Translate(Vector3 delta)
    {
        Position += delta;
        Target += delta;
    }

    public Vector3 Forward
    {
        get
        {
            var forward = Target - Position;
            forward.Y = 0f;
            if (forward.LengthSquared() > 0.000001f)
            {
                forward.Normalize();
                return forward;
            }

            return Vector3.Forward;
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
                return right;
            }

            return Vector3.Right;
        }
    }
}
