using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Sistema_Central.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLogActividadEstructura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Detalle",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "MAC",
                table: "Logs",
                newName: "Mac");

            migrationBuilder.RenameColumn(
                name: "IP",
                table: "Logs",
                newName: "Ip");

            migrationBuilder.RenameColumn(
                name: "TipoEvento",
                table: "Logs",
                newName: "UserRut");

            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Logs",
                newName: "Timestamp");

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "Logs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Logs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsbSerial",
                table: "Logs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "UsbSerial",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "Mac",
                table: "Logs",
                newName: "MAC");

            migrationBuilder.RenameColumn(
                name: "Ip",
                table: "Logs",
                newName: "IP");

            migrationBuilder.RenameColumn(
                name: "UserRut",
                table: "Logs",
                newName: "TipoEvento");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Logs",
                newName: "FechaHora");

            migrationBuilder.AddColumn<string>(
                name: "Detalle",
                table: "Logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
