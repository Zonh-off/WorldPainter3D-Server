namespace Server.Services;

public readonly record struct TileChangeResult(
    bool Ok,
    TileChangeFail Fail,
    long NextAllowedTicksUtc
);

public class WorldService(ServerConfig config)
{
    private sealed class ChunkState
    {
        public readonly object LockObj = new();

        public readonly int[] Ids;
        public readonly long[] NextAllowedTicksUtc;

        public ChunkState(int chunkSize)
        {
            int count = chunkSize * chunkSize;
            Ids = new int[count];
            NextAllowedTicksUtc = new long[count];
        }
    }
    
    private readonly Dictionary<(int cx, int cy), ChunkState> _chunks = new();

    private readonly int _chunkSize = config.World.ChunkSize;
    private readonly int _worldSizeChunks = config.World.WorldSizeInChunks;
    private readonly int _worldSizeTiles = config.World.WorldSizeInChunks * config.World.ChunkSize;
    private readonly long _tileCooldownTicks = TimeSpan.FromMilliseconds(config.World.TileCooldownMs).Ticks;
    
    public void PreGenerateAllChunks(IProgress<float>? progress = null)
    {
        int total = _worldSizeChunks * _worldSizeChunks;
        int done = 0;

        for (int cy = 0; cy < _worldSizeChunks; cy++)
        {
            for (int cx = 0; cx < _worldSizeChunks; cx++)
            {
                CreateChunkIfMissing(cx, cy);

                done++;
                if ((done & 63) == 0)
                    progress?.Report(done / (float)total);
            }
        }

        progress?.Report(1f);
    }

    public List<TileDto> GetChunkTiles(int chunkX, int chunkY)
    {
        if (!IsChunkInBounds(chunkX, chunkY))
            return new();

        var chunk = GetChunk(chunkX, chunkY);

        int startX = chunkX * _chunkSize;
        int startY = chunkY * _chunkSize;

        var result = new List<TileDto>(_chunkSize * _chunkSize);

        lock (chunk.LockObj)
        {
            for (int ly = 0; ly < _chunkSize; ly++)
            {
                for (int lx = 0; lx < _chunkSize; lx++)
                {
                    int idx = ToIndex(lx, ly);

                    result.Add(new TileDto
                    {
                        x = startX + lx,
                        y = startY + ly,
                        id = chunk.Ids[idx]
                    });
                }
            }
        }

        return result;
    }


    public TileChangeResult TrySetTile(int x, int y, int newId)
    {
        if (!IsTileInBounds(x, y))
            return new(false, TileChangeFail.OutOfBounds, 0);

        var now = DateTime.UtcNow.Ticks;

        var (cx, cy, lx, ly) = WorldToChunk(x, y);
        var chunk = GetChunk(cx, cy);
        int idx = ToIndex(lx, ly);

        lock (chunk.LockObj)
        {
            long next = chunk.NextAllowedTicksUtc[idx];
            if (now < next)
                return new(false, TileChangeFail.TileCooldown, next);

            chunk.Ids[idx] = newId;
            chunk.NextAllowedTicksUtc[idx] = now + _tileCooldownTicks;

            return new(true, TileChangeFail.None, chunk.NextAllowedTicksUtc[idx]);
        }
    }

    public int GetTile(int x, int y)
    {
        if (!IsTileInBounds(x, y))
            return 0;

        var (cx, cy, lx, ly) = WorldToChunk(x, y);
        var chunk = GetChunk(cx, cy);
        int idx = ToIndex(lx, ly);

        lock (chunk.LockObj)
            return chunk.Ids[idx];
    }
    
    public void SetTileNoCooldown(int x, int y, int id)
    {
        if (!IsTileInBounds(x, y))
            return;

        var (cx, cy, lx, ly) = WorldToChunk(x, y);
        var chunk = GetChunk(cx, cy);
        int idx = ToIndex(lx, ly);

        lock (chunk.LockObj)
            chunk.Ids[idx] = id;
    }
    
    private ChunkState GetChunk(int cx, int cy)
    {
        var key = (cx, cy);
        if (_chunks.TryGetValue(key, out var chunk))
            return chunk;

        return CreateChunkIfMissing(cx, cy);
    }

    private ChunkState CreateChunkIfMissing(int cx, int cy)
    {
        var key = (cx, cy);
        if (_chunks.TryGetValue(key, out var existing))
            return existing;

        var chunk = new ChunkState(_chunkSize);

        int startX = cx * _chunkSize;
        int startY = cy * _chunkSize;

        for (int ly = 0; ly < _chunkSize; ly++)
        {
            for (int lx = 0; lx < _chunkSize; lx++)
            {
                int wx = startX + lx;
                int wy = startY + ly;
                chunk.Ids[ToIndex(lx, ly)] = GenerateTile(wx, wy);
            }
        }

        _chunks[key] = chunk;
        return chunk;
    }

    #region Helpers
    private int ToIndex(int lx, int ly) => lx + ly * _chunkSize;

    private (int cx, int cy, int lx, int ly) WorldToChunk(int x, int y)
    {
        int cx = x / _chunkSize;
        int cy = y / _chunkSize;

        int lx = x - cx * _chunkSize;
        int ly = y - cy * _chunkSize;

        return (cx, cy, lx, ly);
    }

    private bool IsChunkInBounds(int chunkX, int chunkY)
        => chunkX >= 0 && chunkY >= 0 && chunkX < _worldSizeChunks && chunkY < _worldSizeChunks;

    private bool IsTileInBounds(int x, int y)
        => x >= 0 && y >= 0 && x < _worldSizeTiles && y < _worldSizeTiles;
    #endregion

    #region ProceduralGeneration
    private int GenerateTile(int x, int y)
    {
        float noise = GetNoise(x, y);

        if (noise < 0.45f) return 0;
        if (noise < 0.75f) return 1;
        return 2;
    }

    private const int WorldSeed = 12345;

    private float GetNoise(int x, int y)
    {
        unchecked
        {
            int hash = x;
            hash = hash * 374761393 + y * 668265263;
            hash = hash ^ (hash >> 13);
            hash = hash * 1274126177;
            hash ^= WorldSeed;

            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }
    #endregion
}
