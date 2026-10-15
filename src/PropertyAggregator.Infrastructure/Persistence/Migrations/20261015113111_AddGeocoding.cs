using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeocoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "geocodedAt",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "locationPrecision",
                table: "properties",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "geocodingCache",
                columns: table => new
                {
                    query = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_geocodingCache", x => x.query);
                });

            migrationBuilder.CreateIndex(
                name: "ix_properties_pendingGeocoding",
                table: "properties",
                column: "firstSeenAt",
                filter: "\"location\" IS NULL AND \"geocodedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geocodingCache");

            migrationBuilder.DropIndex(
                name: "ix_properties_pendingGeocoding",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "geocodedAt",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "locationPrecision",
                table: "properties");
        }
    }
}
