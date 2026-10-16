using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirationCheckRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expirationCheckRequestedAt",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expirationCheckRequestedAt",
                table: "properties");
        }
    }
}
