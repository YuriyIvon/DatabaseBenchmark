$toolPath="..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"

. .\ConnectionStrings.ps1
. .\QueryFiles.ps1

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $tableNameOverride = $tableNameOverrides[$databaseType]
  $tableNameParameter = if ($tableNameOverride -ne $null) { "--TableName=$tableNameOverride" } else { "" }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json $tableNameParameter --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json $tableNameParameter --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  $queryFile = $queryFiles[$databaseType]
  & $toolPath query --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json $tableNameParameter --QueryFilePath=Definitions/$queryFile --QueryCount=1 --QueryParallelism=1
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  $rawQueryFile = $rawQueryFiles[$databaseType]
  if ($rawQueryFile -ne $null)
  {
    & $toolPath raw-query --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableName="Sales" --QueryFilePath=Definitions/$rawQueryFile --QueryParametersFilePath=Definitions/SalesAggregateRawQueryParameters.json --QueryCount=1 --QueryParallelism=1
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }

  Write-Output ""
}
