namespace Server.Services;

public enum TileChangeFail
{
    None = 0,
    PlayerCooldown,
    TileCooldown,
    OutOfBounds,
    InvalidTileId,
}