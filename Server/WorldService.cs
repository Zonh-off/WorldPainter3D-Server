namespace Server;

public class WorldService
{
    public int ChunkSize { get; } = 4;
    
    private Dictionary<(int x, int y), int> _tiles = new();

    public List<TileDto> GetChunkTiles(int chunkX, int chunkY)
    {
        int startX = chunkX * ChunkSize;
        int startY = chunkY * ChunkSize;

        var result = new List<TileDto>(ChunkSize * ChunkSize);

        for (int lx = 0; lx < ChunkSize; lx++)
        for (int ly = 0; ly < ChunkSize; ly++)
        {
            int wx = startX + lx;
            int wy = startY + ly; 

            result.Add(new TileDto()
            {
                x =  wx,
                y = wy,
                id = GetTile(wx, wy)
            });
        }

        return result;
    }
    
    private int GetTile(int x, int y)
    {
        if (_tiles.TryGetValue((x, y), out var id))
            return id;

        id = GenerateTile(x, y);
        _tiles[(x, y)] = id;
        return id;
    }

    public void SetTile(int x, int y, int id)
    {
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
