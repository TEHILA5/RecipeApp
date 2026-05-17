using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeApp.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Recipes");
        }
    }
}
