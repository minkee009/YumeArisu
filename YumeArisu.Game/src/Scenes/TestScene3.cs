using System.Numerics;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scenes;

public class TestScene3 : Scene
{
    private Sprite _stressSprite;

    protected override void OnLoad()
    {
        _stressSprite = Resources.Get<Sprite>("Sprite/Yuuka.sprite");

        var cameraObject = CreateGameObject("StressCamera");
        cameraObject.Transform.LocalPosition = new Vector3(0f, 0f, 20f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.ProjectionMode = ProjectionMode.Orthogonal;
        camera.Size = 20f;
        camera.NearPlane = -100f;
        camera.FarPlane = 100f;

        const int columns = 30;
        const int rows = 20;
        const float spacing = 1.1f;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var stressObject = CreateGameObject($"StressSprite_{row}_{column}");
                stressObject.Transform.LocalPosition = new Vector3(
                    (column - (columns - 1) * 0.5f) * spacing,
                    (row - (rows - 1) * 0.5f) * spacing,
                    0f);

                var renderer = stressObject.AddComponent<SpriteRenderer>();
                renderer.Sprite = _stressSprite;
                renderer.Color = new Color(
                    0.25f + column / (float)columns * 0.75f,
                    0.25f + row / (float)rows * 0.75f,
                    1f,
                    1f);
                renderer.Enabled = true;
            }
        }

        CreateParticleEmitter("StressEmitter_Center", Vector2.Zero, Vector2.UnitY);
        CreateParticleEmitter("StressEmitter_Left", new Vector2(-10f, 0f), Vector2.UnitX);
        CreateParticleEmitter("StressEmitter_Right", new Vector2(10f, 0f), -Vector2.UnitX);
        CreateParticleEmitter("StressEmitter_Top", new Vector2(0f, 7f), -Vector2.UnitY);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Resources.Release(_stressSprite);
    }

    private void CreateParticleEmitter(string name, Vector2 position, Vector2 direction)
    {
        var emitterObject = CreateGameObject(name);
        emitterObject.Transform.LocalPosition = new Vector3(position, 0f);

        var emitter = emitterObject.AddComponent<ParticleEmitter>();
        emitter.Sprite = _stressSprite;
        emitter.EmissionRate = 250f;
        emitter.ParticleLifetime = 5f;
        emitter.EmissionDirection = direction;
        emitter.EmissionSpread = MathF.PI * 0.5f;
        emitter.StartSpeed = 3f;
        emitter.Gravity = new Vector2(0f, -2f);
        emitter.StartSize = 0.5f;
        emitter.EndSize = 1.5f;
        emitter.StartColor = Color.White;
        emitter.EndColor = new Color(0.2f, 0.6f, 1f, 0f);
        emitter.Enabled = true;
        emitter.Play();
    }
}