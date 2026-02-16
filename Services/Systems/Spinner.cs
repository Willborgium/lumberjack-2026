using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Lumberjack.Services.Systems;

public class Spinner : IUpdatable
{
    public void Update(GameTime gameTime)
    {
        foreach (var target in _targets)
        {
            target.Rotation += new Vector3(0.01f, 0.02f, 0.03f);
        }
    }

    public void AddTarget(Renderable3DBase target)
    {
        _targets.Add(target);
    }

    private readonly List<Renderable3DBase> _targets = [];
}
