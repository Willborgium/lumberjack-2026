using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public interface IState
{
    bool IsExitRequested { get; }

    void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources, InputService input);
    void Update(GameTime gameTime);
    void Render(GameTime gameTime, GraphicsDevice graphicsDevice);

    void SetDebugger(IDebugger debugger);
}
