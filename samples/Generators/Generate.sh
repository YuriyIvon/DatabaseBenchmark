#!/bin/bash

databaseType="Postgres"
connectionString="Host=localhost;Port=5432;Database=benchmark;Username=postgres;Password=password"
userCount=100
postCount=1000

toolPath="../../src/DatabaseBenchmark/bin/Release/net8.0/linux-x64/DatabaseBenchmark"

$toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=UsersTable.json --DropExisting=true
if [ $? -ne 0 ]; then
  exit $?
fi

$toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=UsersTable.json --DataSourceType=Generator --DataSourceFilePath=UsersDataSource.json --DataSourceMaxRows=$userCount
if [ $? -ne 0 ]; then
  exit $?
fi

$toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=PostsTable.json --DropExisting=true
if [ $? -ne 0 ]; then
  exit $?
fi

$toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=PostsTable.json --DataSourceType=Generator --DataSourceFilePath=PostsDataSource.json --DataSourceMaxRows=$postCount
if [ $? -ne 0 ]; then
  exit $?
fi

$toolPath create --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=EnrichedUsersTable.json --DropExisting=true
if [ $? -ne 0 ]; then
  exit $?
fi

$toolPath import --DatabaseType=$databaseType --ConnectionString="$connectionString" --TableFilePath=EnrichedUsersTable.json --DataSourceType=Generator --DataSourceFilePath=EnrichedUsersDataSource.json
if [ $? -ne 0 ]; then
  exit $?
fi
