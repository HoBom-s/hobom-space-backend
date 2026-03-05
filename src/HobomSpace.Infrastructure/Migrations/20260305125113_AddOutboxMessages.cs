using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HobomSpace.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOutboxMessages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "space",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                event_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                payload = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                retry_count = table.Column<int>(type: "integer", nullable: false),
                sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                last_error = table.Column<string>(type: "text", nullable: true),
                version = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_event_id",
            schema: "space",
            table: "outbox_messages",
            column: "event_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_event_type_status",
            schema: "space",
            table: "outbox_messages",
            columns: new[] { "event_type", "status" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "space");
    }
}
