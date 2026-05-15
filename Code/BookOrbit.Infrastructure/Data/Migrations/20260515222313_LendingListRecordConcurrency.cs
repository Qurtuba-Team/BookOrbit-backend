using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LendingListRecordConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LendingListRecords",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LendingListRecords");
        }
    }
}
