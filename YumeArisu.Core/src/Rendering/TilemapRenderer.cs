using System.Numerics;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Rendering;

/// <summary>
/// 타일 레이어 하나입니다. 여러 레이어가 필요하면 이 컴포넌트를 여러 개 두고
/// RenderOrder로 SpriteRenderer와 섞어서 그리는 순서를 정합니다.
/// </summary>
public class TilemapRenderer : Renderer
{
    private const int Padding = 2;           // 화면 밖 보호 여유(타일 단위)
    private const int MaxVisibleTiles = 8192; // 폭주 방지 상한

    public Tileset Tileset { get; set; }
    public Vector2 CellSize { get; set; } = Vector2.One;
    public Color Color { get; set; } = Color.White;

    internal SpriteBatcher Batcher { get; set; }

    private readonly Dictionary<long, int> _tiles = new();
    private int _minX = int.MaxValue, _minY = int.MaxValue;
    private int _maxX = int.MinValue, _maxY = int.MinValue;

    public TilemapRenderer()
    {
        Material = BuiltInRenderResources.DefaultSpriteMaterial; // SpriteRenderer와 동일한 정렬 규칙
    }

    /// <param name="tileId">0 = 빈 칸, 1부터 Tileset의 타일(왼쪽 위부터 행 우선)</param>
    public void SetTile(int x, int y, int tileId)
    {
        long key = Key(x, y);

        if (tileId == 0)
        {
            _tiles.Remove(key);
            return;
        }

        _tiles[key] = tileId;

        if (x < _minX) _minX = x;
        if (x > _maxX) _maxX = x;
        if (y < _minY) _minY = y;
        if (y > _maxY) _maxY = y;
    }

    public int GetTile(int x, int y) => _tiles.TryGetValue(Key(x, y), out int id) ? id : 0;
    public void ClearTile(int x, int y) => SetTile(x, y, 0);

    protected internal override void OnAttach() => RenderSystem.Instance.RegisterTilemapRenderer(this);
    protected internal override void OnDetach() => RenderSystem.Instance.UnregisterTilemapRenderer(this);

    internal override void Draw()
    {
        if (Tileset is null || Tileset.Columns < 0 || _tiles.Count == 0)
            return;

        var camera = RenderSystem.Instance.CurrentCamera;
        if (camera is null || !TryGetVisibleTileRange(camera, out int x0, out int y0, out int x1, out int y1))
            return;

        x0 = Math.Max(x0, _minX);
        y0 = Math.Max(y0, _minY);
        x1 = Math.Min(x1, _maxX);
        y1 = Math.Min(y1, _maxY);

        if (x0 > x1 || y0 > y1)
            return;

        var worldMatrix = Transform.WorldMatrix;
        float depth = ViewSpaceDepth;       // 레이어 전체가 같은 값 (ParticleEmitter와 동일한 방식)
        var blendMode = Material.BlendMode;

        long rangeCount = (long)(x1 - x0 + 1) * (y1 - y0 + 1);

        // 범위가 저장된 타일 수보다 훨씬 크면(성긴 맵) 딕셔너리를 직접 도는 편이 쌉니다.
        if (rangeCount > _tiles.Count * 4L)
        {
            foreach (var (key, tileId) in _tiles)
            {
                Unpack(key, out int x, out int y);
                if (x < x0 || x > x1 || y < y0 || y > y1)
                    continue;

                SubmitTile(x, y, tileId, worldMatrix, depth, blendMode);
            }
            return;
        }

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                if (_tiles.TryGetValue(Key(x, y), out int tileId))
                    SubmitTile(x, y, tileId, worldMatrix, depth, blendMode);
            }
        }
    }

    private void SubmitTile(int x, int y, int tileId, in Matrix4x4 worldMatrix, float depth, BlendMode blendMode)
    {
        if (!Tileset.GetUV(tileId, out var minMax))
            return; // 잘못된 tileId나 미로드 상태 - 조용히 스킵
            
        var uvRect = new Vector4(minMax.X, minMax.Y, minMax.Z - minMax.X, minMax.W - minMax.Y);
        var model = Matrix4x4.CreateTranslation(x * CellSize.X, y * CellSize.Y, 0f) * worldMatrix;

        Batcher.Submit(
            Tileset.Texture,
            model,
            CellSize,
            Vector2.Zero, // 피벗 : 타일 좌하단 기준
            uvRect,
            Color.ToVector4(),
            blendMode,
            depth);
    }

    /// <summary>카메라 프러스텀을 렌더러 로컬 그리드 공간으로 역투영해 보이는 타일 범위를 구합니다.</summary>
    private bool TryGetVisibleTileRange(Camera camera, out int x0, out int y0, out int x1, out int y1)
    {
        x0 = y0 = x1 = y1 = 0;

        if (!Matrix4x4.Invert(camera.ViewProjectionMatrix, out var invVP))
            return false;
        if (!Matrix4x4.Invert(Transform.WorldMatrix, out var invWorld))
            return false;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        Span<float> ndcXY = stackalloc float[] { -1f, 1f };
        // System.Numerics 투영행렬은 z를 0(near)..1(far)로 내보냄
        Span<float> ndcZ = stackalloc float[] { 0f, 1f };

        foreach (var nx in ndcXY)
        foreach (var ny in ndcXY)
        foreach (var nz in ndcZ)
        {
            var world4 = Vector4.Transform(new Vector4(nx, ny, nz, 1f), invVP);
            if (MathF.Abs(world4.W) < 1e-6f)
                continue;

            var world = new Vector3(world4.X, world4.Y, world4.Z) / world4.W;
            var local = Vector3.Transform(world, invWorld);

            if (local.X < minX) minX = local.X;
            if (local.X > maxX) maxX = local.X;
            if (local.Y < minY) minY = local.Y;
            if (local.Y > maxY) maxY = local.Y;
        }

        x0 = (int)MathF.Floor(minX / CellSize.X) - Padding;
        y0 = (int)MathF.Floor(minY / CellSize.Y) - Padding;
        x1 = (int)MathF.Ceiling(maxX / CellSize.X) + Padding;
        y1 = (int)MathF.Ceiling(maxY / CellSize.Y) + Padding;

        // 원근 카메라가 그리드와 거의 나란히 보면 범위가 폭주할 수 있어 상한을 둡니다.
        long count = (long)(x1 - x0 + 1) * (y1 - y0 + 1);
        if (count > MaxVisibleTiles)
        {
            int cx = (x0 + x1) / 2, cy = (y0 + y1) / 2;
            int half = (int)MathF.Sqrt(MaxVisibleTiles) / 2;
            x0 = cx - half; x1 = cx + half;
            y0 = cy - half; y1 = cy + half;
        }

        return true;
    }

    private static long Key(int x, int y) => ((long)x << 32) | (uint)y;
    private static void Unpack(long key, out int x, out int y)
    {
        x = (int)(key >> 32);
        y = (int)(key & 0xFFFFFFFF);
    }
}