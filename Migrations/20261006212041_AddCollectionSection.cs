using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_Idea.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollectionKicker",
                table: "CompanyInfos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollectionText",
                table: "CompanyInfos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollectionTitle",
                table: "CompanyInfos",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectionKicker",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "CollectionText",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "CollectionTitle",
                table: "CompanyInfos");
        }
    }
}
