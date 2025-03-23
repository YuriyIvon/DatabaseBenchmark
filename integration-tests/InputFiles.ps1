$inputFiles = @{
  SqlServer = @{
    TableFile = "GeneratedSampleTable.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSource.json"
    QueryFile = "GeneratedSampleQuery.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawQueryParameters.json"
  }
  Postgres = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQueryWithArrays.txt"
    RawQueryParametersFile = "GeneratedSampleRawArrayQueryParameters.json"
  }
  PostgresJsonb = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = "GeneratedSampleJsonb"
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = $null
  }
  MySql = @{
    TableFile = "GeneratedSampleTableNoGuid.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceNoGuid.json"
    QueryFile = "GeneratedSampleQuery.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawQueryParameters.json"
  }
  Oracle = @{
    TableFile = "GeneratedSampleTable.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSource.json"
    QueryFile = "GeneratedSampleQuery.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawQueryParameters.json"
  }
  MongoDb = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = "GeneratedSample"
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = "GeneratedSampleAggregateRawMongoDbQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawArrayQueryParameters.json"
  }
  Elasticsearch = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = "GeneratedSampleAggregateRawElasticsearchQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawArrayQueryParameters.json"
  }
  ClickHouse = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQueryWithArrays.txt"
    RawQueryParametersFile = "GeneratedSampleRawArrayQueryParameters.json"
  }
  MonetDb = @{
    TableFile = "GeneratedSampleTableNoGuid.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceNoGuid.json"
    QueryFile = "GeneratedSampleQuery.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawQueryParameters.json"
  }
  Snowflake = @{
    TableFile = "GeneratedSampleTable.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSource.json"
    QueryFile = "GeneratedSampleQuery.json"
    RawQueryFile = "GeneratedSampleAggregateRawSqlQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawQueryParameters.json"
  }
  CosmosDb = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = "GeneratedSample"
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleQueryWithArrays.json"
    RawQueryFile = "GeneratedSampleAggregateRawCosmosDbQuery.txt"
    RawQueryParametersFile = "GeneratedSampleRawArrayQueryParameters.json"
  }
  DynamoDb = @{
    TableFile = "GeneratedSampleTableWithArrays.json"
    TableName = $null
    DataSourceFile = "GeneratedSampleDataSourceWithArrays.json"
    QueryFile = "GeneratedSampleDynamoDbQuery.json"
    RawQueryFile = $null
  }
}
