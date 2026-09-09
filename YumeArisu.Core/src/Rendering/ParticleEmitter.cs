using System.Numerics;
using YumeArisu.Core.Common;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Rendering;

public sealed class ParticleEmitter : Renderer
{
    private const int DefaultCapacity = 1000;

    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;
        public float Rotation;
        public float AngularVelocity;
    }

    public Sprite Sprite { get; set; }
    public float EmissionRate { get; set; } = 10f;
    public int Capacity => DefaultCapacity;
    public float ParticleLifetime { get; set; } = 1f;
    public Vector2 EmissionDirection { get; set; } = Vector2.UnitY;
    public float EmissionSpread { get; set; } = MathF.PI;
    public float StartSpeed { get; set; } = 1f;
    public Vector2 StartVelocity { get; set; }
    public Vector2 Gravity { get; set; }
    public float StartSize { get; set; } = 1f;
    public float EndSize { get; set; } = 1f;
    public Color StartColor { get; set; } = Color.White;
    public Color EndColor { get; set; } = Color.White;
    public float StartRotation { get; set; }
    public float StartAngularVelocity { get; set; }
    public bool PlayOnAwake { get; set; } = true;
    public bool IsPlaying => _isPlaying;
    public int ParticleCount => _particleCount;

    internal SpriteBatcher Batcher { get; set; }

    private readonly Particle[] _particles = new Particle[DefaultCapacity];
    private readonly Random _random = new();
    private int _particleCount;
    private float _emissionAccumulator;
    private bool _isPlaying;

    public ParticleEmitter()
    {
        Material = BuiltInRenderResources.DefaultSpriteMaterial;
        _isPlaying = PlayOnAwake;
    }

    public void Play() => _isPlaying = true;

    public void Stop() => _isPlaying = false;

    public void Clear()
    {
        _particleCount = 0;
        _emissionAccumulator = 0f;
    }

    public void Emit(int count = 1)
    {
        if (count <= 0)
            return;

        int emitCount = Math.Min(count, Capacity - _particleCount);
        for (int i = 0; i < emitCount; i++)
            _particles[_particleCount++] = CreateParticle();
    }

    protected internal override void OnAttach()
    {
        RenderSystem.Instance.RegisterParticleEmitter(this);
    }

    protected internal override void OnDetach()
    {
        RenderSystem.Instance.UnregisterParticleEmitter(this);
    }

    internal void UpdateParticles()
    {
        float deltaTime = Time.DeltaTime;

        for (int i = _particleCount - 1; i >= 0; i--)
        {
            Particle particle = _particles[i];
            particle.Age += deltaTime;
            if (particle.Age >= particle.Lifetime)
            {
            _particles[i] = _particles[--_particleCount];
                continue;
            }

            particle.Velocity += Gravity * deltaTime;
            particle.Position += particle.Velocity * deltaTime;
            particle.Rotation += particle.AngularVelocity * deltaTime;
            _particles[i] = particle;
        }

        if (!_isPlaying || EmissionRate <= 0f || Capacity <= _particleCount)
            return;

        _emissionAccumulator += EmissionRate * deltaTime;
        int emitCount = (int)_emissionAccumulator;
        _emissionAccumulator -= emitCount;
        Emit(emitCount);
    }

    internal override void Draw(RenderContext context)
    {
        if (Sprite is null || Sprite.Texture is null)
            return;

        float sizeX = Sprite.Rect.Size.X / Sprite.PPU;
        float sizeY = Sprite.Rect.Size.Y / Sprite.PPU;
        float u0 = Sprite.Rect.Origin.X / Sprite.Texture.Width;
        float v0 = Sprite.Rect.Origin.Y / Sprite.Texture.Height;
        float uw = Sprite.Rect.Size.X / Sprite.Texture.Width;
        float vh = Sprite.Rect.Size.Y / Sprite.Texture.Height;
        Vector2 pivot = Sprite.Pivot;

        for (int particleIndex = 0; particleIndex < _particleCount; particleIndex++)
        {
            Particle particle = _particles[particleIndex];
            float normalizedAge = Math.Clamp(particle.Age / particle.Lifetime, 0f, 1f);
            Vector4 color = Vector4.Lerp(StartColor.ToVector4(), EndColor.ToVector4(), normalizedAge);
            float size = StartSize + (EndSize - StartSize) * normalizedAge;
            Matrix4x4 model =
                Matrix4x4.CreateRotationZ(particle.Rotation)
                * Matrix4x4.CreateTranslation(new Vector3(particle.Position, 0f))
                * Transform.WorldMatrix;

            Batcher.Submit(
                Sprite.Texture,
                model,
                new Vector2(sizeX * size, sizeY * size),
                pivot,
                new Vector4(u0, v0, uw, vh),
                color,
                Material.BlendMode,
                ViewSpaceDepth);
        }
    }

    private Particle CreateParticle()
    {
        Vector2 direction = EmissionDirection;
        if (direction.LengthSquared() < float.Epsilon)
            direction = Vector2.UnitY;
        else
            direction = Vector2.Normalize(direction);

        float angle = MathF.Atan2(direction.Y, direction.X)
            + NextFloat(-EmissionSpread * 0.5f, EmissionSpread * 0.5f);
        Vector2 randomDirection = new(MathF.Cos(angle), MathF.Sin(angle));

        return new Particle
        {
            Velocity = StartVelocity + randomDirection * StartSpeed,
            Lifetime = MathF.Max(0.0001f, ParticleLifetime),
            Rotation = StartRotation,
            AngularVelocity = StartAngularVelocity
        };
    }

    private float NextFloat(float min, float max) => min + (float)_random.NextDouble() * (max - min);
}