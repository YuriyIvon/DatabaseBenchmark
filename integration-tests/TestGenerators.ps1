$toolPath="..\src\DatabaseBenchmark\bin\Debug\net8.0\DatabaseBenchmark"

. .\ConnectionStrings.ps1
. .\InputFiles.ps1

foreach ($databaseType in $connectionStrings.Keys)
{
  Write-Output $databaseType

  $connectionString = $connectionStrings[$databaseType]
  $inputFilesItem = $inputFiles[$databaseType]
  $tableNameOverride = $inputFilesItem['TableName'] 
  $tableFile = $inputFilesItem['TableFile']
  $dataSourceFile = $inputFilesItem['DataSourceFile']
  $tableNameParameter = if ($tableNameOverride -ne $null) { "--TableName=$tableNameOverride" } else { "" }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$tableFile $tableNameParameter --DropExisting=true
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  & $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/$tableFile $tableNameParameter --DataSourceType=Generator --DataSourceFilePath=Definitions/$dataSourceFile --DataSourceMaxRows=100
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  Write-Output ""
}
