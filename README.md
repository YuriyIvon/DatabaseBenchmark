# Database Benchmark
A universal multi-database query benchmark tool

### Project Status
![Application Status](https://github.com/YuriyIvon/DatabaseBenchmark/actions/workflows/application.yml/badge.svg)

### Motivation
When deciding what database engine to choose for your project, you need to compare how quickly different engines can handle typical queries specific to the project. Usually, such comparison requires writing a test application that executes a few hard-coded queries against the database engines in question. However, this approach is far from ideal:
* Manually written queries are prone to accidental mistakes and omissions that skew benchmark results.
* Manually written equivalent queries for different database engines may get inconsistent in their logic.
* Randomization of query parameters requires extra coding effort.
* Learning how to use a database engine's client library may take significant time, especially for NoSQL databases.
* It is not always easy to find a quick way to import a large amount of data into the database using built-in database capabilities or third-party tools.

This tool addresses the issues from above by introducing a data import and query abstraction mechanism with parameter randomization capabilities. At the same time, it allows falling back to raw queries if the abstraction doesn't provide required query features. Parameter randomization is supported in both cases.

### Usage

#### Table creation


Create a table definition - see [SaleTable.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesTable.json) as an example.

**Though different databases may use different terminology for data objects, the tool uses terms table and column for consistency.**

Supported values for `Type` attribute are:
* `Boolean`
* `DateTime`
* `Double`
* `Guid`
* `Integer`
* `Long`
* `String`
* `Text`

`Queryable` gives a hint if the column is going to participate in query conditions. Based on this information some table builders may generate more optimal definitions.

Once the definition is ready, you can create the table in all database management systems you are comparing:

```
DatabaseBenchmark create --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --TraceQueries=true

DatabaseBenchmark create --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json

DatabaseBenchmark create --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --TraceQueries=true
```

**Please note that in a real-life scenario, the benchmark tool and a database engine usually must run on separate machines to take network throughput into account and avoid resource contention.**

Supported values for `DatabaseType` attribute are:
* `ClickHouse`
* `CosmosDb` - SQL API only, connection string must contain a non-standard property specifying a database name to be used - `Database=database_name`.
* `Elasticsearch`
* `MySql` - any database engine compatible with MySQL: MySQL, MariaDB, SingleStore, etc.
* `MonetDb`
* `MongoDb`
* `Postgres`
* `PostgresJsonb` - stores all queryable "logical" columns in a single JSONB column indexed with GIN index of  jsonb_path_ops type. Supports only `Equals` and `In` primitive operators. 
* `SqlServer`

#### Data import

Once tables are created, it is the time to import the dataset you are going to use in your benchmarks:
```
DatabaseBenchmark import --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv"

DatabaseBenchmark import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv"

DatabaseBenchmark import --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" 
```

These snippets use a sample CSV file from [here](https://eforexcel.com/wp/wp-content/uploads/2017/07/1000000%20Sales%20Records.zip). _You will need to remove spaces from CSV headers to make them consistent with the table definition._

Alternatively, data can be imported from any database supported by the tool:

```
import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Database --DataSourceFilePath=SqlServerDataSource.json
```

Here file [SqlServerDataSource.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SqlServerDataSource.json) describes where and how to get source data. With relational databases, a raw query specified in `Query` parameter must return all columns declared in the target table definition. Extra columns are ignored. For those databases where container name doesn't appear in a query, there is an optional data source parameter `TableName`.

#### Query benchmark

To start running benchmarks you will need to create a set of query definitions. In our example scenario we will use only two queries - one returning a page of results for some search criteria ([SalesPageQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQuery.json)) and one calculating aggregates for a subset of data ([SalesAggregateQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateQuery.json)).

To make sure that the query is properly generated in all cases and you get correct results do a single run:

```
DatabaseBenchmark query --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true

DatabaseBenchmark query --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true

DatabaseBenchmark query --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true
```

The same commands can be executed with `--QueryFilePath=SalesAggregateQuery.json` to make sure it works fine with all databases we are going to benchmark.

There are some parameters specific to the query command:
* `BenchmarkName` - benchmark name to be printed in the results table.
* `QueryParallelism` - number of parallel threads to be run.
* `QueryCount` - number of query executions on each thread.
* `WarmupQueryCount` - number of warm-up query executions on each thread.
* `ReportFormatterType` - benchmark result report output format, currently can be either `Text` or `Csv`.
* `ReportFilePath` - path to the result report output file. If not specified, report is written to the console. 
* `TraceResults` - a Boolean parameter specifying if query results should be printed to the console.

**Please note that the tool, in general, is not responsible for index creation and other database configuration tweaks. Any settings that can be modified after the table has been created must be controlled by the person responsible for the benchmark. Thus, ensure that all indexes and other required settings are in place before running a real benchmark.**

When everything is ready, a full benchmark can be run.

```
DatabaseBenchmark query --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=100 --QueryParallelism=10 --BenchmarkName="SQL Server - Page Query - 10 threads"
```

#### Query benchmark scenarios

The same can obviously be done for other database engines and query definitions, but what if you want a single results table for all the benchmarks at once? The tool supports so-called scenarios that allow you to define multiple benchmarks in a single file and run them sequentially by calling a single command.

A sample scenario benchmarking the same query on different databases can be found in [SalesPageQueryScenario.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQueryScenario.json).

```
DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryScenario.json 
```

Each step definition in this scenario file consists of a set of query command parameters used earlier when running a single benchmark from the command line. There are a few command line parameters that are common for all steps in a scenario: `ReportFormatterType`, `ReportFilePath`, `TraceQueries`, `TraceResults`.

Sometimes there is a need to run a set of benchmarks for each of the database engines you are investigating. For example, you need to understand how query throughput depends on the number of concurrent sessions, so you want to run the same benchmark multiple times with different `QueryParallelism` parameter values. There is a feature that reduces the amount of copy-paste in such situations - parameterized scenarios.

An example of a parameterized scenario can be found in [SalesPageQueryParallelScenario.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQueryParallelScenario.json). Its database-specific parameter files are [SqlServerScenarioParameters.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SqlServerScenarioParameters.json), [PostgresScenarioParameters.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/PostgresScenarioParameters.json), and [MongoDbScenarioParameters.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/MongoDbScenarioParameters.json).

```
DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryParallelScenario.json --QueryScenarioParametersFilePath=SqlServerScenarioParameters.json

DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryParallelScenario.json --QueryScenarioParametersFilePath=PostgresScenarioParameters.json

DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryParallelScenario.json --QueryScenarioParametersFilePath=MongoDbScenarioParameters.json
```

The logic behind parameter files is simple: any property from the parameter file that doesn't exist in the scenario step definition is implicitly added to the definition. Then all placeholders like `${DatabaseType}` are substituted with the corresponding values from the parameter file.

If you need to run only specific steps from a scenario file, use `QueryScenarioStepIndexes` that accepts a comma-separated list of step ordinal numbers.

```
DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryParallelScenario.json --QueryScenarioParametersFilePath=SqlServerScenarioParameters.json --QueryScenarioStepIndexes=1,2
```

#### Raw query benchmark

TODO

### Limitations

There are some limitations that are going to be addressed in the future:

* Query definitions don't support joins. A workaround is using raw queries approach.
* Random inclusion of condition parts into generated queries - there is a reserved property `RandomizeInclusion` on each part of a query condition, but it is not handled yet.
* Configurable partitioning in Cosmos DB and other databases is not supported yet.
* Importing from Elasticsearch database doesn't support unlimited number of rows.