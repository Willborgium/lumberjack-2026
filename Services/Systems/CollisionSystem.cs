using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Lumberjack;

public abstract record CollisionShape(Vector3 Offset);

public sealed record SphereCollisionShape(float Radius, Vector3 Offset) : CollisionShape(Offset);

public sealed record BoxCollisionShape(Vector3 HalfExtents, Vector3 Offset) : CollisionShape(Offset);

public sealed record CapsuleCollisionShape(float Radius, float HalfHeight, Vector3 Offset, Vector3 Up) : CollisionShape(Offset)
{
    public CapsuleCollisionShape(float radius, float halfHeight, Vector3 offset)
        : this(radius, halfHeight, offset, Vector3.Up)
    {
    }
}

public sealed class CollisionBody(string id, string type, ITranslatable target, CollisionShape shape)
{
    public string Id { get; } = id;
    public string Type { get; } = type;
    public ITranslatable Target { get; } = target;
    public CollisionShape Shape { get; } = shape;
}

public class CollisionSystem(Action<string, string> setDebugStat) : IUpdatable
{
    private readonly List<CollisionBody> _bodies = [];
    private readonly Dictionary<(string, string), bool> _objectPairRules = [];
    private readonly Dictionary<(string, string), bool> _typePairRules = [];
    private readonly Dictionary<(string, string), bool> _objectTypeRules = [];

    public void Register(CollisionBody body)
    {
        ArgumentNullException.ThrowIfNull(body);
        _bodies.Add(body);
    }

    public void SetObjectPairRule(string objectA, string objectB, bool canCollide)
    {
        _objectPairRules[CanonicalPair(objectA, objectB)] = canCollide;
    }

    public void SetTypePairRule(string typeA, string typeB, bool canCollide)
    {
        _typePairRules[CanonicalPair(typeA, typeB)] = canCollide;
    }

    public void SetObjectTypeRule(string objectId, string type, bool canCollide)
    {
        _objectTypeRules[(objectId, type)] = canCollide;
    }

    public void Update(GameTime gameTime)
    {
        var collisions = 0;
        var last = "none";

        for (int i = 0; i < _bodies.Count; i++)
        {
            for (int j = i + 1; j < _bodies.Count; j++)
            {
                var a = _bodies[i];
                var b = _bodies[j];

                if (!CanCollide(a, b))
                {
                    continue;
                }

                if (!Intersects(a, b))
                {
                    continue;
                }

                collisions++;
                last = $"{a.Id} <-> {b.Id}";
                DebugLog.Log($"Collision: {last}");
            }
        }

        setDebugStat("Collision Count", collisions.ToString());
        setDebugStat("Last Collision", last);
    }

    private bool CanCollide(CollisionBody a, CollisionBody b)
    {
        if (_objectPairRules.TryGetValue(CanonicalPair(a.Id, b.Id), out var pairRule))
        {
            return pairRule;
        }

        if (_objectTypeRules.TryGetValue((a.Id, b.Type), out var objectTypeRuleA))
        {
            return objectTypeRuleA;
        }

        if (_objectTypeRules.TryGetValue((b.Id, a.Type), out var objectTypeRuleB))
        {
            return objectTypeRuleB;
        }

        if (_typePairRules.TryGetValue(CanonicalPair(a.Type, b.Type), out var typeRule))
        {
            return typeRule;
        }

        return true;
    }

    private static bool Intersects(CollisionBody a, CollisionBody b)
    {
        return (a.Shape, b.Shape) switch
        {
            (SphereCollisionShape sa, SphereCollisionShape sb) => SphereSphere(sa, a.Target.Position, sb, b.Target.Position),
            (BoxCollisionShape ba, BoxCollisionShape bb) => BoxBox(ba, a.Target.Position, bb, b.Target.Position),
            (SphereCollisionShape sa, BoxCollisionShape bb) => SphereBox(sa, a.Target.Position, bb, b.Target.Position),
            (BoxCollisionShape ba, SphereCollisionShape sb) => SphereBox(sb, b.Target.Position, ba, a.Target.Position),
            (CapsuleCollisionShape ca, SphereCollisionShape sb) => CapsuleSphere(ca, a.Target.Position, sb, b.Target.Position),
            (SphereCollisionShape sa, CapsuleCollisionShape cb) => CapsuleSphere(cb, b.Target.Position, sa, a.Target.Position),
            (CapsuleCollisionShape ca, CapsuleCollisionShape cb) => CapsuleCapsule(ca, a.Target.Position, cb, b.Target.Position),
            (CapsuleCollisionShape ca, BoxCollisionShape bb) => CapsuleBox(ca, a.Target.Position, bb, b.Target.Position),
            (BoxCollisionShape ba, CapsuleCollisionShape cb) => CapsuleBox(cb, b.Target.Position, ba, a.Target.Position),
            _ => false,
        };
    }

    private static bool SphereSphere(SphereCollisionShape a, Vector3 aPos, SphereCollisionShape b, Vector3 bPos)
    {
        var aCenter = aPos + a.Offset;
        var bCenter = bPos + b.Offset;
        var r = a.Radius + b.Radius;
        return Vector3.DistanceSquared(aCenter, bCenter) <= r * r;
    }

    private static bool BoxBox(BoxCollisionShape a, Vector3 aPos, BoxCollisionShape b, Vector3 bPos)
    {
        var aCenter = aPos + a.Offset;
        var bCenter = bPos + b.Offset;

        return MathF.Abs(aCenter.X - bCenter.X) <= a.HalfExtents.X + b.HalfExtents.X
            && MathF.Abs(aCenter.Y - bCenter.Y) <= a.HalfExtents.Y + b.HalfExtents.Y
            && MathF.Abs(aCenter.Z - bCenter.Z) <= a.HalfExtents.Z + b.HalfExtents.Z;
    }

    private static bool SphereBox(SphereCollisionShape sphere, Vector3 spherePos, BoxCollisionShape box, Vector3 boxPos)
    {
        var sphereCenter = spherePos + sphere.Offset;
        var boxCenter = boxPos + box.Offset;
        var min = boxCenter - box.HalfExtents;
        var max = boxCenter + box.HalfExtents;
        var closest = Vector3.Clamp(sphereCenter, min, max);
        var distSq = Vector3.DistanceSquared(sphereCenter, closest);
        return distSq <= sphere.Radius * sphere.Radius;
    }

    private static bool CapsuleSphere(CapsuleCollisionShape capsule, Vector3 capsulePos, SphereCollisionShape sphere, Vector3 spherePos)
    {
        GetCapsuleSegment(capsule, capsulePos, out var a, out var b);
        var center = spherePos + sphere.Offset;
        var closest = ClosestPointOnSegment(center, a, b);
        var r = capsule.Radius + sphere.Radius;
        return Vector3.DistanceSquared(closest, center) <= r * r;
    }

    private static bool CapsuleCapsule(CapsuleCollisionShape aCapsule, Vector3 aPos, CapsuleCollisionShape bCapsule, Vector3 bPos)
    {
        GetCapsuleSegment(aCapsule, aPos, out var a0, out var a1);
        GetCapsuleSegment(bCapsule, bPos, out var b0, out var b1);
        var distSq = SegmentSegmentDistanceSquared(a0, a1, b0, b1);
        var r = aCapsule.Radius + bCapsule.Radius;
        return distSq <= r * r;
    }

    private static bool CapsuleBox(CapsuleCollisionShape capsule, Vector3 capsulePos, BoxCollisionShape box, Vector3 boxPos)
    {
        GetCapsuleSegment(capsule, capsulePos, out var s0, out var s1);
        var boxCenter = boxPos + box.Offset;
        var min = boxCenter - box.HalfExtents - new Vector3(capsule.Radius);
        var max = boxCenter + box.HalfExtents + new Vector3(capsule.Radius);

        if (SegmentIntersectsAabb(s0, s1, min, max))
        {
            return true;
        }

        var closest0 = Vector3.Clamp(s0, min, max);
        var closest1 = Vector3.Clamp(s1, min, max);

        return Vector3.DistanceSquared(s0, closest0) <= capsule.Radius * capsule.Radius
            || Vector3.DistanceSquared(s1, closest1) <= capsule.Radius * capsule.Radius;
    }

    private static void GetCapsuleSegment(CapsuleCollisionShape capsule, Vector3 worldPosition, out Vector3 start, out Vector3 end)
    {
        var center = worldPosition + capsule.Offset;
        var up = capsule.Up;
        if (up.LengthSquared() <= 0.000001f)
        {
            up = Vector3.Up;
        }

        up.Normalize();
        var offset = up * capsule.HalfHeight;
        start = center - offset;
        end = center + offset;
    }

    private static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSquared();
        if (lenSq <= 0.000001f)
        {
            return a;
        }

        var t = Vector3.Dot(point - a, ab) / lenSq;
        t = MathHelper.Clamp(t, 0f, 1f);
        return a + ab * t;
    }

    private static float SegmentSegmentDistanceSquared(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        var d1 = q1 - p1;
        var d2 = q2 - p2;
        var r = p1 - p2;
        var a = Vector3.Dot(d1, d1);
        var e = Vector3.Dot(d2, d2);
        var f = Vector3.Dot(d2, r);

        float s;
        float t;

        if (a <= 0.000001f && e <= 0.000001f)
        {
            return Vector3.DistanceSquared(p1, p2);
        }

        if (a <= 0.000001f)
        {
            s = 0f;
            t = MathHelper.Clamp(f / e, 0f, 1f);
        }
        else
        {
            var c = Vector3.Dot(d1, r);
            if (e <= 0.000001f)
            {
                t = 0f;
                s = MathHelper.Clamp(-c / a, 0f, 1f);
            }
            else
            {
                var b = Vector3.Dot(d1, d2);
                var denom = a * e - b * b;

                s = denom != 0f ? MathHelper.Clamp((b * f - c * e) / denom, 0f, 1f) : 0f;
                t = (b * s + f) / e;

                if (t < 0f)
                {
                    t = 0f;
                    s = MathHelper.Clamp(-c / a, 0f, 1f);
                }
                else if (t > 1f)
                {
                    t = 1f;
                    s = MathHelper.Clamp((b - c) / a, 0f, 1f);
                }
            }
        }

        var c1 = p1 + d1 * s;
        var c2 = p2 + d2 * t;
        return Vector3.DistanceSquared(c1, c2);
    }

    private static bool SegmentIntersectsAabb(Vector3 start, Vector3 end, Vector3 min, Vector3 max)
    {
        var dir = end - start;
        float tMin = 0f;
        float tMax = 1f;

        if (!SlabIntersect(start.X, dir.X, min.X, max.X, ref tMin, ref tMax)) return false;
        if (!SlabIntersect(start.Y, dir.Y, min.Y, max.Y, ref tMin, ref tMax)) return false;
        if (!SlabIntersect(start.Z, dir.Z, min.Z, max.Z, ref tMin, ref tMax)) return false;

        return tMax >= tMin;
    }

    private static bool SlabIntersect(float start, float dir, float min, float max, ref float tMin, ref float tMax)
    {
        if (MathF.Abs(dir) <= 0.000001f)
        {
            return start >= min && start <= max;
        }

        var inv = 1f / dir;
        var t0 = (min - start) * inv;
        var t1 = (max - start) * inv;

        if (t0 > t1)
        {
            (t0, t1) = (t1, t0);
        }

        tMin = MathF.Max(tMin, t0);
        tMax = MathF.Min(tMax, t1);

        return tMax >= tMin;
    }

    private static (string, string) CanonicalPair(string left, string right)
    {
        return string.CompareOrdinal(left, right) <= 0 ? (left, right) : (right, left);
    }
}
