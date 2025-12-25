$toolPath="..\src\DatabaseBenchmark\bin\Debug\net10.0\DatabaseBenchmark"
$queryScenarioFile = "Definitions\SampleQueryScenario.json"
$rawQueryScenarioFile = "Definitions\SampleRawQueryScenario.json"

. .\ConnectionStrings.ps1
. .\InputFiles.ps1

Write-Output "Populating databases"
Write-Output ""

@'
{
  "Name": "Sample scenario",
  "Items": [
'@ | Out-File -FilePath $queryScenarioFile -Encoding UTF8

@'
{
  "Name": "Sample scenario",
  "Items": [
'@ | Out-File -FilePath $rawQueryScenarioFile -Encoding UTF8

$first = $true

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $inputFilesItem = $inputFiles[$databaseType]
  $tableFile = $inputFilesItem['TableFile']
  $tableNameOverride = $inputFilesItem['TableName']
  $queryFile = $inputFilesItem['QueryFile']
  $rawQueryFile = $inputFilesItem['RawQueryFile']
  $rawQueryParametersFile = $inputFilesItem['RawQueryParametersFile']
  $tableName = if ($tableNameOverride -ne $null) { $tableNameOverride } else { "GeneratedSample" }

@"
    $(if ($first) {''} else {','})
    {
      "BenchmarkName": "$databaseType Sample Query",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "TableFilePath": "$tableFile",
      "QueryFilePath": "$queryFile",
      "TableName": "$tableName"
    }
"@ | Out-File -FilePath $queryScenarioFile -Append -Encoding UTF8

  if ($rawQueryFile -ne $null)
  {
@"
    $(if ($first) {''} else {','})
    {
      "BenchmarkName": "$databaseType Sample Raw Query",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "QueryFilePath": "$rawQueryFile",
      "QueryParametersFilePath": "$rawQueryParametersFile",
      "TableName": "$tableName"
    }
"@ | Out-File -FilePath $rawQueryScenarioFile -Append -Encoding UTF8
  }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$tableFile --TableName="$tableName" --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$tableFile --TableName="$tableName" --DataSourceType=Generator --DataSourceFilePath=Definitions/$($inputFilesItem['DataSourceFile']) --DataSourceMaxRows=1000
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

& $toolPath query-scenario --QueryScenarioFilePath=$queryScenarioFile --QueryScenarioParametersFilePath=Definitions/SampleQueryScenarioParameters.json

& $toolPath raw-query-scenario --QueryScenarioFilePath=$rawQueryScenarioFile --QueryScenarioParametersFilePath=Definitions/SampleQueryScenarioParameters.json