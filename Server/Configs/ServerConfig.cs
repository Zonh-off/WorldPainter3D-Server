namespace Server;

public sealed class ServerConfig
{
    public WorldConfig World { get; set; } = new();
    public GameplayConfig Gameplay { get; set; } = new();

    public sealed class WorldConfig
    {
        public int ViewDistance { get; set; } = 3;
        public int ChunkSize { get; set; } = 4;
        public int WorldSizeInChunks { get; set; } = 64;
    }

    public sealed class GameplayConfig
    {
        public int TileChangeCooldownMs { get; set; } = 0; // приклад
    }
}