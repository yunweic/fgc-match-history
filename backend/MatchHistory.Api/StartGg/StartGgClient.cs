using System.Net.Http.Json;
using System.Text.Json;

namespace MatchHistory.Api.StartGg;

/// <summary>
/// Thin wrapper over the start.gg GraphQL API (https://api.start.gg/gql/alpha).
/// Every call is a POST with a query string and a variables object.
/// </summary>
public sealed class StartGgClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string VideogamesQuery = """
        query Videogames($name: String!) {
          videogames(query: { perPage: 10, filter: { name: $name } }) {
            nodes { id name displayName }
          }
        }
        """;

    private const string TournamentsQuery = """
        query Tournaments($page: Int!, $perPage: Int!, $videogameIds: [ID]!) {
          tournaments(query: {
            page: $page
            perPage: $perPage
            sortBy: "startAt desc"
            filter: { past: true, videogameIds: $videogameIds }
          }) {
            pageInfo { total totalPages }
            nodes {
              id
              name
              slug
              startAt
              numAttendees
              events(filter: { videogameId: $videogameIds }) {
                id
                name
                numEntrants
              }
            }
          }
        }
        """;

    public async Task<IReadOnlyList<Videogame>> FindVideogamesAsync(string name, CancellationToken ct = default)
    {
        var data = await SendAsync<VideogamesData>(VideogamesQuery, new { name }, ct);
        return data.Videogames?.Nodes ?? [];
    }

    public async Task<TournamentPage> GetPastTournamentsAsync(
        long videogameId, int page = 1, int perPage = 25, CancellationToken ct = default)
    {
        var data = await SendAsync<TournamentsData>(
            TournamentsQuery,
            new { page, perPage, videogameIds = new[] { videogameId } },
            ct);

        return data.Tournaments ?? new TournamentPage(null, []);
    }

    private async Task<T> SendAsync<T>(string query, object variables, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("", new { query, variables }, Json, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new StartGgException(
                $"start.gg returned HTTP {(int)response.StatusCode}: {Truncate(body)}");
        }

        var payload = await response.Content.ReadFromJsonAsync<GraphQlResponse<T>>(Json, ct);

        // GraphQL reports query errors with HTTP 200 and an "errors" array.
        if (payload?.Errors is { Count: > 0 } errors)
        {
            throw new StartGgException(
                "start.gg query failed: " + string.Join("; ", errors.Select(e => e.Message)));
        }

        return payload is { Data: not null }
            ? payload.Data
            : throw new StartGgException("start.gg returned an empty response.");
    }

    private static string Truncate(string s) => s.Length <= 300 ? s : s[..300] + "...";
}

public sealed class StartGgException(string message) : Exception(message);
