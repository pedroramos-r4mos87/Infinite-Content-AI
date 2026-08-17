using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfiniteContentAI.Data.Migrations;

/// <inheritdoc />
public partial class AddPipelines : Migration
{
    private static readonly string[] PipelinePositionColumns =
        ["pipeline_id", "position"];

    private static readonly string[] PipelineTypeColumns =
        ["pipeline_id", "type"];

    private static readonly string[] PipelineOrderingColumns =
        ["organization_id", "project_id", "created_at", "id"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "pipelines",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pipelines", x => x.id);
                table.ForeignKey(
                    name: "FK_pipelines_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "pipeline_steps",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                position = table.Column<int>(type: "integer", nullable: false),
                pipeline_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pipeline_steps", x => x.id);
                table.CheckConstraint("ck_pipeline_steps_position_positive", "position > 0");
                table.ForeignKey(
                    name: "FK_pipeline_steps_pipelines_pipeline_id",
                    column: x => x.pipeline_id,
                    principalTable: "pipelines",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ux_pipeline_steps_pipeline_position",
            table: "pipeline_steps",
            columns: PipelinePositionColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_pipeline_steps_pipeline_type",
            table: "pipeline_steps",
            columns: PipelineTypeColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_pipelines_organization_project_created_at_id",
            table: "pipelines",
            columns: PipelineOrderingColumns);

        migrationBuilder.CreateIndex(
            name: "ix_pipelines_project_id",
            table: "pipelines",
            column: "project_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "pipeline_steps");

        migrationBuilder.DropTable(
            name: "pipelines");
    }
}
