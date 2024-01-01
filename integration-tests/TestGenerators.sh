#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"

. ConnectionStrings.sh

declare -A tableNameOverrides=(
  ["PostgresJsonb"]="GeneratedSampleJsonb"
)

for databaseType in "${!connectionStrings[@]}"
do
  echo $databaseType

  connectionString=${connectionStrings[$databaseType]}
  tableNameOverride=${tableNameOverrides[$databaseType]}

  if [ -n "$tableNameOverride" ]; then
    tableNameParameter="--TableName=$tableNameOverride"
  else
    tableNameParameter=""
  fi

  $toolPath create --DatabaseType=${databaseType} --ConnectionString="${connectionString}" --TableFilePath=Definitions/GeneratedUsersTable.json --DropExisting=true
  if [ $? -ne 0 ]; then
    exit $?
  fi

  $toolPath import --DatabaseType=${databaseType} --ConnectionString="${connectionString}" --TableFilePath=Definitions/GeneratedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedUsersDataSource.json --DataSourceMaxRows=100
  if [ $? -ne 0 ]; then
    exit $?
  fi

  $toolPath create --DatabaseType=${databaseType} --ConnectionString="${connectionString}" --TableFilePath=Definitions/GeneratedSampleTable.json $tableNameParameter --DropExisting=true
  if [ $? -ne 0 ]; then
    exit $?
  fi

  $toolPath import --DatabaseType=${databaseType} --ConnectionString="${connectionString}" --TableFilePath=Definitions/GeneratedSampleTable.json $tableNameParameter --DataSourceType=Generator --DataSourceFilePath=Definitions/GeneratedSampleDataSource.json --DataSourceMaxRows=100
  if [ $? -ne 0 ]; then
    exit $?
  fi

  echo ""
done