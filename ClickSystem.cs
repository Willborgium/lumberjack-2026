using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public interface IClickTarget
{
    bool HitTest(Ray ray, out float distance);
}

public interface IClickReceiver
{
    void OnClicked(string id, IClickTarget target);
}

public class SphereClickTarget(ITranslatable target, float radius, Vector3 offset) : IClickTarget
{
    public bool HitTest(Ray ray, out float distance)
    {
        var center = target.Position + offset;
        var value = ray.Intersects(new BoundingSphere(center, radius));
        if (value.HasValue)
        {
            distance = value.Value;
            return true;
        }

        distance = float.MaxValue;
        return false;
    }
}

public class DebugLogClickReceiver : IClickReceiver
{
    public void OnClicked(string id, IClickTarget target)
    {
        DebugLog.Log($"Clicked: {id}");
    }
}

public class ClickSelectionSystem(
    GraphicsDevice graphicsDevice,
    InputService input,
    ICamera camera,
    Func<Matrix> projectionProvider) : IUpdatable
{
    private readonly List<ClickRegistration> _registrations = [];

    public void Register(string id, IClickTarget target, IClickReceiver receiver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(receiver);

        _registrations.Add(new ClickRegistration(id, target, receiver));
    }

    public void Update(GameTime gameTime)
    {
        if (!input.IsAction(InputAction.PrimaryClick))
        {
            return;
        }

        var ray = CreateMouseRay();
        ClickRegistration? best = null;
        var bestDistance = float.MaxValue;

        foreach (var registration in _registrations)
        {
            if (!registration.Target.HitTest(ray, out var distance))
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = registration;
            }
        }

        if (best is null)
        {
            return;
        }

        best.Value.Receiver.OnClicked(best.Value.Id, best.Value.Target);
    }

    private Ray CreateMouseRay()
    {
        var viewport = graphicsDevice.Viewport;
        var mouse = input.MousePosition;

        var view = camera.GetViewMatrix();
        var projection = projectionProvider();

        var nearPoint = viewport.Unproject(new Vector3(mouse.X, mouse.Y, 0f), projection, view, Matrix.Identity);
        var farPoint = viewport.Unproject(new Vector3(mouse.X, mouse.Y, 1f), projection, view, Matrix.Identity);
        var direction = farPoint - nearPoint;
        if (direction.LengthSquared() > 0.000001f)
        {
            direction.Normalize();
        }

        return new Ray(nearPoint, direction);
    }

    private readonly record struct ClickRegistration(string Id, IClickTarget Target, IClickReceiver Receiver);
}
