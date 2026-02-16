using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schmeconomics.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Addedrefillcolumntotransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRefill",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRefill",
                table: "Transactions");
        }
    }
}
