declare -A queryFiles=(
  ["SqlServer"]="SalesPageWithRangeQuery.json"
  ["Postgres"]="SalesPageWithRangeQuery.json"
  ["PostgresJsonb"]="SalesPageWithRangeQuery.json"
  ["MySql"]="SalesPageWithRangeQuery.json"
  ["Oracle"]="SalesPageWithRangeQuery.json"
  ["MongoDb"]="SalesPageWithRangeQuery.json"
  ["Elasticsearch"]="SalesPageWithRangeQuery.json"
  ["ClickHouse"]="SalesPageWithRangeQuery.json"
  ["MonetDb"]="SalesPageWithRangeQuery.json"
  ["Snowflake"]="SalesPageWithRangeQuery.json"
  ["CosmosDb"]="SalesPageWithRangeQuery.json"
  ["DynamoDb"]="SalesPageWithRangeDynamoDbQuery.json"
)

declare -A rawQueryFiles=(
  ["SqlServer"]="SalesAggregateRawSqlQuery.txt"
  ["Postgres"]="SalesAggregateRawSqlQuery.txt"
  ["MySql"]="SalesAggregateRawSqlQuery.txt"
  ["Oracle"]="SalesAggregateRawSqlQuery.txt"
  ["MongoDb"]="SalesAggregateRawMongoDbQuery.txt"
  ["ClickHouse"]="SalesAggregateRawSqlQuery.txt"
  ["MonetDb"]="SalesAggregateRawSqlQuery.txt"
  ["Snowflake"]="SalesAggregateRawSqlQuery.txt"
  ["CosmosDb"]="SalesAggregateRawCosmosDbQuery.txt"
)

declare -A tableNameOverrides=(
  ["PostgresJsonb"]="SalesJsonb"
)
