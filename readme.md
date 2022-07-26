# Entity Framework Core with PostgreSQL and Timescale

The following repository holds an example of using Entity Framework Core with
PostgreSQL/TimescaleDB.

## Requirements

- Docker Desktop or equivalent
- .NET 7 Preview 6+

Before looking at the code in the repository, I recommend setting up a docker container with
a PostgreSQL database with the TimescaleDB extension enabled.

```console
docker pull timescale/timescaledb-ha:pg14-latest
docker run -d --name timescaledb -p 5432:5432 -e POSTGRES_PASSWORD=password timescale/timescaledb-ha:pg14-latest
```

You'll also want the [sample data from TimescaleDocs](https://assets.timescale.com/docs/downloads/get-started/real_time_stock_data.zip). 

First, you'll want to run the database migrations in the solution.

```console
dotnet tool restore
dotnet ef database update
```

Next, you can follow the instructions on the [documentation page](https://docs.timescale.com/getting-started/latest/add-data/#ingest-the-dataset), or use JetBrains Rider database tools to import the data after running the **EF Core migrations**. The **Companies** table should be relatively quick, but the `Stocks` table will take a bit of time.

## Running the Application

You can run the solution once your data has loaded. The resulting output should look like the following.

```console
Microsoft (MSFT): $266.39 - $260.34 ~$259.51

Process finished with exit code 0.
```

## The Important Bits

There are a few important things in this project that you'll want to take note of.

1. The use of the `[Keyless]` attribute. ([Read More here](https://docs.microsoft.com/en-us/ef/core/modeling/keyless-entity-types?tabs=data-annotations))

2. The addition to our first migration to make our `Stocks` table a `hypertable`.
    ```csharp
    // Convert Stocks Table to Hypertable
    // language=sql
    migrationBuilder.Sql(
        "SELECT create_hypertable( '\"Stocks\"', 'Time');\n" +
        "CREATE INDEX ix_symbol_time ON \"Stocks\" (\"Symbol\", \"Time\" DESC)"
    );
    ```
3. `Stocks` and `IntervalResults` are `read-only` collections. You'll need to determine an insert mechanism for adding new values to the `Stocks` table, but that could be a simple `ADO.NET` method, Dapper, or other inserting approach. This is time-series data, so there is no primary key, just a wave of information.
4. `DateTime` and `Ngpsql` are _funky_, and while it works, there's required knowledge you need to know to make it work. (https://www.npgsql.org/doc/types/datetime.html).

## Thoughts

> Is this the best approach?

I'm not really sure if it's the best approach, but it uses the mechanisms of EF Core 7. It definitely makes for less SQL, but at the cost of a lot of time and effort. In a team environment, this may be helpful in the long run to keep folks from making mistakes or introducing security issues (SQL Injection Attacks).

> So The Timescale Syntax Just Works?

Well, SQL Server treats our `IQueryable<IntervalResults>` like a table, allowing us to add additional filter options on the initial query. If you want to use specific functions from Timescale, you may want to create new database functions or execute raw sql. You'll still need to add the models to your DbContext in some capacity to make it work, similar to how I added `Stocks` and `IntervalResults`.

You can add more functionality to EF Core's sql interpreter, but I didn't want to go too far in this sample.

> What about Dapper?

Sure, you can use Dapper and keep your EF Core models separate from your Timescale/time-series data. The issue there is you'd have to figure out how to migrate Timescale tasks along-side EF Core tasks.

> What about Migrations? EF Core just knows how to do this?

Well, no it didn't. For Timescale, I ended up modifying migrations by hand to include timescale specific calls. This is fine as long as I hold true to **never modifying migrations that have been deployed.**

## License

Copyright (c) 2022 Khalid Abuhakmeh

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


