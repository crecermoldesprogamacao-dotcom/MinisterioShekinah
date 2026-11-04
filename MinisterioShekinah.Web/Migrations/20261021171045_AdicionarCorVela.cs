using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinisterioShekinah.Web.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCorVela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nome",
                table: "VelasVirtuais");

            migrationBuilder.AddColumn<string>(
                name: "CorVela",
                table: "VelasVirtuais",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorVela",
                table: "VelasVirtuais");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "VelasVirtuais",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
