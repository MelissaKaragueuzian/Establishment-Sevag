using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_Idea.Migrations
{
    /// <inheritdoc />
    public partial class AddChandelierAnimationImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroLampOffPath",
                table: "CompanyInfos",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroLampOnPath",
                table: "CompanyInfos",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroLampOffPath",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroLampOnPath",
                table: "CompanyInfos");
        }
    }
}
