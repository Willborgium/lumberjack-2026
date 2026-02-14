using Microsoft.Xna.Framework;

namespace Lumberjack;

public interface ITranslatable
{
    Vector3 Position { get; }
    void Translate(Vector3 delta);
}
