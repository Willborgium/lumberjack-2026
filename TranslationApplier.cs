using Microsoft.Xna.Framework;

namespace Lumberjack;

public class TranslationApplier(ITranslatable target, GroundMovementTranslator translator) : IUpdatable
{
    public void Update(GameTime gameTime)
    {
        if (translator.Translation.LengthSquared() <= 0.000001f)
        {
            return;
        }

        target.Translate(translator.Translation);
    }
}
