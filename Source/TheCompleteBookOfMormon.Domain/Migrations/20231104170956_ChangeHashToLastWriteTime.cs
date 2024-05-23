using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheCompleteBookOfMormon.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ChangeHashToLastWriteTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Pages");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWrittenUtc",
                table: "Pages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWrittenUtc",
                table: "Pages");

            migrationBuilder.AddColumn<byte[]>(
                name: "Hash",
                table: "Pages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
