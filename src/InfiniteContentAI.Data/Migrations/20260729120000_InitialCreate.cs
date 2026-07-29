using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace InfiniteContentAI.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260729120000_InitialCreate")]
public partial class InitialCreate : Migration
{
    private static readonly string[] OrganizationOrderingColumns =
        ["organization_id", "created_at", "id"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
            },
            constraints: table => table.PrimaryKey("pk_projects", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_projects_organization_created_at_id",
            table: "projects",
            columns: OrganizationOrderingColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "projects");
    }
}
