using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseHub.Infrastructure.Persistence.MySql.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Plans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PriceAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                PriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Interval = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Plans", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                TenantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                PlanId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CurrentPeriodEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_TenantId",
            table: "Subscriptions",
            column: "TenantId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Plans");

        migrationBuilder.DropTable(
            name: "Subscriptions");
    }
}
