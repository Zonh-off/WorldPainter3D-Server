using Microsoft.AspNetCore.SignalR;

namespace Server;

[Serializable]
public class GameConfigResponse
{
    public int viewDistance { get; set; }
    public int chunkSize { get; set; }
}

[Serializable]
public class TileDto
{
    public int x { get; set; }
    public int y { get; set; }
    public int id { get; set; }
}

public class GameHub : Hub
{
    private readonly PlayerService _playerService;
    private readonly WorldService _worldService;
    
    private const int ViewDistance = 3;
    private const  int ChunkSize = 4;
    
    public GameHub(PlayerService playerService, WorldService worldService)
    {
        _playerService = playerService;
        _worldService = worldService;
    }

    public override async Task OnConnectedAsync()
    {
        string playerId = Context.ConnectionId;
        Console.WriteLine($"Player connected: {playerId}");
        _playerService.RegisterPlayer(playerId);
        
        await Clients.Caller.SendAsync("GameConfig", new GameConfigResponse
        {
            viewDistance = ViewDistance,
            chunkSize = ChunkSize
        });
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string playerId = Context.ConnectionId;
        Console.WriteLine($"Player disconnected: {playerId}");
        _playerService.UnregisterPlayer(playerId);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task RequestChunk(int x, int y)
    {
        var tiles = _worldService.GetChunkTiles(x, y);

        await Clients.Caller.SendAsync("ChunkData", x, y, tiles);
    }
    
    public async Task ReplaceTile(int x, int y, int id)
    {
        string playerId = Context.ConnectionId;

        if (!_playerService.CanChangeTile(playerId))
            return;

        _worldService.SetTile(x, y, id);
        
        await Clients.All.SendAsync("TileUpdate", x, y, id);
    }
}