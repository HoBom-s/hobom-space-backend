using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HobomSpace.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCommentAndPageVersion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "comments",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                page_id = table.Column<long>(type: "bigint", nullable: false),
                parent_comment_id = table.Column<long>(type: "bigint", nullable: true),
                content = table.Column<string>(type: "text", nullable: false),
                author = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_comments", x => x.id);
                table.ForeignKey(
                    name: "fk_comments_comments_parent_comment_id",
                    column: x => x.parent_comment_id,
                    principalSchema: "space",
                    principalTable: "comments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_comments_pages_page_id",
                    column: x => x.page_id,
                    principalSchema: "space",
                    principalTable: "pages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "page_versions",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                page_id = table.Column<long>(type: "bigint", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                edited_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_page_versions", x => x.id);
                table.ForeignKey(
                    name: "fk_page_versions_pages_page_id",
                    column: x => x.page_id,
                    principalSchema: "space",
                    principalTable: "pages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_comments_page_id",
            schema: "space",
            table: "comments",
            column: "page_id");

        migrationBuilder.CreateIndex(
            name: "ix_comments_parent_comment_id",
            schema: "space",
            table: "comments",
            column: "parent_comment_id");

        migrationBuilder.CreateIndex(
            name: "ix_page_versions_page_id_version",
            schema: "space",
            table: "page_versions",
            columns: new[] { "page_id", "version" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "comments",
            schema: "space");

        migrationBuilder.DropTable(
            name: "page_versions",
            schema: "space");
    }
}
