using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Membr.Module.Member.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_membership_relationships_and_settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memberships_membership_types_membership_type_id",
                schema: "members",
                table: "memberships");

            migrationBuilder.AddColumn<int>(
                name: "member_id",
                schema: "members",
                table: "memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "duration_months",
                schema: "members",
                table: "membership_types",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "fixed_term_anchor_day",
                schema: "members",
                table: "membership_types",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "fixed_term_anchor_month",
                schema: "members",
                table: "membership_types",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "renewal_mode",
                schema: "members",
                table: "membership_types",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "membership_settings",
                schema: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    allow_multiple_memberships = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membership_settings", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "members",
                table: "membership_settings",
                column: "id",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "ix_memberships_member_id",
                schema: "members",
                table: "memberships",
                column: "member_id");

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_members_member_id",
                schema: "members",
                table: "memberships",
                column: "member_id",
                principalSchema: "members",
                principalTable: "members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_membership_types_membership_type_id",
                schema: "members",
                table: "memberships",
                column: "membership_type_id",
                principalSchema: "members",
                principalTable: "membership_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_memberships_members_member_id",
                schema: "members",
                table: "memberships");

            migrationBuilder.DropForeignKey(
                name: "fk_memberships_membership_types_membership_type_id",
                schema: "members",
                table: "memberships");

            migrationBuilder.DropTable(
                name: "membership_settings",
                schema: "members");

            migrationBuilder.DropIndex(
                name: "ix_memberships_member_id",
                schema: "members",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "member_id",
                schema: "members",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "duration_months",
                schema: "members",
                table: "membership_types");

            migrationBuilder.DropColumn(
                name: "fixed_term_anchor_day",
                schema: "members",
                table: "membership_types");

            migrationBuilder.DropColumn(
                name: "fixed_term_anchor_month",
                schema: "members",
                table: "membership_types");

            migrationBuilder.DropColumn(
                name: "renewal_mode",
                schema: "members",
                table: "membership_types");

            migrationBuilder.AddForeignKey(
                name: "fk_memberships_membership_types_membership_type_id",
                schema: "members",
                table: "memberships",
                column: "membership_type_id",
                principalSchema: "members",
                principalTable: "membership_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
