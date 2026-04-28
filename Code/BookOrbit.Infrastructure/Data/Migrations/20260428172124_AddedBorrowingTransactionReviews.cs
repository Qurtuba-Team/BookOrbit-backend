using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedBorrowingTransactionReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BorrowingReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowingReviews_BorrowingTransactions_BorrowingTransactionId",
                        column: x => x.BorrowingTransactionId,
                        principalTable: "BorrowingTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowingReviews_Students_ReviewedStudentId",
                        column: x => x.ReviewedStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowingReviews_Students_ReviewerStudentId",
                        column: x => x.ReviewerStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingReviews_BorrowingTransactionId",
                table: "BorrowingReviews",
                column: "BorrowingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingReviews_ReviewedStudentId",
                table: "BorrowingReviews",
                column: "ReviewedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingReviews_ReviewerStudentId",
                table: "BorrowingReviews",
                column: "ReviewerStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingReviews");
        }
    }
}
