using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HobomSpace.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "space");

        migrationBuilder.CreateTable(
            name: "spaces",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                key = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_spaces", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "pages",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                space_id = table.Column<long>(type: "bigint", nullable: false),
                parent_page_id = table.Column<long>(type: "bigint", nullable: true),
                title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_pages", x => x.id);
                table.ForeignKey(
                    name: "fk_pages_pages_parent_page_id",
                    column: x => x.parent_page_id,
                    principalSchema: "space",
                    principalTable: "pages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_pages_spaces_space_id",
                    column: x => x.space_id,
                    principalSchema: "space",
                    principalTable: "spaces",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_pages_parent_page_id",
            schema: "space",
            table: "pages",
            column: "parent_page_id");

        migrationBuilder.CreateIndex(
            name: "ix_pages_space_id",
            schema: "space",
            table: "pages",
            column: "space_id");

        migrationBuilder.CreateIndex(
            name: "ix_spaces_key",
            schema: "space",
            table: "spaces",
            column: "key",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pages",
            schema: "space");

        migrationBuilder.DropTable(
            name: "spaces",
            schema: "space");
    }
}
