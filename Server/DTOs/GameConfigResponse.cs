namespace Server.DTOs;

[Serializable]
public struct GameConfigResponse
{
    public int viewDistance { get; set; }
    public int chunkSize { get; set; }
    public int worldSizeInChunks { get; set; }
    public long tileChangeCooldownMs { get; set; }
}

[Serializable]
public struct HelloDto
{
    public string guestId;
}

[Serializable]
public struct HelloAckDto
{
    public bool ok;
    public string guestId;
}