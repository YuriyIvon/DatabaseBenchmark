#Update the sample connection strings with your specific connection settings
$connectionStrings = @{
  AzureSearch = "Endpoint=myendpoint;ApiKey=myapikey"
  SqlServer = "Data Source=.;Initial Catalog=benchmark;Integrated Security=True;"
  Postgres = "Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=password"
  PostgresJsonb = "Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=password"
  MySql = "Server=localhost;Database=benchmark;Uid=root;Pwd=password;"
  Oracle = "Data Source=localhost:1521/XE;User Id=SYSTEM;Password=password;Max Pool Size=100;Min Pool Size=1"
  MongoDb = "mongodb://localhost/benchmark"
  Elasticsearch = "http://localhost:9200"
  ClickHouse = "Host=localhost;Port=9000;Database=default;Password=password"
  MonetDb = "Host=localhost;port=50000;Database=demo;username=monetdb;password=monetdb"
  Snowflake = "account=myaccount;host=myhost;user=myuser;password=mypassword;db=mydb;schema=public;warehouse=mywarehouse"
  CosmosDb = "AccountEndpoint=myendpoint;AccountKey=myaccountkey;Database=mydb"
  DynamoDb = "AccessKeyId=myaccountkeyid;SecretAccessKey=myaccountsecret;RegionEndpoint=myregionendpointcode"
}