using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class StateManager
{
    private IState? _current;
    private readonly ResourceManager _resources;
    private readonly InputService _input;

    public StateManager(ResourceManager resources, InputService input)
    {
        _resources = resources;
        _input = input;
    }

    public bool IsExitRequested => _current?.IsExitRequested ?? false;

    public void SetState(IState state, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _resources.Get<BasicEffect>("default-basic-effect", (c, g) =>
        {
            var fx = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = true,
                SpecularPower = 16f,
                View = Matrix.Identity,
                Projection = Matrix.Identity
            };
            fx.EnableDefaultLighting();
            fx.DirectionalLight0.Enabled = true;
            fx.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
            fx.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
            fx.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
            fx.AmbientLightColor = new Vector3(0.18f, 0.18f, 0.18f);
            return fx;
        });

        _current = state;
        _current.Load(content, graphicsDevice, _resources, _input);
    }

    public void Update(GameTime gameTime)
    {
        _input.Update(gameTime);
        _current?.Update(gameTime);
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice) => _current?.Render(gameTime, graphicsDevice);
}
