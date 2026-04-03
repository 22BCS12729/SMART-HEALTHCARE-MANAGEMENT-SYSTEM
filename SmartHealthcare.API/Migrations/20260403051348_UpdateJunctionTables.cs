using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHealthcare.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJunctionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(672));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(4250));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(4257));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(4261));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(4263));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 13, 46, 674, DateTimeKind.Utc).AddTicks(4266));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(4643));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(6042));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(6046));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(6048));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(6051));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 3, 5, 5, 33, 604, DateTimeKind.Utc).AddTicks(6053));
        }
    }
}
