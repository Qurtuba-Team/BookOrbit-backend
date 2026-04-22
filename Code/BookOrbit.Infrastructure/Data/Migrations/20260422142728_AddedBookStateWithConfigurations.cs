using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedBookStateWithConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Books",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Pending");
        }
    }
}
