using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheCompleteBookOfMormon.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemovePageScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_PageScans_ScanId",
                table: "Pages");

            migrationBuilder.DropTable(
                name: "PageScans");

            migrationBuilder.DropIndex(
                name: "IX_Pages_ScanId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "LastWrittenUtc",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "ScanId",
                table: "Pages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Pages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWrittenUtc",
                table: "Pages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ScanId",
                table: "Pages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PageScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageScans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ScanId",
                table: "Pages",
                column: "ScanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_PageScans_ScanId",
                table: "Pages",
                column: "ScanId",
                principalTable: "PageScans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
