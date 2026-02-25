using Microsoft.AspNetCore.SignalR;
using Server.Contracts;
using Server.DTOs;
using Server.Services;

namespace Server.Hubs;

public class GameHub(ServerConfig config, PlayerService playerService, WorldService worldService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        playerService.RegisterConnection(Context.ConnectionId);
        
        await Clients.Caller.SendAsync("NeedHello");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        playerService.UnregisterConnection(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task Hello(string guestId)
    {
        var ok = playerService.BindGuestId(Context.ConnectionId, guestId);
        
        await Clients.Caller.SendAsync("HelloAck", new HelloAckResponse
        {
            ok = ok,
            guestId = guestId
        });

        if (!ok) return;
        
        await Clients.Caller.SendAsync(Messages.GameConfig, new GameConfigResponse
        {
            viewDistance = config.World.ViewDistance,
            chunkSize = config.World.ChunkSize,
            worldSizeInChunks = config.World.WorldSizeInChunks,
            tileChangeCooldownMs = config.Gameplay.TileChangeCooldownMs
        });
    }
    
    public async Task RequestChunk(int x, int y)
    {
        var tiles = worldService.GetChunkTiles(x, y);
        await Clients.Caller.SendAsync(Messages.ChunkData, x, y, tiles);
    }
    
    public async Task TryReplaceTile(int x, int y, int id)
    {
        if (!playerService.TryGetUserId(Context.ConnectionId, out var userId))
        {
            await Clients.Caller.SendAsync("TileChangeResult", new TileChangeResponse
            {
                ok = false,
                fail = TileChangeFail.PlayerCooldown,
                nextAllowedTicksUtc = 0
            });
            return;
        }
        
        // Can't replace
        if (!playerService.TryConsumeTileChange(userId, out var playerNext))
        {
            await Clients.Caller.SendAsync(Messages.TileChangeResult,
                                           new TileChangeResponse
                                           {
                                               ok = false,
                                               fail = TileChangeFail.PlayerCooldown,
                                               nextAllowedTicksUtc = playerNext
                                           });
            return;
        }

        // Can replace
        var result = worldService.TrySetTile(x, y, id);

        await Clients.Caller.SendAsync(Messages.TileChangeResult,
                                       new TileChangeResponse
                                       {
                                           ok = result.Ok,
                                           fail = result.Fail,
                                           nextAllowedTicksUtc = result.NextAllowedTicksUtc
                                       });

        if (!result.Ok)
            return;

        await Clients.All.SendAsync(Messages.TileUpdate, x, y, id);
    }
}