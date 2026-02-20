namespace Server.Services;

public class PlayerService
{
    private class PlayerState
    {
        public DateTime LastTileChange = DateTime.MinValue;
    }

    private readonly Dictionary<string, PlayerState> _players = new();
    private readonly TimeSpan _tileCooldown = TimeSpan.FromMilliseconds(1000);

    public void RegisterPlayer(string playerId)
    {
        if (!_players.ContainsKey(playerId))
            _players[playerId] = new PlayerState();
    }

    public void UnregisterPlayer(string playerId)
    {
        _players.Remove(playerId);
    }
    
    public bool CanChangeTile(string playerId)
    {
        if (!_players.TryGetValue(playerId, out var state))
            return false;

        if (DateTime.UtcNow - state.LastTileChange < _tileCooldown)
            return false;

        state.LastTileChange = DateTime.UtcNow;
        return true;
    }
}
