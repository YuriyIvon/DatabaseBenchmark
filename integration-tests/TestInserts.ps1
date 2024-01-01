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

  & $toolPath insert --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json $tableNameParameter --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Output ""
}
