using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Membr.Module.Member.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_udf_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "udf_definitions",
                schema: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    options = table.Column<string>(type: "text", nullable: false),
                    default_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_udf_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_udf_values",
                schema: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_id = table.Column<int>(type: "integer", nullable: false),
                    udf_definition_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_udf_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_udf_values_members_member_id",
                        column: x => x.member_id,
                        principalSchema: "members",
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_member_udf_values_udf_definitions_udf_definition_id",
                        column: x => x.udf_definition_id,
                        principalSchema: "members",
                        principalTable: "udf_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_member_udf_values_member_id_udf_definition_id",
                schema: "members",
                table: "member_udf_values",
                columns: ["member_id", "udf_definition_id"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_udf_values_udf_definition_id",
                schema: "members",
                table: "member_udf_values",
                column: "udf_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_udf_definitions_name",
                schema: "members",
                table: "udf_definitions",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member_udf_values",
                schema: "members");

            migrationBuilder.DropTable(
                name: "udf_definitions",
                schema: "members");
        }
    }
}
