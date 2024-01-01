#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"
queryScenarioFile="Definitions/SalesQueryScenario.json"
rawQueryScenarioFile="Definitions/SalesRawQueryScenario.json"

. ConnectionStrings.sh
. QueryFiles.sh

echo "Populating databases"
echo ""

echo '{
  "Name": "Sales scenario",
  "Items": [
' > "$queryScenarioFile"

echo '{
  "Name": "Sales scenario",
  "Items": [
' > "$rawQueryScenarioFile"

first=1

for databaseType in "${!connectionStrings[@]}"
do
  echo "$databaseType"

  connectionString=${connectionStrings[$databaseType]}
  queryFile=${queryFiles[$databaseType]}
  rawQueryFile=${rawQueryFiles[$databaseType]}

  if [[ -n "${tableNameOverrides[$databaseType]}" ]]; then
    tableName="${tableNameOverrides[$databaseType]}"
  else
    tableName="Sales"
  fi

  if [ "$first" -ne 1 ]; then
    echo "," >> "$queryScenarioFile"
  fi

  echo "    {
      \"BenchmarkName\": \"$databaseType Page Query With Range\",
      \"DatabaseType\": \"$databaseType\",
      \"ConnectionString\": \"$connectionString\",
      \"QueryFilePath\": \"$queryFile\",
      \"TableName\": \"$tableName\"
    }" >> "$queryScenarioFile"

  if [ -n "$rawQueryFile" ]; then

    if [ "$first" -ne 1 ]; then
      echo "," >> "$rawQueryScenarioFile"
    fi

    echo "    {
      \"BenchmarkName\": \"$databaseType Page Query With Range\",
      \"DatabaseType\": \"$databaseType\",
      \"ConnectionString\": \"$connectionString\",
      \"QueryFilePath\": \"$rawQueryFile\",
      \"TableName\": \"$tableName\"
    }" >> "$rawQueryScenarioFile"
  fi

  $toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --TableName="$tableName" --DropExisting=true
  if [ $? -ne 0 ]; then exit $?; fi

  $toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=Definitions/SalesTable.json --MappingFilePath=Definitions/SalesTableMapping.json --DataSourceType=Csv --DataSourceFilePath="Definitions/1000 Sales Records.csv"
  if [ $? -ne 0 ]; then exit $?; fi
   
  echo ""

  first=0
done

echo '  ]
}
' >> "$queryScenarioFile"

echo '  ]
}
' >> "$rawQueryScenarioFile"

$toolPath query-scenario --QueryScenarioFilePath=Definitions/SalesQueryScenario.json --QueryScenarioParametersFilePath=Definitions/SalesQueryScenarioParameters.json

$toolPath raw-query-scenario --QueryScenarioFilePath=Definitions/SalesRawQueryScenario.json --QueryScenarioParametersFilePath=Definitions/SalesQueryScenarioParameters.json