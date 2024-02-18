$databaseType="Postgres"
$connectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=password"

$userCount = 100
$postCount = 1000

$toolPath="..\..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"

& $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=UsersTable.json --DropExisting=true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=UsersTable.json --DataSourceType=Generator --DataSourceFilePath=UsersDataSource.json --DataSourceMaxRows=$userCount
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=PostsTable.json --DropExisting=true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=PostsTable.json --DataSourceType=Generator --DataSourceFilePath=PostsDataSource.json --DataSourceMaxRows=$postCount
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=EnrichedUsersTable.json --DropExisting=true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=EnrichedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=EnrichedUsersDataSource.json
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
