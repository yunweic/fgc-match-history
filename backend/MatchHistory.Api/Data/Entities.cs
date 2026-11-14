namespace MatchHistory.Api.Data;

// Every Id below is start.gg's own id, not one we generate.
// That makes ingestion idempotent: fetching the same thing twice is an upsert.

public sealed class Player
{
    public long Id { get; set; }
    public required string GamerTag { get; set; }
    public string? Prefix { get; set; } // sponsor/team tag
    public string? CountryCode { get; set; }
}

public sealed class Tournament
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public DateTimeOffset? StartAt { get; set; }
    public bool IsOnline { get; set; }

    public List<Event> Events { get; set; } = [];
}

public sealed class Event
{
    public long Id { get; set; }
    public long TournamentId { get; set; }
    public long VideogameId { get; set; }
    public required string Name { get; set; }
    public int? NumEntrants { get; set; }

    /// <summary>Null until all of this event's sets have been ingested. Lets the backfill resume.</summary>
    public DateTimeOffset? SetsIngestedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public List<MatchSet> Sets { get; set; } = [];
}

/// <summary>
/// One completed set (match) between two entrants.
/// Ordered-pair rule: when both players have a start.gg id, PlayerA is the lower id.
/// So any matchup has exactly one representation, and a head-to-head is one index lookup.
/// </summary>
public sealed class MatchSet
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public string? RoundText { get; set; } // e.g. "Winners Final"
    public DateTimeOffset? CompletedAt { get; set; }

    // Null when the entrant has no start.gg account. The name is always kept.
    public long? PlayerAId { get; set; }
    public long? PlayerBId { get; set; }
    public required string PlayerAName { get; set; }
    public required string PlayerBName { get; set; }

    // Null when only a winner was reported.
    public int? PlayerAScore { get; set; }
    public int? PlayerBScore { get; set; }

    public long? WinnerId { get; set; }
    public bool IsDq { get; set; }

    public Event Event { get; set; } = null!;
    public Player? PlayerA { get; set; }
    public Player? PlayerB { get; set; }
}
