using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProvinceFromPriceStatistic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_priceStatistics_province_city_district_propertyType_transac~",
                table: "priceStatistics");

            migrationBuilder.DropColumn(
                name: "province",
                table: "priceStatistics");

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "priceStatistics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics",
                columns: new[] { "city", "district", "propertyType", "transactionType", "marketType", "areaM2Range" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_priceStatistics_city_district_propertyType_transactionType_~",
                table: "priceStatistics");

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "priceStatistics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "province",
                table: "priceStatistics",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "iX_priceStatistics_province_city_district_propertyType_transac~",
                table: "priceStatistics",
                columns: new[] { "province", "city", "district", "propertyType", "transactionType", "marketType", "areaM2Range" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
