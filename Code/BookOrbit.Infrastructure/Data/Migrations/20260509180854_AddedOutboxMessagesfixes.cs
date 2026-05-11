using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedOutboxMessagesfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "OutboxMessages",
                type: "nvarchar(max)",
                maxLength: 15000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15000)",
                oldMaxLength: 15000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "OutboxMessages",
                type: "nvarchar(15000)",
                maxLength: 15000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 15000);
        }
    }
}
