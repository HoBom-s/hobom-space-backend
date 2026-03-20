using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HobomSpace.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPhase1Features : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            schema: "space",
            table: "pages",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "deleted_by",
            schema: "space",
            table: "pages",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "labels",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                space_id = table.Column<long>(type: "bigint", nullable: false),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_labels", x => x.id);
                table.ForeignKey(
                    name: "fk_labels_spaces_space_id",
                    column: x => x.space_id,
                    principalSchema: "space",
                    principalTable: "spaces",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "page_labels",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                page_id = table.Column<long>(type: "bigint", nullable: false),
                label_id = table.Column<long>(type: "bigint", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_page_labels", x => x.id);
                table.ForeignKey(
                    name: "fk_page_labels_labels_label_id",
                    column: x => x.label_id,
                    principalSchema: "space",
                    principalTable: "labels",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_page_labels_pages_page_id",
                    column: x => x.page_id,
                    principalSchema: "space",
                    principalTable: "pages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_pages_deleted_at",
            schema: "space",
            table: "pages",
            column: "deleted_at");

        migrationBuilder.CreateIndex(
            name: "ix_labels_space_id_name",
            schema: "space",
            table: "labels",
            columns: new[] { "space_id", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_page_labels_label_id",
            schema: "space",
            table: "page_labels",
            column: "label_id");

        migrationBuilder.CreateIndex(
            name: "ix_page_labels_page_id_label_id",
            schema: "space",
            table: "page_labels",
            columns: new[] { "page_id", "label_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "page_labels",
            schema: "space");

        migrationBuilder.DropTable(
            name: "labels",
            schema: "space");

        migrationBuilder.DropIndex(
            name: "ix_pages_deleted_at",
            schema: "space",
            table: "pages");

        migrationBuilder.DropColumn(
            name: "deleted_at",
            schema: "space",
            table: "pages");

        migrationBuilder.DropColumn(
            name: "deleted_by",
            schema: "space",
            table: "pages");
    }
}
