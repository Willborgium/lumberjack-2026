using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class DemoState : BaseState
{
    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var textureEffect = Resources.GetContent<Effect>(ContentPaths.TextureEffect);

        var cubeTexture = Resources.GetContent<Texture2D>(ContentPaths.BrickTexture);
        var sphereTexture = Resources.GetContent<Texture2D>(ContentPaths.MetalTexture);
        var pyramidTexture = Resources.GetContent<Texture2D>(ContentPaths.WoodTexture);
        var prismTexture = Resources.GetContent<Texture2D>(ContentPaths.StoneTexture);
        var floorTexture = Resources.GetContent<Texture2D>(ContentPaths.TileTexture);

        var playerCube = TestFunctions.CreateTexturedCube(textureEffect.Clone(), cubeTexture, size: 1f);
        playerCube.Position = new Vector3(-3.0f, 0.5f, 0f);
        Renderables.Add(playerCube);

        var sphere = TestFunctions.CreateTexturedSphere(textureEffect.Clone(), sphereTexture, 10, 14, 0.9f);
        sphere.Position = new Vector3(0f, 0.9f, 0f);
        Renderables.Add(sphere);

        var pyramid = TestFunctions.CreateTexturedPyramid(textureEffect.Clone(), pyramidTexture, 0.9f, 1.4f);
        pyramid.Position = new Vector3(3.0f, 0f, 0f);
        Renderables.Add(pyramid);

        var prism = TestFunctions.CreateTexturedRectangularPrism(textureEffect.Clone(), prismTexture, 1.6f, 0.8f, 0.6f);
        prism.Position = new Vector3(0f, 0.4f, 3.0f);
        Renderables.Add(prism);

        var spinner = new Spinner();
        spinner.AddTarget(sphere);
        spinner.AddTarget(pyramid);
        spinner.AddTarget(prism);
        Updatables.Add(spinner);

        var floor = TestFunctions.CreateTexturedPlane(textureEffect.Clone(), floorTexture, 120f, 120f, 24);
        floor.CullMode = CullMode.None;

        floor.Position = Vector3.Zero;
        Renderables.Add(floor);

        var thirdPersonCamera = new ThirdPersonCamera(playerCube, Input)
        {
            Distance = 7f,
            HeightOffset = 1.8f,
            SmoothSpeed = 10f,
        };
        thirdPersonCamera.SetViewport(graphicsDevice.Viewport);

        Camera = thirdPersonCamera;
        Updatables.Add(thirdPersonCamera);

        var playerEmitter = new InputMovementActionEmitter(Input);
        var playerTranslator = new GroundMovementTranslator(playerEmitter, thirdPersonCamera)
        {
            MoveSpeed = 4f,
            RunMultiplier = 2.5f
        };
        var playerApplier = new TranslationApplier(playerCube, playerTranslator);

        Updatables.Add(playerEmitter);
        Updatables.Add(playerTranslator);
        Updatables.Add(playerApplier);

        var npcEmitter = new PatternMovementActionEmitter(segmentDurationSeconds: 1.8f);
        var npcTranslator = new GroundMovementTranslator(npcEmitter, new WorldMovementFrameProvider())
        {
            MoveSpeed = 2f,
            RunMultiplier = 1f
        };
        var npcApplier = new TranslationApplier(prism, npcTranslator);

        Updatables.Add(npcEmitter);
        Updatables.Add(npcTranslator);
        Updatables.Add(npcApplier);

        var collisionSystem = new CollisionSystem(SetDebugStat);
        collisionSystem.Register(new CollisionBody("player-cube", "player", playerCube, new BoxCollisionShape(HalfExtents: new Vector3(0.9f), Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("camera", "camera", thirdPersonCamera, new SphereCollisionShape(Radius: 0.5f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("npc-prism", "npc", prism, new BoxCollisionShape(HalfExtents: new Vector3(0.8f, 0.4f, 0.3f), Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("sphere", "obstacle", sphere, new SphereCollisionShape(Radius: 0.95f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("pyramid", "obstacle", pyramid, new CapsuleCollisionShape(radius: 0.5f, halfHeight: 0.7f, offset: new Vector3(0f, 0.35f, 0f))));
        collisionSystem.Register(new CollisionBody("floor", "environment", floor, new BoxCollisionShape(HalfExtents: new Vector3(60f, 0.25f, 60f), Offset: new Vector3(0f, -0.25f, 0f))));

        collisionSystem.SetTypePairRule("environment", "environment", canCollide: false);
        collisionSystem.SetObjectTypeRule("npc-prism", "environment", canCollide: false);
        collisionSystem.SetObjectPairRule("camera", "floor", canCollide: false);

        Updatables.Add(collisionSystem);

        var clickReceiver = new DebugLogClickReceiver();
        var clickSystem = new ClickSelectionSystem(graphicsDevice, Input, thirdPersonCamera, () => GetProjection(graphicsDevice));
        clickSystem.Register("player-cube", new SphereClickTarget(playerCube, radius: 1.1f, offset: Vector3.Zero), clickReceiver);
        clickSystem.Register("npc-prism", new SphereClickTarget(prism, radius: 1.0f, offset: Vector3.Zero), clickReceiver);
        clickSystem.Register("sphere", new SphereClickTarget(sphere, radius: 1.0f, offset: Vector3.Zero), clickReceiver);
        clickSystem.Register("pyramid", new SphereClickTarget(pyramid, radius: 1.0f, offset: new Vector3(0f, 0.5f, 0f)), clickReceiver);
        Updatables.Add(clickSystem);

        Updatables.Add(new CameraDebugger(thirdPersonCamera, SetDebugStat));

        var skyTexture = Resources.GetContent<Texture2D>(ContentPaths.SkyTexture);
        var skyboxEffect = Resources.GetContent<Effect>(ContentPaths.SkyboxEffect);
        var skybox = new Skybox(skyboxEffect.Clone(), thirdPersonCamera, skyTexture, 200f);
        Renderables.Add(skybox);
        Updatables.Add(skybox);

        // Keep POV camera implementation available for future modes while gameplay uses third-person.
        _ = new POVCamera(new Vector3(0f, 2f, 6f), Vector3.Zero, Input);
    }
}
