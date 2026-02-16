using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack.States;

public class DebugWatcher(string key, Func<object> valueFunc, Action<string, string> setDebugStatFunc) : IUpdatable
{
    public void Update(GameTime gameTime) => setDebugStatFunc(key, $"{valueFunc()}");
}

public interface IPlayerState
{
    int WoodCount { get; set; }
}

public class ValueBinder<T>(Func<T> valueFunc, Action<T> setValueFunc) : IUpdatable
{
    public void Update(GameTime gameTime)
    {
        setValueFunc(valueFunc());
    }
}

public class GameState : BaseState, IPlayerState
{
    public int WoodCount { get; set; } = 0;

    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var textureEffect = Resources.GetContent<Effect>(ContentPaths.CelShadingEffect);

        var cubeTexture = Resources.GetContent<Texture2D>(ContentPaths.BrickTexture);
        var playerMesh = TestFunctions.CreateTexturedRectangularPrism(textureEffect.Clone(), cubeTexture, 1f, 3f, 1f);
        playerMesh.Position = new Vector3(0f, 1.51f, 0f);
        Renderables.Add(playerMesh);

        var treeTexture = Resources.GetContent<Texture2D>(ContentPaths.Wood22Texture);
        var treeMesh = TestFunctions.CreateTexturedRectangularPrism(textureEffect.Clone(), treeTexture, 0.5f, 10f, 0.5f);
        treeMesh.Position = new Vector3(25f, 5f, 25f);
        Renderables.Add(treeMesh);

        // We're going to tesselate this floor and add some dimension to it
        var floorTexture = Resources.GetContent<Texture2D>(ContentPaths.Wood15Texture);
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

        var playerRotationBinder = new ValueBinder<Vector3>(
            () => thirdPersonCamera.Forward,
            cameraForward => playerMesh.Rotation = new Vector3(playerMesh.Rotation.X, MathF.Atan2(cameraForward.X, cameraForward.Z), playerMesh.Rotation.Z)
        );
        Updatables.Add(playerRotationBinder);

        Camera = thirdPersonCamera;
        Updatables.Add(thirdPersonCamera);

        Updatables.Add(new DebugWatcher("Wood Count", () => WoodCount, SetDebugStat));

        // Buttons -> Actions -> Translations -> Applied Translations
        var playerMovementEmitter = new InputMovementActionEmitter(Input);
        Updatables.Add(playerMovementEmitter);

        var playerTranslator = new GroundMovementTranslator(playerMovementEmitter, thirdPersonCamera)
        {
            MoveSpeed = 10f,
            RunMultiplier = 2.5f
        };
        Updatables.Add(playerTranslator);

        var playerApplier = new TranslationApplier(playerMesh, playerTranslator);
        Updatables.Add(playerApplier);

        var playerActionEmitter = new PlayerActionEmitter(Input);
        Updatables.Add(playerActionEmitter);

        var playerActionHandler = new PlayerActionHandler(playerActionEmitter, this);
        Updatables.Add(playerActionHandler);

        var collisionSystem = new CollisionSystem(SetDebugStat);

        collisionSystem.Register(new CollisionBody("player-mesh", "player", playerMesh, BoxCollisionShape.FromRenderable(playerMesh)));
        collisionSystem.Register(new CollisionBody("tree-mesh", "inanimate-object", treeMesh, CapsuleCollisionShape.FromRenderable(treeMesh, padding: 0.1f)));
        collisionSystem.Register(new CollisionBody("camera", "camera", thirdPersonCamera, new SphereCollisionShape(Radius: 0.5f, Offset: Vector3.Zero)));
        collisionSystem.Register(new CollisionBody("floor", "environment", floor, new BoxCollisionShape(HalfExtents: new Vector3(60f, 0.25f, 60f), Offset: new Vector3(0f, -0.25f, 0f))));

        collisionSystem.SetTypePairRule("inanimate-object", "environment", canCollide: false);
        collisionSystem.SetTypePairRule("inanimate-object", "inanimate-object", canCollide: false);
        collisionSystem.SetTypePairRule("environment", "environment", canCollide: false);
        collisionSystem.SetObjectPairRule("camera", "floor", canCollide: false);
        collisionSystem.AddObjectPairListener("player-mesh", "tree-mesh", details => DebugLog.Log($"Listener object-object: {details.LeftObjectId}<->{details.RightObjectId}"));
        collisionSystem.AddObjectTypeListener("player-mesh", "inanimate-object", details => DebugLog.Log($"Listener object-type: {details.LeftObjectId} vs {details.RightType}"));
        collisionSystem.AddTypePairListener("player", "inanimate-object", details => DebugLog.Log($"Listener type-type: {details.LeftType}<->{details.RightType}"));

        Updatables.Add(collisionSystem);

        Updatables.Add(new CameraDebugger(thirdPersonCamera, SetDebugStat));

        var skyTexture = Resources.GetContent<Texture2D>(ContentPaths.SkyTexture);
        var skyboxEffect = Resources.GetContent<Effect>(ContentPaths.SkyboxEffect);
        var skybox = new Skybox(skyboxEffect.Clone(), thirdPersonCamera, skyTexture, 200f);
        Renderables.Add(skybox);
        Updatables.Add(skybox);
    }
}
