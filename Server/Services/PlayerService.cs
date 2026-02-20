namespace Server.Services;

public class PlayerService(ServerConfig config)
{
    private class PlayerState
    {
        public long NextAllowedTileChangeTicks;
    }
    
    private readonly Dictionary<string, string> _connToUser = new();
    private readonly Dictionary<string, PlayerState> _users = new();
    private readonly long _cooldownTicks =
        TimeSpan.FromMilliseconds(config.Gameplay.TileChangeCooldownMs).Ticks;
    
    public void RegisterConnection(string connectionId)
    {
        Console.WriteLine($"Registering connection: {connectionId}");
    }

    public void UnregisterConnection(string connectionId)
    {
        Console.WriteLine($"Unregistering connection: {connectionId}");
        
        _connToUser.Remove(connectionId);
    }
    
    public bool BindGuestId(string connectionId, string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId) || guestId.Length > 64)
            return false;

        _connToUser[connectionId] = guestId;

        if (!_users.ContainsKey(guestId))
            _users[guestId] = new PlayerState();

        return true;
    }
    
    public bool TryGetUserId(string connectionId, out string userId)
        => _connToUser.TryGetValue(connectionId, out userId!);
    
    public bool TryConsumeTileChange(string userId, out long nextAllowed)
    {
        nextAllowed = 0;

        if (!_users.TryGetValue(userId, out var state))
            return false;

        var now = DateTime.UtcNow.Ticks;

        if (now < state.NextAllowedTileChangeTicks)
        {
            nextAllowed = state.NextAllowedTileChangeTicks;
            return false;
        }

        state.NextAllowedTileChangeTicks = now + _cooldownTicks;
        nextAllowed = state.NextAllowedTileChangeTicks;
        return true;
    }
}
