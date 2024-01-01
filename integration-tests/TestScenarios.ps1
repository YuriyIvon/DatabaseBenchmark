$toolPath="..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"
$queryScenarioFile = "Definitions\SalesQueryScenario.json"
$rawQueryScenarioFile = "Definitions\SalesRawQueryScenario.json"

. .\ConnectionStrings.ps1
. .\QueryFiles.ps1

Write-Output "Populating databases"
Write-Output ""

@'
{
  "Name": "Sales scenario",
  "Items": [
'@ | Out-File -FilePath $queryScenarioFile -Encoding UTF8

@'
{
  "Name": "Sales scenario",
  "Items": [
'@ | Out-File -FilePath $rawQueryScenarioFile -Encoding UTF8

$first = $true

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $queryFile = $queryFiles[$databaseType]
  $rawQueryFile = $rawQueryFiles[$databaseType]
  $tableName = if ($tableNameOverrides[$databaseType] -ne $null) { $tableNameOverrides[$databaseType] } else { "Sales" }

@"
    $(if ($first) {''} else {','})
    {
      "BenchmarkName": "$databaseType Page Query With Range",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "QueryFilePath": "$queryFile",
      "TableName": "$tableName"
    }
"@ | Out-File -FilePath $queryScenarioFile -Append -Encoding UTF8

  if ($rawQueryFile -ne $null)
  {
@"
    $(if ($first) {''} else {','})
    {
      "BenchmarkName": "$databaseType Page Query With Range",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "QueryFilePath": "$rawQueryFile",
      "TableName": "$tableName"
    }
"@ | Out-File -FilePath $rawQueryScenarioFile -Append -Encoding UTF8
  }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --TableName="$tableName" --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --TableName="$tableName" --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Output ""

  $first = $false
}

@'
  ]
}
'@ | Out-File -FilePath $queryScenarioFile -Append -Encoding UTF8

@'
  ]
}
'@ | Out-File -FilePath $rawQueryScenarioFile -Append -Encoding UTF8

& $toolPath query-scenario --QueryScenarioFilePath=Definitions/SalesQueryScenario.json --QueryScenarioParametersFilePath=Definitions/SalesQueryScenarioParameters.json

& $toolPath raw-query-scenario --QueryScenarioFilePath=Definitions/SalesRawQueryScenario.json --QueryScenarioParametersFilePath=Definitions/SalesQueryScenarioParameters.json