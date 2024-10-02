using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quartz.Net_Dashboard.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbJobList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JobTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobData = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScheduleExpression = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduleExpressionDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CrAgent = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    CrDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbJobList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbSample",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbSample", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbSampleSync",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbSampleSync", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbJobList");

            migrationBuilder.DropTable(
                name: "TbSample");

            migrationBuilder.DropTable(
                name: "TbSampleSync");
        }
    }
}
