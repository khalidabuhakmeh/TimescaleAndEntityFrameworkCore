using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimescaleSample.Migrations
{
    /// <inheritdoc />
    public partial class AddGetWeeklyResultsFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var function = $"""
            create or replace FUNCTION get_weekly_results("value" timestamp with time zone)
                returns Table
                        (
                            "Symbol"  text,
                            "Name"    text,
                            "Start"   numeric,
                            "End"     numeric,
                            "Average" numeric
                        )
                LANGUAGE SQL
            as
            $func$
            SELECT srt."Symbol",
                   C."Name",
                   first("Price", "Time") as "Start",
                   last("Price", "Time")  as "End",
                   avg("Price")           as "Average"
            FROM "Stocks" srt
                     inner join "Companies" C on C."Symbol" = srt."Symbol"
            WHERE "Time" > "value" - INTERVAL '1 week' 
            AND  "Time" <= "value" 
            GROUP BY srt."Symbol", "Name"
            ORDER BY "End" DESC;
            $func$;
            """;

            migrationBuilder.Sql(function);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop function get_weekly_results(timestamp with time zone);");
        }
    }
}
