
# Database Benchmark
![Application Status](https://github.com/YuriyIvon/DatabaseBenchmark/actions/workflows/build.yml/badge.svg)

A universal multi-database query benchmark and data generator tool

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
  * [Insert benchmark](#insert_benchmark)
  * [Data sources](#data_sources)
  * [Table definition](#table_definition)
  * [Query definition](#query_definition)
  * [Value randomization rule](#value_randomization_rule)
  * [Generators](#generators)
  * [Report columns](#report_columns)
  * [Connection strings](#connection_strings)

* [Limitations](#limitations)

## Usage<a name="usage"></a>

### Table creation<a name="table_creation"></a>

Create a [table definition](#table_definition) - see [SalesTable.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesTable.json) as an example.

**Though different databases may use different terminology for data objects, the tool uses relational terms such as table, column, and row for consistency.**

Once the definition is ready, you can create the table in all database management systems you are comparing:

```
DatabaseBenchmark create --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --TraceQueries=true

DatabaseBenchmark create --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json

DatabaseBenchmark create --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --TraceQueries=true
```

**Please note that in most real-life scenarios, the benchmark tool and a database engine must run on separate machines to take network throughput into account and avoid resource contention.**

<a name="database_types"></a>Supported values for `DatabaseType` parameter are:
* `ClickHouse`
* `CosmosDb` - SQL API only, connection string must contain a non-standard property specifying a database name to be used - `Database=database_name`.
* `DynamoDB` - the connection string format is specific to the tool, an example can be found in the [Connection strings](#connection_strings) section.
* `Elasticsearch`
* `MySql` - any database engine compatible with MySQL: MySQL, MariaDB, SingleStore, etc.
* `MonetDb`
* `MongoDb`
* `Oracle`
* `Postgres`
* `PostgresJsonb` - stores all queryable "logical" columns in a single JSONB column indexed with GIN index of jsonb_path_ops type.
* `Snowflake`
* `SqlServer`

Connection string samples for each of the database engines can be found in the [corresponding section](#connection_strings).

Other optional parameters of the `create` command are:
* `DropExisting` - specifies whether to re-create the table if it already exists.
* `PostScriptFilePath` - path to a database script to be executed after the table has been created. Can be used to automate index creation when a table is re-created during the benchmark.

This command also has a few database-specific parameters:
* `ClickHouse.Engine` - table engine. Default is `MergeTree()`.
* `ClickHouse.OrderBy` - table sort order. Default is `tuple()`.
* `MySql.Engine` - table engine. Default is `InnoDB`.
* `PostgresJsonb.CreateGinIndex` - specifies whether to create a default GIN index on the jsonb field (with `jsonb_path_ops` operator class). True by default.

### Data import<a name="data_import"></a>

Once tables are created, it is the time to import the dataset you are going to use in your benchmarks:
```
DatabaseBenchmark import --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" --MappingFilePath=SalesTableMapping.json

DatabaseBenchmark import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" --MappingFilePath=SalesTableMapping.json

DatabaseBenchmark import --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" --MappingFilePath=SalesTableMapping.json
```

These snippets use a sample CSV file from [here](https://eforexcel.com/wp/wp-content/uploads/2017/07/1000000%20Sales%20Records.zip).

Alternatively, data can be imported from any database supported by the tool:

```
DatabaseBenchmark import --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --DataSourceType=Database --DataSourceFilePath=SalesSqlServerDataSource.json
```

Here file [SalesSqlServerDataSource.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesSqlServerDataSource.json) defines where and how to get source data. With relational databases, a raw query stored in a file referenced by `QueryFilePath` parameter must return all columns declared in the target table definition. Extra columns are ignored. For those databases where the container name doesn't appear in a query, there is an optional data source parameter `TableName`.

More information on all data source types and their parameters can be found in the [corresponding section](#data_sources).

Other optional parameters of the `import` command are:
* `MappingFilePath` - in case column names in the data source don't match table columns (like in the example above, where CSV headers contain space characters), this parameter can be used to point to a JSON file with column mappings. An object in this file must have `Columns` array where each item consists of two fields - `SourceColumnName` and `TableColumnName`.
* `DataSourceCulture` - culture identifier (e.g., "en-GB") used for parsing input string values if a string value is mapped to a non-string column. The current system culture is used by default.
* `DataSourceMaxRows` - maximum number of rows that the data source can return.
* `PostScriptFilePath` - path to a database script to be executed after the data has been imported. One of its typical uses is rebuilding indices.

This command also has a database-specific parameter:
* `MongoDb.CollectCosmosDbRequestUnits` - allows collecting request charge metric in case of Azure Cosmos DB API for MongoDB (may affect query timing).

### Query benchmark<a name="query_benchmark"></a>

To start running benchmarks, you will need to create a set of [query definitions](#query_definition). In our example scenario we will use only two queries - one returning a page of results for some search criteria ([SalesPageQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesPageQuery.json)) and one calculating aggregates for a subset of data ([SalesAggregateQuery.json](https://github.com/YuriyIvon/DatabaseBenchmark/blob/main/samples/Sales/SalesAggregateQuery.json)).

To make sure that the query is properly generated in all cases and you get correct results do a single run:

```
DatabaseBenchmark query --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true

DatabaseBenchmark query --DatabaseType=Postgres --ConnectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=postgres" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true

DatabaseBenchmark query --DatabaseType=MongoDb --ConnectionString="mongodb://localhost/benchmark" --TableFilePath=SalesTable.json --QueryFilePath=SalesPageQuery.json --QueryCount=1 --WarmupQueryCount=0 --TraceQueries=true --TraceResults=true
```

The same commands can be executed with `--QueryFilePath=SalesAggregateQuery.json` to make sure it works fine with all databases we are going to benchmark.

There are some parameters specific to the `query` command:
* `BenchmarkName` - benchmark name to be printed in the results table.
* `QueryParallelism` - number of parallel threads to be run.
* `QueryCount` - number of query executions on each thread.
* `WarmupQueryCount` - number of warm-up query executions on each thread.
* `QueryDelay` - delay between query executions (in milliseconds).
* `ReportFormatterType` - benchmark result report output format, currently can be either `Text` or `Csv`.
* `ReportFilePath` - path to the result report output file. If not specified, the report is written to the console.
* `ReportColumns` - a comma-separated list of report columns to be shown. For the list of supported values refer to [report columns](#report_columns).
* `ReportCustomMetricColumns` - a comma-separated list of custom metric report columns to be shown. For the list of supported values refer to [report columns](#report_columns).
* `TraceResults` - Boolean parameter specifying if query results should be printed to the console.

This command also has a few database-specific parameters:
* `CosmosDb.BatchSize` - a maximum number of items to be fetched in one round-trip.
* `MongoDb.BatchSize` - a maximum number of items to be fetched in one round-trip.
* `MongoDb.CollectCosmosDbRequestUnits` - allows collecting request charge metric in case of Azure Cosmos DB API for MongoDB (may affect query timing).
* `PostgresJsonb.UseGinOperators` - specifies whether to use GIN-specific query operators such as @> where possible. Is `true` by default.

**Please note that the tool, in general, is not responsible for index creation and other database configuration tweaks. Any settings that can be modified after the table has been created must be controlled by the person responsible for the benchmark. Thus, ensure that all indexes and other required settings are in place before running a real benchmark.**

To speed up sample queries used in this manual, you can create a compound index in each database on `Country` and `OrderDate` columns. Alternatively, in SQL Server, you can create a columnstore index instead.

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
* `ValueRandomizationRule` - specifies the [value randomization rule](#value_randomization_rule).

### Raw query benchmark scenarios<a name="raw_query_benchmark_scenarios"></a>

Raw query benchmark scenarios are very similar to [regular scenarios](#query_benchmark_scenarios). The only difference is in a few step definition properties that are explained in the [raw query benchmark](#raw_query_benchmark) section.

```
DatabaseBenchmark raw-query-scenario --QueryScenarioFilePath=SalesAggregateRawQueryScenario.json
```
### Insert benchmark<a name="insert_benchmark"></a>
An insert benchmark is a combination of a query benchmark and import procedure: it sequentially reads rows from a data source and inserts them into the target database according to the query execution parameters. Therefore, the "insert" command combines the parameters of "query" and "import" except for the reference to a query definition:

```
DatabaseBenchmark insert --DatabaseType=SqlServer --ConnectionString="Data Source=.;Initial Catalog=benchmark;Integrated Security=True;" --TableFilePath=SalesTable.json --DataSourceType=Csv --DataSourceFilePath="1000000 Sales Records.csv" --QueryParallelism=10 --QueryCount=10000 --WarmupQueryCount=100 --BatchSize=10 
```

### Data sources<a name="data_sources"></a>
#### Csv
Reads data from a CSV file specified by the `DataSourceFilePath` parameter. Supports the following extra parameters:

* `DataSource.Csv.Delimiter` - overrides the default value delimiter.
* `DataSource.Csv.TreatBlankAsNull` - specifies whether to treat blank values as null. Is `true` by default.

#### Database
Reads data from any database engine supported by the tool. The file specified by the `DataSourceFilePath` parameter must contain a valid JSON object with the following attributes:

* `DatabaseType` - any of the [database types](#database_types) supported by the tool.
* `ConnectionString` - a connection string.
* `QueryFilePath` - a path to the file containing the raw query retrieving the source data.
* `TableName` - a specific table (collection) name the query should be executed against. Should be used only for the database engines, where raw queries don't include the table name (e.g., MongoDB, Elasticsearch, etc.)

#### Generator
Dynamically generates data based on user-defined configurations, allowing for the customization of columns and their corresponding generator settings to suit specific data simulation needs.

The file specified by the `DataSourceFilePath` parameter must contain a valid JSON object with the `Columns` array, where each array element has the following attributes:

* `Name` - specifies the name of the column to be provided by the data source.
* `GeneratorOptions` - defines the type of value generator to be used for the column, along with its relevant options. See [generators](#generators) for more details. 

### Table definition<a name="table_definition"></a>
A table definition has the following top-level properties:

* `Name` - table name.
* `Columns` - an array of column definitions.

Where each column definition has the following properties:

* `Name` - column name.
* `Type` - column type, can be one of `Boolean`, `DateTime`, `Double`, `Guid`, `Integer`, `Long`,  `String`, and `Text`.
* `Nullable` - specifies if the column is nullable.
* `Queryable` - gives a hint if the column is going to participate in query conditions. Based on this information some table builders may generate more optimal definitions.
* `DatabaseGenerated` - specifies if the column is auto-generated. Databases that don't support auto-generated columns will report a warning.
* `PartitionKey` - specifies if the table should be partitioned by this column. Is currently supported for Cosmos DB and DynamoDB only (if there is no partition key column in the table definition, a dummy constant-value partition key is created). Other database plugins ignore this flag.
* `SortKey` - specifies if the column should be assigned as the sort key for the table. Is supported for DynamoDB only. Other database plugins ignore this flag.

### Query definition<a name="query_definition"></a>

A query definition has the following top-level properties:

* `Distinct` - specifies whether to apply "distinct" to the query results. Is `false` by default.
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
* `RandomizeInclusion` - specifies if this condition should be randomly included in the query. Is `false` by default.

A primitive condition has the following properties:

* `ColumnName` - table column to be checked in the condition (the first operand).
* `Operator` - conditional operator, can be one of `Equals`, `NotEquals`, `In`, `Greater`, `GreaterEquals`, `Lower`, `LowerEquals`, `Contains`, and `StartsWith`.
* `Value` - specifies the value of the second operand. Is ignored if `RandomizeValue` is `true`. 
* `RandomizeValue` - specifies if the value should be randomized. Is `false` by default.
* `ValueRandomizationRule` - specifies the [value randomization rule](#value_randomization_rule).
* `RandomizeInclusion` - specifies if this condition should be randomly included in the query. Is `false` by default.

`Aggregate` has the following top-level properties:
* `GroupColumnNames` - an array of columns the query result should be grouped by.
* `ResultColumns` - an array of aggregate columns to be returned by the query.

Each element of `ResultColumns` array has the following properties:
* `SourceColumnName` - a table column the aggregate function will be applied to. Can be omitted for `Count` aggregate. 
* `Function` - an aggregate function, can be one of `Average`, `Count`, `DistinctCount`, `Max`, `Min`, `Sum`.
* `ResultColumnName` - a name assigned to the aggregate expression in the result set.

### Value randomization rule<a name="value_randomization_rule"></a>

When `RandomizeValue` is `true` on a query condition or on a raw query parameter, this structure defines how the value should be generated.

* `UseExistingValues` - specifies if the value should be randomly picked from already existing values in this column. Is `true` by default.
* `GeneratorOptions` - specifies value generator type and its options. See [generators](#generators) for more details. Can be omitted only if `UseExistingValues` is set to `true`.
* `MinCollectionLength` - minimum random collection length. Default value is `1`.
* `MaxCollectionLength` - maximum random collection length. Default value is `10`.

The last two options are currently used to produce values for the `In` operator only. 

### Generators<a name="generators"></a>
Generators can be used for creating test data and generating query parameter values in benchmarks. Each generator is configured using the "Generator Options" structure, which includes a mandatory `Type` field. The characteristics and parameters of a generator vary based on its specific type. 

Detailed descriptions of each generator type are provided in the following sub-sections.

#### Address
Generates geographical attributes. The following values are available for its `Kind` attribute:
* `BuildingNumber`
* `City`
* `Country`
* `County`
* `FullAddress`
* `Latitude`
* `Longitude`
* `SecondaryAddress`
* `State`
* `StreetAddress`
* `StreetName`
* `ZipCode`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### Boolean
Generates a random Boolean. Its `Weight` attribute specifies the probability of the `true` value in the range between 0 and 1.

#### ColumnItem
Randomly picks a value from a list retrieved from a table column in the target database. Uses the same logic as the [ListItem generator](#listitem_generator), but a different primary source.

The available attributes are:
* `TableName` - name of the source table.
* `ColumnName` - name of the source column.
* `ColumnType` - source column type.
* `Distinct` - specifies whether to apply a distinct value filter when retrieving data from the source column.
* `WeightedItems` - see [ListItem generator](#listitem_generator) for more details.
* `MaxSourceRows` - specifies the maximum number of rows to collect from the source table. It is useful when dealing with a source table that contains a large number of rows, but there is no need to process the entire dataset. Setting this parameter helps reduce memory usage and decrease the startup time for the tool. The default value is `0` meaning that there is no limit.
* `SkipSourceRows` - specifies the number of rows to skip in the data source after collecting each row. This may be useful when the subset of source rows, limited by the `MaxSourceRows` parameter, needs to be distributed across the source dataset. If set to `0`, all rows are retrieved consecutively.

#### ColumnIterator
Sequentially returns each item from a list retrieved from a table column in the target database. Uses the same logic as the [ListIterator generator](#listiterator_generator), but a different primary source.

The available attributes are:
* `TableName` - a name of the source table.
* `ColumnName` - a name of the source column.
* `ColumnType` - a source column type.
* `Distinct` - specifies whether to apply a distinct value filter when retrieving data from the source column.

#### Company
Generates some company attributes. The following values are available for its `Kind` attribute:
* `CompanySuffix`
* `CompanyName`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### Constant

Returns a fixed constant specified by the `Value` parameter.

#### DataSourceIterator
Sequentially returns values of a column from the specified data source. Multiple generator instances with the same data source type and file path share the same iterator, thus allowing to fetch entire rows from the data source. It can be useful in scenarios when the source data needs to be enriched with randomly generated columns: in this case, `DataSourceIterator` will be used to fetch the raw data while others can add extra generated columns to the target data set.

The available attributes are:
* `DataSourceType` - a [data source](#data_sources) type.
* `DataSourceFilePath` - a path to the [data source](#data_sources) file.
* `ColumnName` - a name of the column to be returned.

#### DateTime
Generates random date/time value. The available attributes are:
* `MinValue` - minimum value. Defaults to the current date and time.
* `MaxValue` - maximum value. Defaults to the year after the current date and time.
* `Direction` - specifies the growth direction for generated values - `None`, `Ascending`, or `Descending`. If set to `Ascending`, the sequence of generated values starts from `MinValue` and grows until `MaxValue`. With `Descending`, it starts from `MaxValue` and goes in the opposite direction until `MinValue`. Default is `None`.
* `Delta` - when `Direction` is not `None`, this parameter determines the increment for each subsequently generated value or the maximum increment if `RandomizeDelta` is also enabled. If `Direction` is `None`, it specifies the fixed interval between all possible generated values. For example, with a `MinValue` of `2020-01-01 13:00:00`, a `MaxValue` of `2020-01-04 13:00:00`, and a `Delta` of `1.00:00:00` (one day), the utility will generate values from January 1st to January 4th, 2020, all at `13:00:00`, in random order. The value of `00:00:00` is allowed only when `Direction` is `None` and means no restriction on the set of generated values other than the minimum and maximum. The default value is `00:00:00`.

#### Finance
Generates finance-related pieces of information. The following values are available for its  `Kind`  attribute:
* `Bic`
* `BitcoinAddress`
* `CreditCardCvv`
* `CreditCardNumber`
* `Currency`
* `EthereumAddress`
* `Iban`

#### Float<a name="#float_generator"></a>
Generates random floating-point numbers. The available attributes are:
* `MinValue` - minimum value. Default is `0`.
* `MaxValue` - maximum value. Default is `100`.
* `Direction` - specifies the growth direction for generated values - `None`, `Ascending`, or `Descending`. If set to `Ascending`, the sequence of generated values starts from `MinValue` and grows until `MaxValue`. With `Descending`, it starts from `MaxValue` and goes in the opposite direction until `MinValue`. Default is `None`.
* `Delta` - when `Direction` is not `None`, this parameter determines the increment for each subsequently generated value or the maximum increment if `RandomizeDelta` is also enabled. If `Direction` is `None`, it specifies the fixed interval between all possible generated values. For instance, with a `MinValue` of 10, a `MaxValue` of 30, and a `Delta` of 5, the utility will generate the values 10, 15, 20, 25, and 30 in random order. The value of `0` is allowed only if `Direction` is `None` and means no restriction on the set of generated values apart from minimum and maximum. The default value is `0`.

#### Guid
Generates a random GUID.

#### Integer
Generates random integers. The available attributes are the same as in the [Float](#float_generator) generator.

#### Internet
Generates Internet-related pieces of information. The following values are available for its  `Kind`  attribute:
* `DomainName`
* `Email`
* `Ip`
* `Ipv6`
* `Mac`
* `Port`
* `Url`
* `UserAgent`
* `UserName`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### ListItem<a name="listitem_generator"></a>
Randomly picks a value from the provided list. The available attributes are:

* `Items` - a list of values from which one must be randomly picked.
* `WeightedItems` - a list of objects providing values along with their probabilities, from which one must be randomly picked. Each object has two attributes, where `Value` is a value and `Weight` is its probability in the range between 0 and 1.

At least one of the attributes from above must be provided. The sum of probabilities in the `WeightedItems` attribute must not exceed 1. If both attributes are specified, the generator calculates the total probability in `WeightedItems`, subtracts it from 1, and evenly distributes the result between elements of the `Items` collection. Therefore, if all probabilities in the `WeightedItems` collection add up to 1, no values from the `Items` collection will ever be produced. Using both attributes provides a way to boost the probability of a few terms from a large list.

#### ListIterator<a name="listiterator_generator"></a>
Sequentially returns each item from the provided list. Once the end of the list is reached, the data generation process stops, so that no generated queries or data source rows can be further produced.

The available attributes are:
* `Items` - a list of values from which each one should be sequentially picked.

#### Name
Generates random names. The following values are available for its  `Kind`  attribute:
* `FirstName`
* `LastName`
* `FullName`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### Null
A nested generator designed to integrate the functionality of any other generator with the capability to generate null values.

The available attributes are:
* `Weight` - the probability of a null value. The allowed range for this parameter is between 0 and 1. Default value is `0.5`.
* `SourceGeneratorOptions` - the underlying generator options - can be any of the other types.

#### Pattern

Generates a string according to a pattern specified by the `Pattern` parameter. The pattern can use any of the following three wildcard characters:

* `#` - a digit.
* `?` - a capital letter.
* `*` - either a digit or a capital letter.

For example, the pattern `??-####` will generate values like `JK-3276`.

#### Phone
Generates random phone numbers. The following values are available for its  `Kind`  attribute:
* `PhoneNumber`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### String
Generates random strings. The available attributes are:
* `MinLength` - minimum random string length. Default value is `1`
* `MaxLength` - maximum random string length. Default value is `10`
* `AllowedCharacters` - characters to be used when generating a random string. By default contains uppercase Latin letters and digits.

#### Text
Generates random pieces of text. The following values are available for its `Kind` attribute:
* `Word`
* `Sentence`
* `Paragraph`
* `Text`

The `Locale` parameter specifies the locale to be used for address generation. A list of supported values can be found on [the Bogus library page](https://github.com/bchavez/Bogus?tab=readme-ov-file#locales).

#### Unique
Enforces uniqueness of values produced by the underlying generator. Most of the generators provided here do not have uniqueness guarantees. This generator wraps any other generator and tracks the values it produces. If a newly generated value has been seen before, it calls the underlying generator again until a new unique value is produced or the number of attempts is exhausted.

The available attributes are:
* `AttemptCount` - the number of attempts to produce a unique value from the underlying generator. The default value is `100`.
* `SourceGeneratorOptions` - the underlying generator options - can be any of the other types.

#### Vehicle
Generates vehicle-related pieces of information. The following values are available for its  `Kind`  attribute:
* `Manufacturer`
* `Model`
* `Vin`
* `Fuel`
* `Type`

### Report columns<a name="report_columns"></a>
Each benchmark command has a couple of optional parameters - `ReportColumns` and `ReportCustomMetricColumns`. These parameters control the set of columns to be shown in the benchmark report.

For `ReportColumns` the available column identifiers are:
* `name` - benchmark name.
* `avg` - average query execution time.
* `stdDev` - standard deviation of the query execution time.
* `min` - minimum query execution time.
* `p10` - 10th percentile of the query execution time.
* `p50` - median query execution time.
* `p90` - 90th percentile of the query execution time.
* `max` - maximum query execution time.
* `qps` - average queries per second.
* `avgRows`  - average number of rows returned by the queries.

By default, all columns are shown except for `p10` and `p90`.

For `ReportCustomMetricColumns`:
* `avg` - average.
* `stdDev` - standard deviation.
* `min` - minimum.
* `max` - maximum.

By default, all four columns are shown for each custom metric.

Please note that the latter setting is applied to all custom metrics provided by a database plugin, so if only `avg` is specified, the average value will be printed for each custom metric.

### Connection strings<a name="connection_strings"></a>
The list below shows only basic examples. For more advanced connection settings please refer to the respective client library documentation.

**ClickHouse**

`Host=myhost;Port=9000;Database=default;Password=mypassword`

**Cosmos DB**

`AccountEndpoint=myendpoint;AccountKey=myaccountkey;Database=mydb`

**Dynamo DB**

`AccessKeyId=myaccountkeyid;SecretAccessKey=myaccountsecret;RegionEndpoint=myregionendpointcode`

All available region endpoint codes can be found in the [official DynamoDB documentation](https://docs.aws.amazon.com/general/latest/gr/rande.html).

**Elasticsearch**

`http://localhost:9200`

**MySQL**

`Server=myhost;Database=mydb;Uid=myuser;Pwd=mypassword;`

**MonetDB**

`Host=myhost;port=50000;Database=mydb;username=myuser;password=mypassword`

**MongoDB**

`mongodb://myhost/mydb`

**Oracle**

`Data Source=myhost:1521/XE;User Id=myuser;Password=mypassword`

**PostgreSQL**

`Host=myhost;Port=5432;Database=mydb;Username=myuser;Password=mypassword`

**Snowflake**

`account=myaccount;host=myhost;user=myuser;password=mypassword;db=mydb;schema=public;warehouse=mywarehouse`

**SQL Server**

`Data Source=.;Initial Catalog=mydb;Integrated Security=True;`

## Limitations<a name="limitations"></a>

There are some limitations that are going to be addressed in the future:

* Query definitions don't support joins. A workaround is using the raw queries approach.
* Random inclusion of condition parts is currently not supported for raw queries.
* Configurable partitioning is supported for Cosmos DB and DynamoDB only.
* Importing from Elasticsearch database doesn't support an unlimited number of rows.
* Generators do not currently support contextually dependent values. For example, the first name and full name values produced by the name generator for the same row of data won't correspond to each other.