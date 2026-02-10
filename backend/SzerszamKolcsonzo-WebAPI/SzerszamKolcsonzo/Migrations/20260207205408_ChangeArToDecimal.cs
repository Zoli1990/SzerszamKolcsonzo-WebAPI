using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzerszamKolcsonzo.Migrations
{
    /// <inheritdoc />
    public partial class ChangeArToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NapiDij",
                table: "Eszkozok");

            migrationBuilder.AlterColumn<decimal>(
                name: "Vetelar",
                table: "Eszkozok",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "KiadasiAr",
                table: "Eszkozok",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 1,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 1500m, 19990m });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 2,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 1500m, 23000m });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 2000m, 43500m });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 5000m, 87999m });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 5,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 500m, 35000m });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 6,
                columns: new[] { "KiadasiAr", "Vetelar" },
                values: new object[] { 500m, 17000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Vetelar",
                table: "Eszkozok",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<int>(
                name: "KiadasiAr",
                table: "Eszkozok",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<decimal>(
                name: "NapiDij",
                table: "Eszkozok",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 1,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 1500, 0m, 19990 });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 2,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 1500, 0m, 23000 });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 2000, 0m, 43500 });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 5000, 0m, 87999 });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 5,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 500, 0m, 35000 });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 6,
                columns: new[] { "KiadasiAr", "NapiDij", "Vetelar" },
                values: new object[] { 500, 0m, 17000 });
        }
    }
}
