# 🌍 Multiplayer World Server (Unity + SignalR)

Server-side authoritative world simulation for a tile-based survival / automation game.

This server is responsible for:

* procedural world generation
* multiplayer tile synchronization
* anti-spam protection (player cooldown)
* per-tile modification cooldown
* persistent player identity without JWT (guest-based)

---

## ⚙️ Architecture Overview

### Server-Authoritative Model

Clients **do not modify the world directly**.

Instead, they:

1. send `TryReplaceTile(requestId, x, y, id)`
2. server validates:

   * player cooldown
   * tile cooldown
   * world bounds
3. server responds:

   * `TileChangeResult` (ACK / NACK)
4. if accepted → server broadcasts:

   * `TileUpdate(x, y, id)`

`TileUpdate` is the **single source of truth**.

---

## 👤 Player Identification (Guest ID)

Authentication is implemented **without JWT** using a stable `guestId`:

* generated on client (UUID)
* stored locally (PlayerPrefs)
* sent during handshake:

```
Hello(guestId)
```

Server maps:

```
ConnectionId → guestId
guestId → PlayerState
```

This prevents cooldown reset abuse on reconnect.

---

## 🧱 World System

World is:

* finite (bounded)
* divided into chunks
* procedurally generated
* fully server-side

### Chunk Storage Model

Each chunk stores:

* tile IDs
* tile modification cooldowns

```
ChunkState
 ├── int[] Ids
 └── long[] NextAllowedTicksUtc
```

This allows:

* O(1) tile access
* per-tile cooldown
* reduced memory usage vs Dictionary<(x,y)>

---

## 🔒 Cooldown System

### Player Cooldown

Limits how often a player can attempt tile modification.

```
PlayerState.NextAllowedTileChangeTicksUtc
```

Prevents network spam.

---

### Tile Cooldown

Each tile has its own modification cooldown:

```
Chunk.NextAllowedTicksUtc[idx]
```

Prevents:

* grief spam
* race conditions
* multi-player override conflicts

Cooldown check and tile modification are applied **atomically** using chunk-level locking.

---

## 🌐 World Bounds

World size is limited via config:

```
worldSizeInChunks * chunkSize
```

Out-of-bounds modification attempts are rejected server-side.

---

## 🚀 World Pregeneration

All chunks are pre-generated on server startup:

```
WorldService.PreGenerateAllChunks()
```

Benefits:

* eliminates runtime generation lag
* stabilizes memory usage
* ensures deterministic world layout

---

## 📄 Configuration (YAML)

Server uses `config.yml`

Example:

```yaml
world:
  viewDistance: 3
  chunkSize: 4
  worldSizeInChunks: 64
  playerTileCooldownMs: 1000
  tileCooldownMs: 3000
```

---

## 🧩 Tech Stack

* ASP.NET Core
* SignalR
* YAML config
* Server-authoritative multiplayer model

---

## 🛡️ Multiplayer Safety

Implemented:

* reconnect-safe player identity
* per-player rate limiting
* per-tile modification cooldown
* atomic world updates
* server-side bounds validation

---

## 📡 Client Flow

```
Connect →
NeedHello →
Hello(guestId) →
GameConfig →

Click →
TryReplaceTile →

TileChangeResult →
(if ok) wait for TileUpdate
```

Clients should always treat `TileUpdate` as the final world state.

---
