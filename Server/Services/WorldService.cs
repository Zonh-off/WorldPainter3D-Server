namespace Server.Services;

[Serializable]
public record struct TileDto
{
    public int x { get; set; }
    public int y { get; set; }
    public int id { get; set; }
}

public class WorldService(ServerConfig config)
{
    private readonly Dictionary<(int x, int y), int> _tiles = new();
    private readonly int _chunkSize = config.World.ChunkSize;
    private readonly int _worldSizeChunks = config.World.WorldSizeInChunks;
    private readonly int _worldSizeTiles = config.World.WorldSizeInChunks * config.World.ChunkSize;
    
    public List<TileDto> GetChunkTiles(int chunkX, int chunkY)
    {
        if (!IsChunkInBounds(chunkX, chunkY))
            return new();

        int startX = chunkX * _chunkSize;
        int startY = chunkY * _chunkSize;

        var result = new List<TileDto>(_chunkSize * _chunkSize);

        for (int lx = 0; lx < _chunkSize; lx++)
        for (int ly = 0; ly < _chunkSize; ly++)
        {
            int wx = startX + lx;
            int wy = startY + ly;

            result.Add(new TileDto
            {
                x = wx,
                y = wy,
                id = GetTile(wx, wy)
            });
        }

        return result;
    }
    
    private int GetTile(int x, int y)
    {
        if (!IsTileInBounds(x, y))
            return 0;

        if (_tiles.TryGetValue((x, y), out var id))
            return id;

        id = GenerateTile(x, y);
        _tiles[(x, y)] = id;
        return id;
    }

    public void SetTile(int x, int y, int id)
    {
        if (!IsTileInBounds(x, y))
            return;

        _tiles[(x, y)] = id;
    }

    private int GenerateTile(int x, int y)
    {
        float noise = GetNoise(x, y);

        if (noise < 0.45f)
            return 0;
        else if (noise < 0.75f)
            return 1;
        else
            return 2;
    }
    
    private bool IsChunkInBounds(int chunkX, int chunkY)
    {
        return chunkX >= 0 &&
            chunkY >= 0 &&
            chunkX < _worldSizeChunks &&
            chunkY < _worldSizeChunks;
    }
    
    private bool IsTileInBounds(int x, int y)
    {
        return x >= 0 &&
            y >= 0 &&
            x < _worldSizeTiles &&
            y < _worldSizeTiles;
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

            // Convert to 0..1 range
            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}
