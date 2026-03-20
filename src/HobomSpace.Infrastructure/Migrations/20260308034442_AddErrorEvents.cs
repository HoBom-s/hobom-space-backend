using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HobomSpace.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddErrorEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "error_events",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                stack_trace = table.Column<string>(type: "text", nullable: true),
                screen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                error_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                nickname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_error_events", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_error_events_created_at",
            schema: "space",
            table: "error_events",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_error_events_error_type",
            schema: "space",
            table: "error_events",
            column: "error_type");

        migrationBuilder.CreateIndex(
            name: "ix_error_events_screen",
            schema: "space",
            table: "error_events",
            column: "screen");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "error_events",
            schema: "space");
    }
}
