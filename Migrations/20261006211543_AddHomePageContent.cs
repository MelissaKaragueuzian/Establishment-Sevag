using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_Idea.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroBadge",
                table: "CompanyInfos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroFeatures",
                table: "CompanyInfos",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroHighlight",
                table: "CompanyInfos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImagePath",
                table: "CompanyInfos",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeroProductId",
                table: "CompanyInfos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroText",
                table: "CompanyInfos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "CompanyInfos",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroBadge",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroFeatures",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroHighlight",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroImagePath",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroProductId",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroText",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "CompanyInfos");
        }
    }
}
