namespace Server.DTOs;

[Serializable]
public struct HelloRequest
{
    public string guestId { get; set; }
}