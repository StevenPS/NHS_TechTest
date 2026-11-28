using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSpace.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261127165406_AddUserFullName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "AspNetUsers",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FullName",
            table: "AspNetUsers");
    }
}
