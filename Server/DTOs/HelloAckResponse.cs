namespace Server.DTOs;

[Serializable]
public struct HelloAckResponse
{
    public bool ok { get; set; }
    public string guestId { get; set; }
}