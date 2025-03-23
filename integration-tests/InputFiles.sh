declare -A inputFiles

# SqlServer
inputFiles[SqlServer_TableFile]="GeneratedSampleTable.json"
inputFiles[SqlServer_TableName]=""
inputFiles[SqlServer_DataSourceFile]="GeneratedSampleDataSource.json"
inputFiles[SqlServer_QueryFile]="GeneratedSampleQuery.json"
inputFiles[SqlServer_RawQueryFile]="GeneratedSampleAggregateRawSqlQuery.txt"
inputFiles[SqlServer_RawQueryParametersFile]="GeneratedSampleRawQueryParameters.json"

# Postgres
inputFiles[Postgres_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[Postgres_TableName]=""
inputFiles[Postgres_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[Postgres_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[Postgres_RawQueryFile]="GeneratedSampleAggregateRawSqlQueryWithArrays.txt"
inputFiles[Postgres_RawQueryParametersFile]="GeneratedSampleRawArrayQueryParameters.json"

# PostgresJsonb
inputFiles[PostgresJsonb_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[PostgresJsonb_TableName]="GeneratedSampleJsonb"
inputFiles[PostgresJsonb_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[PostgresJsonb_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[PostgresJsonb_RawQueryFile]=""
inputFiles[PostgresJsonb_RawQueryParametersFile]=""

# MySql
inputFiles[MySql_TableFile]="GeneratedSampleTableNoGuid.json"
inputFiles[MySql_TableName]=""
inputFiles[MySql_DataSourceFile]="GeneratedSampleDataSourceNoGuid.json"
inputFiles[MySql_QueryFile]="GeneratedSampleQuery.json"
inputFiles[MySql_RawQueryFile]="GeneratedSampleAggregateRawSqlQuery.txt"
inputFiles[MySql_RawQueryParametersFile]="GeneratedSampleRawQueryParameters.json"

# Oracle
inputFiles[Oracle_TableFile]="GeneratedSampleTable.json"
inputFiles[Oracle_TableName]=""
inputFiles[Oracle_DataSourceFile]="GeneratedSampleDataSource.json"
inputFiles[Oracle_QueryFile]="GeneratedSampleQuery.json"
inputFiles[Oracle_RawQueryFile]="GeneratedSampleAggregateRawSqlQuery.txt"
inputFiles[Oracle_RawQueryParametersFile]="GeneratedSampleRawQueryParameters.json"

# MongoDb
inputFiles[MongoDb_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[MongoDb_TableName]="GeneratedSample"
inputFiles[MongoDb_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[MongoDb_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[MongoDb_RawQueryFile]="GeneratedSampleAggregateRawMongoDbQuery.txt"
inputFiles[MongoDb_RawQueryParametersFile]="GeneratedSampleRawArrayQueryParameters.json"

# Elasticsearch
inputFiles[Elasticsearch_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[Elasticsearch_TableName]=""
inputFiles[Elasticsearch_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[Elasticsearch_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[Elasticsearch_RawQueryFile]="GeneratedSampleAggregateRawElasticsearchQuery.txt"
inputFiles[Elasticsearch_RawQueryParametersFile]="GeneratedSampleRawArrayQueryParameters.json"

# ClickHouse
inputFiles[ClickHouse_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[ClickHouse_TableName]=""
inputFiles[ClickHouse_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[ClickHouse_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[ClickHouse_RawQueryFile]="GeneratedSampleAggregateRawSqlQueryWithArrays.txt"
inputFiles[ClickHouse_RawQueryParametersFile]="GeneratedSampleRawArrayQueryParameters.json"

# MonetDb
inputFiles[MonetDb_TableFile]="GeneratedSampleTableNoGuid.json"
inputFiles[MonetDb_TableName]=""
inputFiles[MonetDb_DataSourceFile]="GeneratedSampleDataSourceNoGuid.json"
inputFiles[MonetDb_QueryFile]="GeneratedSampleQuery.json"
inputFiles[MonetDb_RawQueryFile]="GeneratedSampleAggregateRawSqlQuery.txt"
inputFiles[MonetDb_RawQueryParametersFile]="GeneratedSampleRawQueryParameters.json"

# Snowflake
inputFiles[Snowflake_TableFile]="GeneratedSampleTable.json"
inputFiles[Snowflake_TableName]=""
inputFiles[Snowflake_DataSourceFile]="GeneratedSampleDataSource.json"
inputFiles[Snowflake_QueryFile]="GeneratedSampleQuery.json"
inputFiles[Snowflake_RawQueryFile]="GeneratedSampleAggregateRawSqlQuery.txt"
inputFiles[Snowflake_RawQueryParametersFile]="GeneratedSampleRawQueryParameters.json"

# CosmosDb
inputFiles[CosmosDb_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[CosmosDb_TableName]="GeneratedSample"
inputFiles[CosmosDb_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[CosmosDb_QueryFile]="GeneratedSampleQueryWithArrays.json"
inputFiles[CosmosDb_RawQueryFile]="GeneratedSampleAggregateRawCosmosDbQuery.txt"
inputFiles[CosmosDb_RawQueryParametersFile]="GeneratedSampleRawArrayQueryParameters.json"

# DynamoDb
inputFiles[DynamoDb_TableFile]="GeneratedSampleTableWithArrays.json"
inputFiles[DynamoDb_TableName]=""
inputFiles[DynamoDb_DataSourceFile]="GeneratedSampleDataSourceWithArrays.json"
inputFiles[DynamoDb_QueryFile]="GeneratedSampleDynamoDbQuery.json"
inputFiles[DynamoDb_RawQueryFile]=""
inputFiles[DynamoDb_RawQueryParametersFile]=""