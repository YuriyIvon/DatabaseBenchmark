# Database Benchmark
![Application Status](https://github.com/YuriyIvon/DatabaseBenchmark/actions/workflows/build.yml/badge.svg)

A universal multi-database query benchmark tool

## Motivation<a name="motivation"></a>
When deciding what database engine to choose for your project, you need to compare how quickly different engines can handle typical queries specific to the project. Usually, such comparison requires writing a test application that executes a few hard-coded queries against the database engines in question. However, this approach is far from ideal:
* Manually written queries are prone to accidental mistakes and omissions that skew benchmark results.
* Manually written equivalent queries for different database engines may get inconsistent in their logic.
* Randomization of query parameters requires extra coding effort.
* Learning how to use a database engine's client library may take significant time, especially for NoSQL databases.
* It is not always easy to find a quick way to import a large amount of data into the database using built-in database capabilities or third-party tools.

This tool addresses the issues from above by introducing a data import and query abstraction mechanism with parameter randomization capabilities. At the same time, it allows falling back to raw queries if the abstraction doesn't provide required query features. Parameter randomization is supported in both cases.

## Table of contents

* [Usage](#usage)
  * [Table creation](#table_creation)
  * [Data import](#data_import)
  * [Query benchmark](#query_benchmark)
  * [Query benchmark scenarios](#query_benchmark_scenarios)
  * [Raw query benchmark](#raw_query_benchmark)
  * [Raw query benchmark scenarios](#raw_query_benchmark_scenarios)
  * [Query definition](#query_definition)
  * [Value randomization rules](#value_randomization_rules)
* [Limitations](#limitations)

## Usage<a name="usage"></a>

### Table creation<a name="table_creation"></a>

Create a table definition - see [SaleTable.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesTable.json) as an example.

**Though different databases may use different terminology for data objects, the tool uses relational terms such as table, column, and row for consistency.**

Supported values for `Type` attribute are `Boolean`, `DateTime`, `Double`, `Guid`, `Integer`, `Long`,  `String`, and `Text`.

`Queryable` gives a hint if the column is going to participate in query conditions. Based on this information some table builders may generate more optimal definitions.

Once the definition is ready, you can create the table in all database management systems you are comparing:

```
DatabaseBenchmark create --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --TraceQueries=true

DatabaseBenchmark create --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json

DatabaseBenchmark create --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --TraceQueries=true
```

**Please note that in most real-life scenarios, the benchmark tool and a database engine must run on separate machines to take network throughput into account and avoid resource contention.**

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

### Data import<a name="data_import"></a>

Once tables are created, it is the time to import the dataset you are going to use in your benchmarks:
```
DatabaseBenchmark import --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv"

DatabaseBenchmark import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv"

DatabaseBenchmark import --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" 
```

These snippets use a sample CSV file from [here](https://eforexcel.com/wp/wp-content/uploads/2017/07/1000000%20Sales%20Records.zip). _You will need to remove spaces from CSV headers to make them consistent with the table definition._

Alternatively, data can be imported from any database supported by the tool:

```
DatabaseBenchmark import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Database --DataSourceFilePath=SalesSqlServerDataSource.json
```

Here file [SalesSqlServerDataSource.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesSqlServerDataSource.json) defines where and how to get source data. With relational databases, a raw query specified in `Query` parameter must return all columns declared in the target table definition. Extra columns are ignored. For those databases where the container name doesn't appear in a query, there is an optional data source parameter `TableName`.

### Query benchmark<a name="query_benchmark"></a>

To start running benchmarks, you will need to create a set of [query definitions](#query_definition). In our example scenario we will use only two queries - one returning a page of results for some search criteria ([SalesPageQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQuery.json)) and one calculating aggregates for a subset of data ([SalesAggregateQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateQuery.json)).

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
* `ReportFilePath` - path to the result report output file. If not specified, the report is written to the console. 
* `TraceResults` - Boolean parameter specifying if query results should be printed to the console.

**Please note that the tool, in general, is not responsible for index creation and other database configuration tweaks. Any settings that can be modified after the table has been created must be controlled by the person responsible for the benchmark. Thus, ensure that all indexes and other required settings are in place before running a real benchmark.**

To speed up sample queries used in this manual, you can create two indexes in each database: one on `Country` column and another on `ItemType` and `OrderDate` columns. Alternatively, in SQL Server, you can create a columnstore index instead.

When everything is ready, a full benchmark can be run.

```
DatabaseBenchmark query --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=100 --QueryParallelism=10 --BenchmarkName="SQL Server - Page Query - 10 threads"
```

### Query benchmark scenarios<a name="query_benchmark_scenarios"></a>

The same can obviously be done for other database engines and query definitions, but what if you want a single results table for all the benchmarks at once? The tool supports so-called scenarios that allow you to define multiple benchmarks in a single file and run them sequentially by calling a single command.

A sample scenario benchmarking the same query on different databases can be found in [SalesPageQueryScenario.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQueryScenario.json).

```
DatabaseBenchmark query-scenario --QueryScenarioFilePath=SalesPageQueryScenario.json 
```

Each step definition in this scenario file consists of a set of query command parameters used earlier when running a single benchmark from the command line. There are a few command-line parameters that are common for all steps in a scenario: `ReportFormatterType`, `ReportFilePath`, `TraceQueries`, `TraceResults`.

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

### Raw query benchmark<a name="raw_query_benchmark"></a>

If the query you want to benchmark can't be expressed as a [query definition](#query_definition), you can leverage the Database Benchmark's raw query feature.

```
DatabaseBenchmark raw-query --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableName=Sales --QueryFilePath=SalesAggregateRawSqlQuery.txt --QueryParametersFilePath=SalesAggregateRawQueryParameters.json --QueryCount=100

DatabaseBenchmark raw-query --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableName=Sales --QueryFilePath=SalesAggregateRawMongoDbQuery.txt --QueryParametersFilePath=SalesAggregateRawQueryParameters.json --QueryCount=100
```
Here files [SalesAggregateRawSqlQuery.txt](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateRawSqlQuery.txt) and [SalesAggregateRawMongoDbQuery.txt](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateRawMongoDbQuery.txt) contain database-specific query patterns, while [SalesAggregateRawQueryParameters.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateRawQueryParameters.json) provide a common list of parameter definitions. 

This command differs from the regular query command by the following parameters:
* `QueryFilePath` - path to a text file containing a query pattern. The pattern is parameterized with placeholders like `${ParameterName}`.
* `QueryParametersFilePath`- path to a JSON file containing query parameter definitions. If you manually write equivalent raw queries for different databases, they all can share the same parameter file.  This parameter is optional and can be omitted if the query has no parameters.
* `TableName` -  for those databases where the container name doesn't appear in a query, specifies the container to be queried.

A parameter file must contain a JSON array of objects having the following properties:
* `Name` - parameter name.
* `Type` - parameter type. Similarly to the column type, can be one of `Boolean`, `DateTime`, `Double`, `Guid`, `Integer`, `Long`,  `String`, and `Text`. 
* `Value` - parameter value. Is ignored if `RandomizeValue` is `true`.
* `Collection` - when `RandomizeValue` is `true`, specifies if a random collection should be generated.
* `RandomizeValue` - specifies if the value should be randomized.
* `ValueRandomizationRule` - specifies [value randomization rules](#value_randomization_rules).

### Raw query benchmark scenarios<a name="raw_query_benchmark_scenarios"></a>

Raw query benchmark scenarios are very similar to [regular scenarios](#query_benchmark_scenarios). The only difference is in a few step definition properties that are explained in the [raw query benchmark](#raw_query_benchmark) section.

```
DatabaseBenchmark raw-query-scenario --QueryScenarioFilePath=SalesAggregateRawQueryScenario.json
```

### Query definition<a name="query_definition"></a>

A query definition has the following top-level properties:

* `Columns` - an array of columns to be returned by the query. If `Aggregate` is specified, it may contain only columns present in its `GroupColumnNames` array.  
* `Condition` - an object representing the query condition.
* `Aggregate` - an object representing the aggregation to be done.
* `Sort` - an array of objects defining the sort order. Each object in the array has `ColumnName` and `Direction` properties, where the latter can be either `Ascending` or `Descending`. 
* `Skip` - specifies how many rows to skip before returning a query result.
* `Take` - sets the maximum number of query result rows to be returned. The default value is 0, which means that the limit is not specified.

`Condition` is represented by an object tree structure consisting of "group condition" and "primitive condition" nodes.

A group condition has the following properties:

* `Operator` - logical operator, can be one of `And`, `Or`, and `Not`.
* `Conditions` - array of conditions to be combined by the logical operator. Please note that `Not` allows only one condition in this array.

A primitive condition has the following properties:

* `ColumnName` - table column to be checked in the condition (the first operand).
* `Operator` - conditional operator, can be one of `Equals`, `NotEquals`, `In`, `Greater`, `GreaterEquals`, `Lower`, `LowerEquals`, `Contains`, and `StartsWith`.
* `Value` - specifies the value of the second operand. Is ignored if `RandomizeValue` is `true`. 
* `RandomizeValue` - specifies if the value should be randomized.
* `ValueRandomizationRule` - specifies [value randomization rules](#value_randomization_rules).

`Aggregate` has the following top-level properties:
* `GroupColumnNames` - an array of columns the query result should be grouped by.
* `ResultColumns` - an array of aggregate columns to be returned by the query.

Each element of `ResultColumns` array has the following properties:
* `SourceColumnName` - a table column the aggregate function will be applied to. Can be omitted for `Count` aggregate. 
* `Function` - an aggregate function, can be one of `Average`, `Count`, `DistinctCount`, `Max`, `Min`, `Sum`.
* `ResultColumnName` - a name assigned to the aggregate expression in the result set.

### Value randomization rules<a name="value_randomization_rules"></a>

When `RandomizeValue` is `true` on a query condition or on a raw query parameter, this structure defines how the value should be generated. The value type depends on the column used in the condition or on the corresponding raw query parameter definition. A random collection is generated only for `In` operator condition or for a raw query parameter with `Collection` property set to `true`.

* `UseExistingValues` - specifies if the value should be randomly picked from already existing values in this column. Is `true` by default.
* `ExistingValuesOverride` - provides a specific array of values to pick a random value from. Can be particularly useful in case "distinct" queries are too slow on the database being tested. 
* `ExistingValuesSourceTableName` - provides an alternative table to select existing values from.
* `ExistingValuesSourceColumnName` - provides an alternative column name to select existing values from. 
* `MinCollectionLength` - minimum random collection length. Is equal to `1` by default.
* `MaxCollectionLength` - maximum random collection length. Is equal to `10` by default.
* `MinNumericValue` - minimum numeric value. Is equal to `0` by default.
* `MaxNumericValue` - maximum numeric value. Is equal to `100` by default.
* `MinDateTimeValue` - minimum date time value. Is equal to the current date and time minus 10 years by default. 
* `MaxDateTimeValue` - maximum date time value. Is equal to the current date and time by default.
* `MinStringValueLength` - minimum random string length. Is equal to `1` by default.
* `MaxStringValueLength` - maximum random string length. Is equal to `10` by default.
* `AllowedCharacters` - characters to be used when generating a random string. By default contains uppercase Latin letters and digits.

## Limitations<a name="limitations"></a>

There are some limitations that are going to be addressed in the future:

* Query definitions don't support joins. A workaround is using the raw queries approach.
* Random inclusion of condition parts into generated queries - there is a reserved property `RandomizeInclusion` on each part of a query condition, but it is not handled yet.
* Configurable partitioning in Cosmos DB and other databases is not supported yet.
* Importing from Elasticsearch database doesn't support unlimited number of rows.