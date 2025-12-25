$toolPath="..\src\DatabaseBenchmark\bin\Debug\net10.0\DatabaseBenchmark"

$traceQueries = "false"

. .\ConnectionStrings.ps1
. .\InputFiles.ps1

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $inputFilesItem = $inputFiles[$databaseType]
  $tableNameOverride = $inputFilesItem['TableName']
  $tableNameParameter = if ($tableNameOverride -ne $null) { "--TableName=$tableNameOverride" } else { "" }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$($inputFilesItem['TableFile']) $tableNameParameter --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath insert --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$($inputFilesItem['TableFile']) $tableNameParameter --DataSourceType=Generator --DataSourceFilePath=Definitions/$($inputFilesItem['DataSourceFile']) --DataSourceMaxRows=100 --TraceQueries=$traceQueries
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Output ""
}
