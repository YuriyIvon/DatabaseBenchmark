$toolPath="..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"

. .\ConnectionStrings.ps1

$tableNameOverrides = @{
  PostgresJsonb = "GeneratedSampleJsonb"
}

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $tableNameOverride = $tableNameOverrides[$databaseType]
  $tableNameParameter = if ($tableNameOverride -ne $null) { "--TableName=$tableNameOverride" } else { "" }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedSampleTable.json $tableNameParameter --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedSampleTable.json $tableNameParameter --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedSampleDataSource.json --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Output ""
}
