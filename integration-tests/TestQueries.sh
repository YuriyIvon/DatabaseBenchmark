#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net10.0/linux-x64/DatabaseBenchmark"

traceQueries="true"
traceResults="true"
queryCount=1

. ./connectionStrings.sh
. ./inputFiles.sh

for databaseType in "${!connectionStrings[@]}"; do
  echo "$databaseType"

  connectionString="${connectionStrings[$databaseType]}"
  tableFile="${inputFiles[${databaseType}_TableFile]}"
  dataSourceFile="${inputFiles[${databaseType}_DataSourceFile]}"
  queryFile="${inputFiles[${databaseType}_QueryFile]}"
  rawQueryFile="${inputFiles[${databaseType}_RawQueryFile]}"
  rawQueryParametersFile="${inputFiles[${databaseType}_RawQueryParametersFile]}"
  tableNameOverride="${inputFiles[${databaseType}_TableName]}"

  if [[ -n "$tableNameOverride" ]]; then
    tableNameParameter="--TableName=$tableNameOverride"
  else
    tableNameParameter=""
  fi

  "$toolPath" create \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/GeneratedUsersTable.json" \
    --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" import \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/GeneratedUsersTable.json" \
    --DataSourceType=Generator \
    --DataSourceFilePath="Definitions/GeneratedUsersDataSource.json" \
    --DataSourceMaxRows=100
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" create \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    $tableNameParameter \
    --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" import \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    $tableNameParameter \
    --DataSourceType=Generator \
    --DataSourceFilePath="Definitions/$dataSourceFile" \
    --DataSourceMaxRows=100
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" query \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    $tableNameParameter \
    --QueryFilePath="Definitions/$queryFile" \
    --QueryCount=$queryCount \
    --QueryParallelism=1 \
    --TraceQueries="$traceQueries" \
    --TraceResults="$traceResults"
  if [[ $? -ne 0 ]]; then exit $?; fi

  if [[ -n "$rawQueryFile" ]]; then
    "$toolPath" raw-query \
      --DatabaseType="$databaseType" \
      --ConnectionString="$connectionString" \
      $tableNameParameter \
      --QueryFilePath="Definitions/$rawQueryFile" \
      --QueryParametersFilePath="Definitions/$rawQueryParametersFile" \
      --QueryCount=$queryCount \
      --QueryParallelism=1 \
      --TraceQueries="$traceQueries" \
      --TraceResults="$traceResults"
    if [[ $? -ne 0 ]]; then exit $?; fi
  fi

  echo ""
done