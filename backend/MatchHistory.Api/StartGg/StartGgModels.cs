namespace MatchHistory.Api.StartGg;

// Shapes of the JSON that start.gg sends back. Property names are matched
// case-insensitively, so "numAttendees" in JSON maps to NumAttendees here.

public sealed record GraphQlResponse<T>(T? Data, List<GraphQlError>? Errors);

public sealed record GraphQlError(string Message);

public sealed record VideogamesData(VideogameConnection? Videogames);

public sealed record VideogameConnection(List<Videogame> Nodes);

public sealed record Videogame(long Id, string Name, string? DisplayName);

public sealed record TournamentsData(TournamentPage? Tournaments);

public sealed record TournamentPage(PageInfo? PageInfo, List<Tournament> Nodes);

public sealed record PageInfo(int Total, int TotalPages);

public sealed record Tournament(
    long Id,
    string Name,
    string Slug,
    long? StartAt, // Unix seconds
    int? NumAttendees,
    List<TournamentEvent>? Events);

public sealed record TournamentEvent(long Id, string Name, int? NumEntrants);
