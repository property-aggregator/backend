using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "priceStatistics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    propertyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    marketType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    areaM2Range = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    avgPricePerM2 = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    medianPricePerM2 = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sampleSize = table.Column<int>(type: "integer", nullable: false),
                    calculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_priceStatistics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sourcePortal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sourceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sourceUrl = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    pricePerM2 = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    areaM2 = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    rooms = table.Column<int>(type: "integer", nullable: true),
                    floor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    propertyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    marketType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sellerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    location = table.Column<Point>(type: "geography(Point, 4326)", nullable: true),
                    mainImageUrl = table.Column<string>(type: "text", nullable: true),
                    isDeal = table.Column<bool>(type: "boolean", nullable: false),
                    dealDiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    firstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    archivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_properties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "propertyPriceHistory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    propertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_propertyPriceHistory", x => x.id);
                    table.ForeignKey(
                        name: "fK_propertyPriceHistory_properties_propertyId",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_province_city_district_propertyType_transac~",
                table: "priceStatistics",
                columns: new[] { "province", "city", "district", "propertyType", "transactionType", "marketType", "areaM2Range" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "iX_properties_city_propertyType_transactionType",
                table: "properties",
                columns: new[] { "city", "propertyType", "transactionType" },
                filter: "\"archivedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_properties_deals_firstSeenAt",
                table: "properties",
                column: "firstSeenAt",
                filter: "\"isDeal\" AND \"archivedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "iX_properties_location",
                table: "properties",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "iX_properties_sourcePortal_lastSeenAt",
                table: "properties",
                columns: new[] { "sourcePortal", "lastSeenAt" },
                filter: "\"archivedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "iX_properties_sourcePortal_sourceId",
                table: "properties",
                columns: new[] { "sourcePortal", "sourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_propertyPriceHistory_propertyId_createdAt",
                table: "propertyPriceHistory",
                columns: new[] { "propertyId", "createdAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "priceStatistics");

            migrationBuilder.DropTable(
                name: "propertyPriceHistory");

            migrationBuilder.DropTable(
                name: "properties");
        }
    }
}
