using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Membr.Module.Member.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_contact_information : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_information",
                schema: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    contact_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contact_detail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_information", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_information_members_member_id",
                        column: x => x.member_id,
                        principalSchema: "members",
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_information_member_id_contact_type",
                schema: "members",
                table: "contact_information",
                columns: ["member_id", "contact_type"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_information",
                schema: "members");
        }
    }
}
