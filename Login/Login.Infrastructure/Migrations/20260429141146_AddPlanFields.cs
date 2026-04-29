using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanType",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);  // 0 = PlanType.None

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrialStartedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlanExpiresAt",
                table: "Users");
        }
    }
}
