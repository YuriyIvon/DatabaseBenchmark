#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"

. ConnectionStrings.sh
. QueryFiles.sh

for databaseType in "${!connectionStrings[@]}"
do
  echo "$databaseType"

  connectionString=${connectionStrings[$databaseType]}

  $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --DropExisting=true
  if [ $? -ne 0 ]; then exit $?; fi

  $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if [ $? -ne 0 ]; then exit $?; fi

  queryFile=${queryFiles[$databaseType]}
  $toolPath query --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --QueryFilePath="Definitions/$queryFile" --QueryCount=1 --QueryParallelism=1
  if [ $? -ne 0 ]; then exit $?; fi

  rawQueryFile=${rawQueryFiles[$databaseType]}
  if [ -n "$rawQueryFile" ]; then
      $toolPath raw-query --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableName="Sales" --QueryFilePath="Definitions/$rawQueryFile" --QueryParametersFilePath=Definitions/SalesAggregateRawQueryParameters.json --QueryCount=1 --QueryParallelism=1
      if [ $? -ne 0 ]; then exit $?; fi
  fi

  echo ""
done