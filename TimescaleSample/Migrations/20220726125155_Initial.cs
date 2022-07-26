using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimescaleSample.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    DayVolume = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Stocks_Companies_Symbol",
                        column: x => x.Symbol,
                        principalTable: "Companies",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Symbol",
                table: "Stocks",
                column: "Symbol");
            
            // Convert Stocks Table to Hypertable
            // language=sql
            migrationBuilder.Sql(
                "SELECT create_hypertable( '\"Stocks\"', 'Time');\n" +
                "CREATE INDEX ix_symbol_time ON \"Stocks\" (\"Symbol\", \"Time\" DESC)"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
