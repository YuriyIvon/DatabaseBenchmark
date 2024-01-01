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

  $toolPath insert --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if [ $? -ne 0 ]; then exit $?; fi

  echo ""
done