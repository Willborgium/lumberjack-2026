using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class GameState : BaseState
{
    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var colorEffect = Resources.GetContent<Effect>(ContentPaths.ColorEffect);

        var cube = TestFunctions.CreateCube(colorEffect.Clone());
        cube.Position = new Vector3(-3.5f, 0f, 0f);
        Renderables.Add(cube);

        var sphere = TestFunctions.CreateSphere(colorEffect.Clone(), 10, 14, 0.9f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        Renderables.Add(sphere);

        var pyramid = TestFunctions.CreatePyramid(colorEffect.Clone(), 0.9f, 1.4f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        Renderables.Add(pyramid);

        var prism = TestFunctions.CreateRectangularPrism(colorEffect.Clone(), 1.6f, 0.8f, 0.6f);
        prism.Position = new Vector3(0f, -1.6f, 0f);
        Renderables.Add(prism);

        var spinner = new Spinner();
        spinner.AddTarget(sphere);
        spinner.AddTarget(pyramid);
        spinner.AddTarget(prism);
        Updatables.Add(spinner);

        var textureEffect = Resources.GetContent<Effect>(ContentPaths.TextureEffect);
        var floorTexture = Resources.GetContent<Texture2D>(ContentPaths.GrassTexture);
        var floor = TestFunctions.CreateTexturedPlane(textureEffect.Clone(), floorTexture, 120f, 120f, 24);
        floor.CullMode = CullMode.None;

        floor.Position = new Vector3(0f, -2.2f, 0f);
        Renderables.Add(floor);

        var camera = new Camera(new Vector3(0, 0, 6f), Vector3.Zero, Input);
        Camera = camera;
        camera.SetViewport(graphicsDevice.Viewport);
        Updatables.Add(camera);

        var playerEmitter = new InputMovementActionEmitter(Input);
        var playerTranslator = new GroundMovementTranslator(playerEmitter, camera)
        {
            MoveSpeed = 4f,
            RunMultiplier = 2.5f
        };
        var playerApplier = new TranslationApplier(camera, playerTranslator);

        Updatables.Add(playerEmitter);
        Updatables.Add(playerTranslator);
        Updatables.Add(playerApplier);

        var npcEmitter = new PatternMovementActionEmitter(segmentDurationSeconds: 1.8f);
        var npcTranslator = new GroundMovementTranslator(npcEmitter, new WorldMovementFrameProvider())
        {
            MoveSpeed = 2f,
            RunMultiplier = 1f
        };
        var npcApplier = new TranslationApplier(cube, npcTranslator);

        Updatables.Add(npcEmitter);
        Updatables.Add(npcTranslator);
        Updatables.Add(npcApplier);

        var collisionSystem = new CollisionSystem(SetDebugStat);
        collisionSystem.Register(new CollisionBody("camera", "player", camera, new SphereCollisionShape(Radius: 0.5f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("npc-cube", "npc", cube, new BoxCollisionShape(HalfExtents: new Vector3(0.9f), Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("sphere", "obstacle", sphere, new SphereCollisionShape(Radius: 0.95f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("pyramid", "obstacle", pyramid, new CapsuleCollisionShape(radius: 0.5f, halfHeight: 0.7f, offset: new Vector3(0f, 0.35f, 0f))));
        collisionSystem.Register(new CollisionBody("floor", "environment", floor, new BoxCollisionShape(HalfExtents: new Vector3(60f, 0.25f, 60f), Offset: new Vector3(0f, -0.25f, 0f))));

        collisionSystem.SetTypePairRule("environment", "environment", canCollide: false);
        collisionSystem.SetObjectTypeRule("npc-cube", "environment", canCollide: false);
        collisionSystem.SetObjectPairRule("camera", "floor", canCollide: false);

        Updatables.Add(collisionSystem);

        Updatables.Add(new CameraDebugger(camera, SetDebugStat));

        var skyTexture = Resources.GetContent<Texture2D>(ContentPaths.SkyTexture);
        var skyboxEffect = Resources.GetContent<Effect>(ContentPaths.SkyboxEffect);
        var skybox = new Skybox(skyboxEffect.Clone(), camera, skyTexture, 200f);
        Renderables.Add(skybox);
        Updatables.Add(skybox);
    }
}
