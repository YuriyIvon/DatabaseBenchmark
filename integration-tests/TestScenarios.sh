#!/bin/bash

toolPath="../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"
queryScenarioFile="Definitions/SampleQueryScenario.json"
rawQueryScenarioFile="Definitions/SampleRawQueryScenario.json"

. ./connectionStrings.sh
. ./inputFiles.sh

echo "Populating databases"
echo ""

cat <<EOF > "$queryScenarioFile"
{
  "Name": "Sample scenario",
  "Items": [
EOF

cat <<EOF > "$rawQueryScenarioFile"
{
  "Name": "Sample scenario",
  "Items": [
EOF

first=true

for databaseType in "${!connectionStrings[@]}"; do
  echo "$databaseType"

  connectionString="${connectionStrings[$databaseType]}"
  tableFile="${inputFiles[${databaseType}_TableFile]}"
  tableNameOverride="${inputFiles[${databaseType}_TableName]}"
  queryFile="${inputFiles[${databaseType}_QueryFile]}"
  rawQueryFile="${inputFiles[${databaseType}_RawQueryFile]}"
  rawQueryParametersFile="${inputFiles[${databaseType}_RawQueryParametersFile]}"
  dataSourceFile="${inputFiles[${databaseType}_DataSourceFile]}"
  tableName="${tableNameOverride:-GeneratedSample}"

  if [ "$first" = false ]; then
    echo "," >> "$queryScenarioFile"
  fi

  cat <<EOF >> "$queryScenarioFile"
    {
      "BenchmarkName": "$databaseType Sample Query",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "TableFilePath": "$tableFile",
      "QueryFilePath": "$queryFile",
      "TableName": "$tableName"
    }
EOF

  if [[ -n "$rawQueryFile" ]]; then
    if [ "$first" = false ]; then
      echo "," >> "$rawQueryScenarioFile"
    fi

    cat <<EOF >> "$rawQueryScenarioFile"
    {
      "BenchmarkName": "$databaseType Sample Raw Query",
      "DatabaseType": "$databaseType",
      "ConnectionString": "$connectionString",
      "QueryFilePath": "$rawQueryFile",
      "QueryParametersFilePath": "$rawQueryParametersFile",
      "TableName": "$tableName"
    }
EOF
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
    --TableName="$tableName" \
    --DropExisting=true
  if [[ $? -ne 0 ]]; then exit $?; fi

  "$toolPath" import \
    --DatabaseType="$databaseType" \
    --ConnectionString="$connectionString" \
    --TableFilePath="Definitions/$tableFile" \
    --TableName="$tableName" \
    --DataSourceType=Generator \
    --DataSourceFilePath="Definitions/$dataSourceFile" \
    --DataSourceMaxRows=1000
  if [[ $? -ne 0 ]]; then exit $?; fi

  echo ""

  first=false
done

echo "  ]" >> "$queryScenarioFile"
echo "}" >> "$queryScenarioFile"

echo "  ]" >> "$rawQueryScenarioFile"
echo "}" >> "$rawQueryScenarioFile"

"$toolPath" query-scenario \
  --QueryScenarioFilePath="$queryScenarioFile" \
  --QueryScenarioParametersFilePath="Definitions/SampleQueryScenarioParameters.json"

"$toolPath" raw-query-scenario \
  --QueryScenarioFilePath="$rawQueryScenarioFile" \
  --QueryScenarioParametersFilePath="Definitions/SampleQueryScenarioParameters.json"
