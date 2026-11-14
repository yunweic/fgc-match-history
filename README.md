# FGC Match History

Head-to-head match history for fighting game tournament players, built on
[start.gg](https://www.start.gg) data.

The goal: a caster has about thirty seconds before a match starts. Type two
player names and see whether they have played before, who won, and where.

The first supported game is **The King of Fighters XV**.

## Status

Early work in progress.

| Piece | State |
|---|---|
| start.gg client (game and tournament lookups) | Working |
| Postgres schema (players, tournaments, events, sets) | Working |
| Ingestion of tournaments and sets from start.gg | Not started |
| Player search and head-to-head API | Not started |
| Web UI | Not started |

## Design

- **start.gg ids are primary keys.** Fetching the same tournament or set twice
  is an upsert, so ingestion is idempotent and can be re-run safely.
- **Players are keyed on start.gg's player id**, which stays the same across
  tag and sponsor changes.
- **Sets store their two players as an ordered pair.** Player A is always the
  lower id, enforced by a check constraint. Every matchup has exactly one
  representation, so a head-to-head is a single index lookup on
  `(player_a_id, player_b_id, completed_at desc)`.
- **Everything is ingested; filtering happens at query time.** Small locals are
  noisy, but data that was never stored can't be recovered later.
- **Player search uses a trigram index** so partial names and typos still match.

## Stack

- C# / ASP.NET Core (.NET 11)
- EF Core with PostgreSQL 17
- React + TypeScript (planned)

## Running locally

You need the [.NET 11 SDK](https://dotnet.microsoft.com/download), Docker, and
a start.gg API token (create one in your start.gg developer settings).

Start the database:

```bash
docker compose up -d
```

Store your start.gg token in user-secrets, which keeps it outside the repository:

```bash
cd backend/MatchHistory.Api
dotnet user-secrets set "StartGg:ApiToken" "<your token>"
```

Apply the schema and run the API:

```bash
dotnet tool restore
dotnet ef database update
dotnet run
```

Then try:

- `http://localhost:5153/api/startgg/videogames?name=King of Fighters XV`
- `http://localhost:5153/api/startgg/tournaments?videogameId=36963`

The database credentials in `docker-compose.yml` are for local development only.
