using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookOrbit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedBorrowingTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BorrowingTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowingRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LenderStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowerStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookCopyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExpectedReturnDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualReturnDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowingTransactions_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowingTransactions_BorrowingRequests_BorrowingRequestId",
                        column: x => x.BorrowingRequestId,
                        principalTable: "BorrowingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowingTransactions_Students_BorrowerStudentId",
                        column: x => x.BorrowerStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowingTransactions_Students_LenderStudentId",
                        column: x => x.LenderStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_BookCopyId",
                table: "BorrowingTransactions",
                column: "BookCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_BorrowerStudentId",
                table: "BorrowingTransactions",
                column: "BorrowerStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_BorrowingRequestId",
                table: "BorrowingTransactions",
                column: "BorrowingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_LenderStudentId",
                table: "BorrowingTransactions",
                column: "LenderStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_State",
                table: "BorrowingTransactions",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingTransactions");
        }
    }
}
