using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Sistema_Central.Migrations
{
    /// <inheritdoc />
    public partial class UsbNullableUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Rut",
                table: "Usuarios");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "DispositivosUSB",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "DispositivosUSB",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Rut",
                table: "Usuarios",
                column: "Rut",
                unique: true);
        }
    }
}
