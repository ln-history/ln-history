# ln-history-api — agent guide

Read-only REST + WebSocket API over the `lnhistory` Postgres gossip DB and a Bitcoin Core node.
.NET 10 / ASP.NET Core, Dapper + Npgsql, System.Text.Json. Deployed as `ghcr.io/ln-history/ln-history-api`
behind nginx at `https://api.ln-history.info`.

## Build / run

The .NET 10 SDK is at **`~/.dotnet`** (system `dotnet` in `/usr/local/share/dotnet` is 8.x). Prefix:

```bash
export DOTNET_ROOT=$HOME/.dotnet PATH=$HOME/.dotnet:$PATH
dotnet build LN-history-api.sln
# Development ⇒ auth + rate limits OFF:
ASPNETCORE_ENVIRONMENT=Development dotnet run --project LN-history.Startup --no-launch-profile --urls http://127.0.0.1:5199
```

## Layering (never point dependencies up)

`LN-history.Api` → `LN-history.Core` → `LN-history.Data` / `Bitcoin.Data` → `LN-History.Model` (leaf).
- **Api**: controllers, DTOs (`Dto/`), domain→DTO mappers (`Mapping/`), auth/rate-limit middleware+filters. No SQL/HTTP.
- **Core**: services returning **domain models, never DTOs**. Orchestrates data stores. No SQL/HTTP.
- **Data**: Dapper data stores over a singleton `NpgsqlDataSource` (short-lived conn per query). Hand-written SQL.
- **Model**: dependency-free domain types + enums. Everyone references it.

## Conventions that bite if ignored

- **snake_case JSON**, enums as snake_case strings, `byte[]`→base64, **nulls emitted** (stable schema).
  Digit-containing names need explicit `[JsonPropertyName]` (`node_id_1`, not `node_id1`).
- `raw_gossip` is the universal name for raw bytes. `scid` always emitted as both number + `scid_str`.
- **DB cost gate**: EXPLAIN anything touching the big tables (`channel_updates` 147 GB, `gossip_inventory`,
  `node_addresses`); >1M planner cost ⇒ warn before running. See the `ln-history-database` skill.
- Join gossip tables by **`internal_id`** (8-byte int), not `gossip_id` (65-byte hex).

## Auth & rate limiting (see the ln-history-api skill for the full design)

Multi-tenant API keys with cost-weighted limiting; the `api` schema in lnhistory Postgres is the store.
Enforcement is **in-memory** (authoritative) with an async flush to `api.usage` (durable ledger).
- Each endpoint has a static **weight** (`api.endpoint_weights`, admin-tunable). A request pre-charges its
  weight against the key's **daily budget** (default 1000 units, resets 00:00 UTC) *before* the handler runs —
  over-budget ⇒ 429, DB untouched. Refund on handler error. Plus a per-key burst token bucket.
- `role=admin` bypasses all limits. The config `ApiKey` stays valid as an **emergency unlimited admin**
  (anti-lockout). Keys stored as **sha256 only**, cached in memory, reloaded every ~30s.
- WebSocket stream: per-key connection cap; only the `?since` backfill is charged (live tail is in-memory,
  zero DB load).
- **Keys are managed out-of-band** (`ln_api_admin.py` → `api` schema over psql). The public API is read-only;
  there are no key-management endpoints.

## Migrations

`Documentation/migrations/*.sql`, applied with the DB migration role (admin). Additive schema (like the
`api` schema) is low-risk; triggers on ingest tables (the gossip-stream NOTIFY trigger) affect the hot path —
**disable them during bulk imports**. After any rename-swap, re-`GRANT` (grants follow the OID, not the name).

## Docs

- `Documentation/REFACTOR_PLAN.md` — the agreed 3-layer design (source of truth).
- `~/.config/ai/skills/personal/ln-history/ln-history-api/SKILL.md` — full reference (routes, DTOs, auth, CLIs).
- The `ln-history-database` skill — DB schema, indexes, ingestion pipeline, cost-gate protocol.
