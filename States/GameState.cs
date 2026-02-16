using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class GameState : BaseState
{
    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var textureEffect = Resources.GetContent<Effect>(ContentPaths.TextureEffect);

        var cubeTexture = Resources.GetContent<Texture2D>(ContentPaths.BrickTexture);

        var floorTexture = Resources.GetContent<Texture2D>(ContentPaths.Wood15Texture);

        var playerMesh = TestFunctions.CreateTexturedRectangularPrism(textureEffect.Clone(), cubeTexture, 1f, 3f, 1f);
        playerMesh.Position = new Vector3(0f, 1.5f, 0f);
        Renderables.Add(playerMesh);

        // We're going to tesselate this floor and add some dimension to it
        var floor = TestFunctions.CreateTexturedPlane(textureEffect.Clone(), floorTexture, 250f, 250f, 50);
        floor.CullMode = CullMode.None;
        floor.EnableBehindCameraCulling = false;

        floor.Position = Vector3.Zero;
        Renderables.Add(floor);

        var thirdPersonCamera = new ThirdPersonCamera(playerMesh, Input)
        {
            Distance = 10f,
            HeightOffset = 1.5f,
            SmoothSpeed = 10f,
        };
        thirdPersonCamera.SetViewport(graphicsDevice.Viewport);

        Camera = thirdPersonCamera;
        Updatables.Add(thirdPersonCamera);

        // Buttons -> Actions -> Translations -> Applied Translations
        var playerEmitter = new InputMovementActionEmitter(Input);
        var playerTranslator = new GroundMovementTranslator(playerEmitter, thirdPersonCamera)
        {
            MoveSpeed = 10f,
            RunMultiplier = 2.5f
        };
        var playerApplier = new TranslationApplier(playerMesh, playerTranslator);

        Updatables.Add(playerEmitter);
        Updatables.Add(playerTranslator);
        Updatables.Add(playerApplier);

        var collisionSystem = new CollisionSystem(SetDebugStat);

        collisionSystem.Register(new CollisionBody("player-mesh", "player", playerMesh, new BoxCollisionShape(HalfExtents: new Vector3(0.5f, 1.5f, 0.5f), Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("camera", "camera", thirdPersonCamera, new SphereCollisionShape(Radius: 0.5f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("floor", "environment", floor, new BoxCollisionShape(HalfExtents: new Vector3(60f, 0.25f, 60f), Offset: new Vector3(0f, -0.25f, 0f))));

        collisionSystem.SetTypePairRule("environment", "environment", canCollide: false);
        collisionSystem.SetObjectPairRule("camera", "floor", canCollide: false);

        Updatables.Add(collisionSystem);

        Updatables.Add(new CameraDebugger(thirdPersonCamera, SetDebugStat));

        var skyTexture = Resources.GetContent<Texture2D>(ContentPaths.SkyTexture);
        var skyboxEffect = Resources.GetContent<Effect>(ContentPaths.SkyboxEffect);
        var skybox = new Skybox(skyboxEffect.Clone(), thirdPersonCamera, skyTexture, 200f);
        Renderables.Add(skybox);
        Updatables.Add(skybox);
    }
}
