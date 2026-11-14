using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchHistory.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    gamer_tag = table.Column<string>(type: "text", nullable: false),
                    prefix = table.Column<string>(type: "text", nullable: true),
                    country_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tournaments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_online = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tournaments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    tournament_id = table.Column<long>(type: "bigint", nullable: false),
                    videogame_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    num_entrants = table.Column<int>(type: "integer", nullable: true),
                    sets_ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_tournaments_tournament_id",
                        column: x => x.tournament_id,
                        principalTable: "tournaments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    event_id = table.Column<long>(type: "bigint", nullable: false),
                    round_text = table.Column<string>(type: "text", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    player_a_id = table.Column<long>(type: "bigint", nullable: true),
                    player_b_id = table.Column<long>(type: "bigint", nullable: true),
                    player_a_name = table.Column<string>(type: "text", nullable: false),
                    player_b_name = table.Column<string>(type: "text", nullable: false),
                    player_a_score = table.Column<int>(type: "integer", nullable: true),
                    player_b_score = table.Column<int>(type: "integer", nullable: true),
                    winner_id = table.Column<long>(type: "bigint", nullable: true),
                    is_dq = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sets", x => x.id);
                    table.CheckConstraint("ck_sets_player_order", "player_a_id IS NULL OR player_b_id IS NULL OR player_a_id < player_b_id");
                    table.ForeignKey(
                        name: "fk_sets_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sets_players_player_a_id",
                        column: x => x.player_a_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sets_players_player_b_id",
                        column: x => x.player_b_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_tournament_id",
                table: "events",
                column: "tournament_id");

            migrationBuilder.CreateIndex(
                name: "ix_players_gamer_tag",
                table: "players",
                column: "gamer_tag")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_sets_event_id",
                table: "sets",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_sets_player_a_id_player_b_id_completed_at",
                table: "sets",
                columns: new[] { "player_a_id", "player_b_id", "completed_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_sets_player_b_id",
                table: "sets",
                column: "player_b_id");

            migrationBuilder.CreateIndex(
                name: "ix_tournaments_start_at",
                table: "tournaments",
                column: "start_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sets");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "tournaments");
        }
    }
}
