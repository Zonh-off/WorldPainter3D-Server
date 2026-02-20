using Server.Services;

namespace Server.DTOs;

[Serializable]
public struct TileChangeResponse
{
    public int requestId;
    public bool ok;
    public TileChangeFail fail;
    public long nextAllowedTicksUtc;
}
