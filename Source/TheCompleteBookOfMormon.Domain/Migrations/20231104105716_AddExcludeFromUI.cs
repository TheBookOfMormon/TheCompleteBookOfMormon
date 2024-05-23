using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheCompleteBookOfMormon.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddExcludeFromUI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExcludeFromUI",
                table: "Editions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludeFromUI",
                table: "Editions");
        }
    }
}
