using Server.Services;

namespace Server.DTOs;

[Serializable]
public struct TileChangeResponse
{
    public bool ok { get; set; }
    public TileChangeFail fail { get; set; }
    public long nextAllowedTicksUtc { get; set; }
}
