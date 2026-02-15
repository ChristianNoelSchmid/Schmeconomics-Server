using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schmeconomics.Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedtransactiontoincludecreatorID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatorId",
                table: "Transactions",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_CreatorId",
                table: "Transactions",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_CreatorId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CreatorId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Transactions");
        }
    }
}
