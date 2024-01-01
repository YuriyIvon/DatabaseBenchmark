$queryFiles = @{
  SqlServer = "SalesPageWithRangeQuery.json"
  Postgres = "SalesPageWithRangeQuery.json"
  PostgresJsonb = "SalesPageWithRangeQuery.json"
  MySql = "SalesPageWithRangeQuery.json"
  Oracle = "SalesPageWithRangeQuery.json"
  MongoDb = "SalesPageWithRangeQuery.json"
  Elasticsearch = "SalesPageWithRangeQuery.json"
  ClickHouse = "SalesPageWithRangeQuery.json"
  MonetDb = "SalesPageWithRangeQuery.json"
  Snowflake = "SalesPageWithRangeQuery.json"
  CosmosDb = "SalesPageWithRangeQuery.json"
  DynamoDb = "SalesPageWithRangeDynamoDbQuery.json"
}

$rawQueryFiles = @{
  SqlServer = "SalesAggregateRawSqlQuery.txt"
  Postgres = "SalesAggregateRawSqlQuery.txt"
  PostgresJsonb = $null
  MySql = "SalesAggregateRawSqlQuery.txt"
  Oracle = "SalesAggregateRawSqlQuery.txt"
  MongoDb = "SalesAggregateRawMongoDbQuery.txt"
  Elasticsearch = $null
  ClickHouse = "SalesAggregateRawSqlQuery.txt"
  MonetDb = "SalesAggregateRawSqlQuery.txt"
  Snowflake = "SalesAggregateRawSqlQuery.txt"
  CosmosDb = "SalesAggregateRawCosmosDbQuery.txt"
  DynamoDb = $null
}

$tableNameOverrides = @{
  PostgresJsonb = "SalesJsonb"
}
