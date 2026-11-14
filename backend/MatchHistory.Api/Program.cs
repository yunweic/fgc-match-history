using System.Net.Http.Headers;
using MatchHistory.Api.Data;
using MatchHistory.Api.StartGg;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// The token lives in user-secrets (outside the repo), never in appsettings.json.
// Set it with: dotnet user-secrets set "StartGg:ApiToken" "<your token>"
builder.Services.AddHttpClient<StartGgClient>((services, http) =>
{
    var token = services.GetRequiredService<IConfiguration>()["StartGg:ApiToken"];
    if (string.IsNullOrWhiteSpace(token))
    {
        throw new StartGgException(
            "No start.gg token configured. Run: dotnet user-secrets set \"StartGg:ApiToken\" \"<your token>\"");
    }

    http.BaseAddress = new Uri("https://api.start.gg/gql/alpha");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
});

// Local Postgres from docker-compose.yml; the connection string is in appsettings.Development.json.
builder.Services.AddDbContext<MatchHistoryDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("MatchHistory"))
    .UseSnakeCaseNamingConvention());

var app = builder.Build();

// Turn start.gg failures into a readable 502 instead of a stack trace.
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (StartGgException ex)
    {
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapGet("/", () => "Match history API is running.");

// Step 1: find the game's id on start.gg, e.g. /api/startgg/videogames?name=King of Fighters XV
app.MapGet("/api/startgg/videogames", async (string name, StartGgClient startGg, CancellationToken ct) =>
    Results.Ok(await startGg.FindVideogamesAsync(name, ct)));

// Step 2: list past tournaments for that id, e.g. /api/startgg/tournaments?videogameId=123
app.MapGet("/api/startgg/tournaments", async (
    long videogameId, StartGgClient startGg, CancellationToken ct, int page = 1, int perPage = 25) =>
    Results.Ok(await startGg.GetPastTournamentsAsync(videogameId, page, perPage, ct)));

app.Run();
