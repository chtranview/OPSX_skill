using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TpiUbikeAreaRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sno = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Sna = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Snaen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sarea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sareaen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ar = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Aren = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableRentBikes = table.Column<int>(type: "int", nullable: false),
                    AvailableReturnBikes = table.Column<int>(type: "int", nullable: false),
                    Act = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Mday = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SrcUpdateTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InfoTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InfoDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TpiUbikeAreaRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TpiUbikeAreaRecords_CollectedTime",
                table: "TpiUbikeAreaRecords",
                column: "CollectedTime");

            migrationBuilder.CreateIndex(
                name: "IX_TpiUbikeAreaRecords_Sarea",
                table: "TpiUbikeAreaRecords",
                column: "Sarea");

            migrationBuilder.CreateIndex(
                name: "IX_TpiUbikeAreaRecords_SessionId",
                table: "TpiUbikeAreaRecords",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TpiUbikeAreaRecords");
        }
    }
}
