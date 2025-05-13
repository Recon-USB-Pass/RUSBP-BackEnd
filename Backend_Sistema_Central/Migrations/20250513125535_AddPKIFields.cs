using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Sistema_Central.Migrations
{
    /// <inheritdoc />
    public partial class AddPKIFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaAsignacion",
                table: "DispositivosUSB",
                newName: "FechaAlta");

            migrationBuilder.RenameColumn(
                name: "CertThumbprint",
                table: "DispositivosUSB",
                newName: "Thumbprint");

            migrationBuilder.AlterColumn<string>(
                name: "Mac",
                table: "Usuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "Usuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "SerialUsb",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Revoked",
                table: "DispositivosUSB",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialUsb",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Revoked",
                table: "DispositivosUSB");

            migrationBuilder.RenameColumn(
                name: "Thumbprint",
                table: "DispositivosUSB",
                newName: "CertThumbprint");

            migrationBuilder.RenameColumn(
                name: "FechaAlta",
                table: "DispositivosUSB",
                newName: "FechaAsignacion");

            migrationBuilder.AlterColumn<string>(
                name: "Mac",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ip",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
