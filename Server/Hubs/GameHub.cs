using Microsoft.AspNetCore.SignalR;
using Server.Contracts;
using Server.Services;

namespace Server.Hubs;

[Serializable]
public record struct GameConfigResponse
{
    public int viewDistance { get; set; }
    public int chunkSize { get; set; }
    public int worldSizeInChunks { get; set; }
}

public class GameHub(ServerConfig config, PlayerService playerService, WorldService worldService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        string playerId = Context.ConnectionId;
        Console.WriteLine($"Player connected: {playerId}");
        playerService.RegisterPlayer(playerId);
        
        await Clients.Caller.SendAsync(Messages.GameConfig, new GameConfigResponse
        {
            viewDistance = config.World.ViewDistance,
            chunkSize = config.World.ChunkSize,
            worldSizeInChunks = config.World.WorldSizeInChunks
        });
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string playerId = Context.ConnectionId;
        Console.WriteLine($"Player disconnected: {playerId}");
        playerService.UnregisterPlayer(playerId);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task RequestChunk(int x, int y)
    {
        var tiles = worldService.GetChunkTiles(x, y);
        await Clients.Caller.SendAsync(Messages.ChunkData, x, y, tiles);
    }
    
    public async Task ReplaceTile(int x, int y, int id)
    {
        string playerId = Context.ConnectionId;

        if (!playerService.CanChangeTile(playerId))
            return;

        worldService.SetTile(x, y, id);
        
        await Clients.All.SendAsync(Messages.TileUpdate, x, y, id);
    }
}