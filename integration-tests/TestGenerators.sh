#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"

source ./connectionStrings.sh
source ./inputFiles.sh

for databaseType in "${!connectionStrings[@]}"; do
  echo "$databaseType"

  connectionString="${connectionStrings[$databaseType]}"
  tableFile="${inputFiles[${databaseType}_TableFile]}"
  tableNameOverride="${inputFiles[${databaseType}_TableName]}"
  dataSourceFile="${inputFiles[${databaseType}_DataSourceFile]}"

  if [[ -n "$tableNameOverride" ]]; then
    tableNameParameter="--TableName=$tableNameOverride"
  else
    tableNameParameter=""
  fi

  "$toolPath" create --DatabaseType="$databaseType" --ConnectionString="$connectionString" \
    --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" import --DatabaseType="$databaseType" --ConnectionString="$connectionString" \
    --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator \
    --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" create --DatabaseType="$databaseType" --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" $tableNameParameter --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" import --DatabaseType="$databaseType" --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" $tableNameParameter --DataSourceType=Generator \
    --DataSourceFilePath="Definitions/$dataSourceFile" --DataSourceMaxRows=100
  if [[ $? -ne 0 ]]; then exit $?; fi

  echo ""
done
