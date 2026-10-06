using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_Idea.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kicker = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Intro = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Step1Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Step1Text = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Step2Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Step2Text = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Step3Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Step3Text = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Quote = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    QuoteAuthor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Image1Path = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Image1Label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Image2Path = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Image2Label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Stat1Value = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Stat1Text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Stat2Value = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Stat2Text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutPages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutPages");
        }
    }
}
