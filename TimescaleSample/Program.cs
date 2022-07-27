using Microsoft.EntityFrameworkCore;
using TimescaleSample.Models;

var db = new StocksDbContext();

var sql = """
SELECT * FROM "Stocks"
WHERE "Time" > now() - INTERVAL '1 week'
AND "Symbol" = 'MSFT'
""" ;

var trades = db.Stocks.FromSqlRaw(sql).Count();
Console.WriteLine($"{trades} trades of MSFT in the last week");

// UTC Only
// Read this and cry https://www.npgsql.org/doc/types/datetime.html
var date = new DateTime(2022, 06, 29, 0, 0, 0, DateTimeKind.Utc);
var top = db
    .GetWeeklyResults(date)
    .First(x => x.Symbol == "MSFT");

Console.WriteLine($"{top.Name} ({top.Symbol}): {top.Start:C} - {top.End:C} ~{top.Average:C}");