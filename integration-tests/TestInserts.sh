#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"

traceQueries="false"

. ./connectionStrings.sh
. ./inputFiles.sh

for databaseType in "${!connectionStrings[@]}"; do
  echo "$databaseType"

  connectionString="${connectionStrings[$databaseType]}"
  tableFile="${inputFiles[${databaseType}_TableFile]}"
  dataSourceFile="${inputFiles[${databaseType}_DataSourceFile]}"
  tableNameOverride="${inputFiles[${databaseType}_TableName]}"

  if [[ -n "$tableNameOverride" ]]; then
    tableNameParameter="--TableName=$tableNameOverride"
  else
    tableNameParameter=""
  fi

  "$toolPath" create \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    $tableNameParameter \
    --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" insert \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    $tableNameParameter \
    --DataSourceType=Generator \
    --DataSourceFilePath="Definitions/$dataSourceFile" \
    --DataSourceMaxRows=100 \
    --TraceQueries="$traceQueries"
  if [[ $? -ne 0 ]]; then exit $?; fi

  echo ""
done