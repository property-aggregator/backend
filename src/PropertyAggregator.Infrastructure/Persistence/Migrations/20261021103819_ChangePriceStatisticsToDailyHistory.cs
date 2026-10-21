using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangePriceStatisticsToDailyHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics");

            migrationBuilder.DropColumn(
                name: "calculatedAt",
                table: "priceStatistics");

            migrationBuilder.AddColumn<DateOnly>(
                name: "calculatedOn",
                table: "priceStatistics",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_calculatedOn",
                table: "priceStatistics",
                column: "calculatedOn");

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics",
                columns: new[] { "city", "district", "propertyType", "transactionType", "marketType", "areaM2Range", "calculatedOn" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_priceStatistics_calculatedOn",
                table: "priceStatistics");

            migrationBuilder.DropIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics");

            migrationBuilder.DropColumn(
                name: "calculatedOn",
                table: "priceStatistics");

            migrationBuilder.AddColumn<DateTime>(
                name: "calculatedAt",
                table: "priceStatistics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics",
                columns: new[] { "city", "district", "propertyType", "transactionType", "marketType", "areaM2Range" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
