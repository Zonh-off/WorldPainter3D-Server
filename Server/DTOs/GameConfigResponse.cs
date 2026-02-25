namespace Server.DTOs;

[Serializable]
public struct GameConfigResponse
{
    public int viewDistance { get; set; }
    public int chunkSize { get; set; }
    public int worldSizeInChunks { get; set; }
    public long tileChangeCooldownMs { get; set; }
}