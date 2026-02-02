using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public interface IState
{
    void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources, InputService input);
    void Update(GameTime gameTime);
    void Render(GameTime gameTime, GraphicsDevice graphicsDevice);
    bool IsExitRequested { get; }
    void SetDebugger(IDebugger debugger);
}
