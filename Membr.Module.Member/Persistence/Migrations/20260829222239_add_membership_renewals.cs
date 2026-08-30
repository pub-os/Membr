using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Membr.Module.Member.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_membership_renewals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "membership_renewals",
                schema: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    membership_id = table.Column<int>(type: "integer", nullable: false),
                    renewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    previous_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    new_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membership_renewals", x => x.id);
                    table.ForeignKey(
                        name: "fk_membership_renewals_memberships_membership_id",
                        column: x => x.membership_id,
                        principalSchema: "members",
                        principalTable: "memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_membership_renewals_membership_id",
                schema: "members",
                table: "membership_renewals",
                column: "membership_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membership_renewals",
                schema: "members");
        }
    }
}
