using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userFavorites",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    propertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_userFavorites", x => new { x.userId, x.propertyId });
                    table.ForeignKey(
                        name: "fK_userFavorites_properties_propertyId",
                        column: x => x.propertyId,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_userFavorites_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "iX_userFavorites_propertyId",
                table: "userFavorites",
                column: "propertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userFavorites");
        }
    }
}
