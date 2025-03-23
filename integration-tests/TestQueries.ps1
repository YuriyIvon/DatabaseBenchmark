$toolPath="..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"

$traceQueries = "true"
$traceResults = "true"
$queryCount = 1

. .\ConnectionStrings.ps1
. .\InputFiles.ps1

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $inputFilesItem = $inputFiles[$databaseType]
  $tableNameOverride = $inputFilesItem['TableName']
  $tableNameParameter = if ($tableNameOverride -ne $null) { "--TableName=$tableNameOverride" } else { "" }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$($inputFilesItem['TableFile']) $tableNameParameter --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$($inputFilesItem['TableFile']) $tableNameParameter --DataSourceType=Generator --DataSourceFilePath=Definitions/$($inputFilesItem['DataSourceFile']) --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath query --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$($inputFilesItem['TableFile']) $tableNameParameter --QueryFilePath=Definitions/$($inputFilesItem['QueryFile']) --QueryCount=$queryCount --QueryParallelism=1 --TraceQueries=$traceQueries --TraceResults=$traceResults
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  $rawQueryFile = $inputFilesItem['RawQueryFile']
  if ($rawQueryFile -ne $null)
  {
    $parametersFile = $inputFilesItem['RawQueryParametersFile']
    & $toolPath raw-query --DatabaseType=$databaseType --ConnectionString="$connectionString" $tableNameParameter --QueryFilePath=Definitions/$rawQueryFile --QueryParametersFilePath=Definitions/$parametersFile --QueryCount=$queryCount --QueryParallelism=1 --TraceQueries=$traceQueries --TraceResults=$traceResults
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }

  Write-Output ""
}
