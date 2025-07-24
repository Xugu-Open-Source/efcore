using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.XuGu.IntegrationTests.Migrations
{
    public partial class InitDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "guid", nullable: false),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Author = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Publisher = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    YearOfPublication = table.Column<int>(type: "int", nullable: false),
                    ISBN = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Price = table.Column<decimal>(type: "decimal(38,17)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Description = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Value = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.ConfigurationId);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    ActionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Actor = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Details = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    PasswordHash = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    FullName = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Email = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    Phone = table.Column<string>(type: "varchar", nullable: true)
                        .Annotation("XuGu:CharSet", "gb2312"),
                    UserType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "StockAlerts",
                columns: table => new
                {
                    StockAlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: false),
                    AlertDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BookId1 = table.Column<Guid>(type: "guid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAlerts", x => x.StockAlertId);
                    table.ForeignKey(
                        name: "FK_StockAlerts_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    BorrowRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    BorrowDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<Guid>(type: "guid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.BorrowRecordId);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    FineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("XuGu:ValueGenerationStrategy", XGValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(type: "decimal(38,17)", nullable: false),
                    DateIssued = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    BorrowRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.FineId);
                    table.ForeignKey(
                        name: "FK_Fines_BorrowRecords_BorrowRecordId",
                        column: x => x.BorrowRecordId,
                        principalTable: "BorrowRecords",
                        principalColumn: "BorrowRecordId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "guid", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    FineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "FineId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("XuGu:CharSet", "gb2312");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_FineId",
                table: "BookCategories",
                column: "FineId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_BookId",
                table: "BorrowRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_UserId",
                table: "BorrowRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BorrowRecordId",
                table: "Fines",
                column: "BorrowRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_BookId1",
                table: "StockAlerts",
                column: "BookId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "StockAlerts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Fines");

            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
