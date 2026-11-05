using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "districtCode",
                table: "properties",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "locationMatchedAt",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "simcCode",
                table: "properties",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tercCode",
                table: "properties",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "locationDatasetVersions",
                columns: table => new
                {
                    version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    importedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_locationDatasetVersions", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    tercCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    simcCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    isCity = table.Column<bool>(type: "boolean", nullable: false),
                    isHidden = table.Column<bool>(type: "boolean", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    offerCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_locations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "iX_properties_districtCode",
                table: "properties",
                column: "districtCode");

            migrationBuilder.CreateIndex(
                name: "ix_properties_pendingLocationMatch",
                table: "properties",
                column: "firstSeenAt",
                filter: "\"locationMatchedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "iX_properties_simcCode",
                table: "properties",
                column: "simcCode");

            migrationBuilder.CreateIndex(
                name: "iX_properties_tercCode",
                table: "properties",
                column: "tercCode");

            migrationBuilder.CreateIndex(
                name: "iX_locations_normalizedName",
                table: "locations",
                column: "normalizedName");

            migrationBuilder.CreateIndex(
                name: "iX_locations_type_code",
                table: "locations",
                columns: new[] { "type", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "locationDatasetVersions");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropIndex(
                name: "iX_properties_districtCode",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "ix_properties_pendingLocationMatch",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "iX_properties_simcCode",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "iX_properties_tercCode",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "districtCode",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "locationMatchedAt",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "simcCode",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "tercCode",
                table: "properties");
        }
    }
}
