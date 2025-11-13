using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Sebben_que_é_agenda.Migrations
{
    /// <inheritdoc />
    public partial class AddTipUsuarioToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipUsuario",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipUsuario",
                table: "Usuarios");
        }
    }
}
