using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;

#nullable disable

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        private readonly IDbContextSchema _dbContextSchema;

        public Initial(IDbContextSchema dbContextSchema)
        {
            _dbContextSchema = Guard.AgainstNull(dbContextSchema);
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: _dbContextSchema.Schema);

            migrationBuilder.CreateTable(
                name: "Projection",
                schema: _dbContextSchema.Schema,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(650)", maxLength: 650, nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionJournal",
                schema: _dbContextSchema.Schema,
                columns: table => new
                {
                    ProjectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagedThreadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionJournal", x => new { x.ProjectionId, x.SequenceNumber });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projection",
                schema: _dbContextSchema.Schema,
                table: "Projection",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projection",
                schema: _dbContextSchema.Schema);

            migrationBuilder.DropTable(
                name: "ProjectionJournal",
                schema: _dbContextSchema.Schema);
        }
    }
}
