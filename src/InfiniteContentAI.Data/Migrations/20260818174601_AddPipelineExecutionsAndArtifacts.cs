using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfiniteContentAI.Data.Migrations;

/// <inheritdoc />
public partial class AddPipelineExecutionsAndArtifacts : Migration
{
    private static readonly string[] ArtifactOrderingColumns =
        ["pipeline_execution_id", "created_at", "id"];

    private static readonly string[] ExecutionPipelineOrderingColumns =
        ["organization_id", "pipeline_id", "created_at", "id"];

    private static readonly string[] ExecutionProjectOrderingColumns =
        ["organization_id", "project_id", "created_at", "id"];

    private static readonly string[] ExecutionPipelineStepColumns =
        ["pipeline_execution_id", "pipeline_step_id"];

    private static readonly string[] ExecutionPositionColumns =
        ["pipeline_execution_id", "position"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "pipeline_executions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                pipeline_id = table.Column<Guid>(type: "uuid", nullable: false),
                pipeline_version = table.Column<int>(type: "integer", nullable: false),
                topic = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failure_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                failure_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pipeline_executions", x => x.id);
                table.CheckConstraint("ck_pipeline_executions_pipeline_version_positive", "pipeline_version > 0");
                table.CheckConstraint("ck_pipeline_executions_status_valid", "status IN ('Pending', 'Running', 'Completed', 'Failed')");
                table.ForeignKey(
                    name: "FK_pipeline_executions_pipelines_pipeline_id",
                    column: x => x.pipeline_id,
                    principalTable: "pipelines",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_pipeline_executions_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "step_executions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                pipeline_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                pipeline_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                position = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failure_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                failure_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_step_executions", x => x.id);
                table.CheckConstraint("ck_step_executions_position_positive", "position > 0");
                table.CheckConstraint("ck_step_executions_status_valid", "status IN ('Pending', 'Running', 'Completed', 'Failed')");
                table.CheckConstraint("ck_step_executions_type_valid", "type IN ('Research', 'Script')");
                table.ForeignKey(
                    name: "FK_step_executions_pipeline_executions_pipeline_execution_id",
                    column: x => x.pipeline_execution_id,
                    principalTable: "pipeline_executions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_step_executions_pipeline_steps_pipeline_step_id",
                    column: x => x.pipeline_step_id,
                    principalTable: "pipeline_steps",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "artifacts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                pipeline_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                step_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_artifacts", x => x.id);
                table.CheckConstraint("ck_artifacts_content_length", "char_length(content) BETWEEN 1 AND 100000");
                table.CheckConstraint("ck_artifacts_type_valid", "type IN ('Research', 'Script')");
                table.ForeignKey(
                    name: "FK_artifacts_pipeline_executions_pipeline_execution_id",
                    column: x => x.pipeline_execution_id,
                    principalTable: "pipeline_executions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_artifacts_projects_project_id",
                    column: x => x.project_id,
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_artifacts_step_executions_step_execution_id",
                    column: x => x.step_execution_id,
                    principalTable: "step_executions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_artifacts_execution_created_at_id",
            table: "artifacts",
            columns: ArtifactOrderingColumns);

        migrationBuilder.CreateIndex(
            name: "ix_artifacts_project_id",
            table: "artifacts",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ux_artifacts_step_execution_id",
            table: "artifacts",
            column: "step_execution_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_pipeline_executions_organization_pipeline_created_at_id",
            table: "pipeline_executions",
            columns: ExecutionPipelineOrderingColumns);

        migrationBuilder.CreateIndex(
            name: "ix_pipeline_executions_organization_project_created_at_id",
            table: "pipeline_executions",
            columns: ExecutionProjectOrderingColumns);

        migrationBuilder.CreateIndex(
            name: "ix_pipeline_executions_pipeline_id",
            table: "pipeline_executions",
            column: "pipeline_id");

        migrationBuilder.CreateIndex(
            name: "ix_pipeline_executions_project_id",
            table: "pipeline_executions",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ix_step_executions_pipeline_step_id",
            table: "step_executions",
            column: "pipeline_step_id");

        migrationBuilder.CreateIndex(
            name: "ux_step_executions_execution_pipeline_step",
            table: "step_executions",
            columns: ExecutionPipelineStepColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_step_executions_execution_position",
            table: "step_executions",
            columns: ExecutionPositionColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "artifacts");

        migrationBuilder.DropTable(
            name: "step_executions");

        migrationBuilder.DropTable(
            name: "pipeline_executions");
    }
}
