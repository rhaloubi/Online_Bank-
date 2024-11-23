using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBank.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientID",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ClientID",
                table: "Accounts",
                column: "ClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Clients_ClientID",
                table: "Accounts",
                column: "ClientID",
                principalTable: "Clients",
                principalColumn: "ClientID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clients_ClientID",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ClientID",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ClientID",
                table: "Accounts");
        }
    }
}
