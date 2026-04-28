using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPointTransactionsAndBorrowingRequestEventTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BorrowingTransactionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingTransactionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowingTransactionEvents_BorrowingTransactions_BorrowingTransactionId",
                        column: x => x.BorrowingTransactionId,
                        principalTable: "BorrowingTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactionEvents_BorrowingTransactionId",
                table: "BorrowingTransactionEvents",
                column: "BorrowingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactionEvents_State",
                table: "BorrowingTransactionEvents",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_BorrowingReviewId",
                table: "PointTransactions",
                column: "BorrowingReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_Reason",
                table: "PointTransactions",
                column: "Reason");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_StudentId",
                table: "PointTransactions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingTransactionEvents");

            migrationBuilder.DropTable(
                name: "PointTransactions");
        }
    }
}
