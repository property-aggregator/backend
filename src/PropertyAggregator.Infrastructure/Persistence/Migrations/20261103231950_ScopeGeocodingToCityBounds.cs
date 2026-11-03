using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeGeocodingToCityBounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cached and stored results came from unbounded queries that could resolve to a street in another city.
            migrationBuilder.Sql("""TRUNCATE TABLE "geocodingCache";""");
            migrationBuilder.Sql("""
                UPDATE "properties"
                SET "location" = NULL, "locationPrecision" = NULL, "geocodedAt" = NULL
                WHERE "location" IS NOT NULL OR "geocodedAt" IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "query",
                table: "geocodingCache",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<double>(
                name: "east",
                table: "geocodingCache",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "north",
                table: "geocodingCache",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "placeRank",
                table: "geocodingCache",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "south",
                table: "geocodingCache",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "west",
                table: "geocodingCache",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""TRUNCATE TABLE "geocodingCache";""");

            migrationBuilder.DropColumn(
                name: "east",
                table: "geocodingCache");

            migrationBuilder.DropColumn(
                name: "north",
                table: "geocodingCache");

            migrationBuilder.DropColumn(
                name: "placeRank",
                table: "geocodingCache");

            migrationBuilder.DropColumn(
                name: "south",
                table: "geocodingCache");

            migrationBuilder.DropColumn(
                name: "west",
                table: "geocodingCache");

            migrationBuilder.AlterColumn<string>(
                name: "query",
                table: "geocodingCache",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400);
        }
    }
}
