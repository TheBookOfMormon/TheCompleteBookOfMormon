using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheCompleteBookOfMormon.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddEditionFolderName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "Editions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "Editions");
        }
    }
}
