using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitPropertyTercCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tercCode",
                table: "properties",
                newName: "municipalityCode");

            migrationBuilder.RenameIndex(
                name: "iX_properties_tercCode",
                table: "properties",
                newName: "iX_properties_municipalityCode");

            migrationBuilder.AddColumn<string>(
                name: "countyCode",
                table: "properties",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "voivodeshipCode",
                table: "properties",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            // The renamed column held a TERC code of any level (2, 4 or 7 characters).
            migrationBuilder.Sql("""
                UPDATE "properties"
                SET "voivodeshipCode" = left("municipalityCode", 2),
                    "countyCode" = CASE WHEN length("municipalityCode") >= 4 THEN left("municipalityCode", 4) END,
                    "municipalityCode" = CASE WHEN length("municipalityCode") = 7 THEN "municipalityCode" END
                WHERE "municipalityCode" IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "iX_properties_countyCode",
                table: "properties",
                column: "countyCode");

            migrationBuilder.CreateIndex(
                name: "iX_properties_voivodeshipCode",
                table: "properties",
                column: "voivodeshipCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "properties"
                SET "municipalityCode" = COALESCE("municipalityCode", "countyCode", "voivodeshipCode")
                WHERE "voivodeshipCode" IS NOT NULL;
                """);

            migrationBuilder.DropIndex(
                name: "iX_properties_countyCode",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "iX_properties_voivodeshipCode",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "countyCode",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "voivodeshipCode",
                table: "properties");

            migrationBuilder.RenameColumn(
                name: "municipalityCode",
                table: "properties",
                newName: "tercCode");

            migrationBuilder.RenameIndex(
                name: "iX_properties_municipalityCode",
                table: "properties",
                newName: "iX_properties_tercCode");
        }
    }
}
