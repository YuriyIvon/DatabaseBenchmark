#!/bin/bash

. bop_variables.sh

echo "MongoDB Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=MongoDb --ConnectionString=$MONGODB_CONNECTION_STRING --TableFilePath=$TABLE_FILE --TableName=$MONGODB_COMPOSITEIDX_TABLE --DropExisting=true

echo "MongoDB Separate Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=MongoDb --ConnectionString=$MONGODB_CONNECTION_STRING --TableFilePath=$TABLE_FILE --TableName=$MONGODB_SEPARATEIDX_TABLE --DropExisting=true

echo "PostgreSQL Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=Postgres --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_COMPOSITEIDX_TABLE --DropExisting=true

echo "PostgreSQL Separate Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=Postgres --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_SEPARATEIDX_TABLE --DropExisting=true

echo "PostgreSQL JSONB Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=PostgresJsonb --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_JSONB_COMPOSITEIDX_TABLE --DropExisting=true --PostgresJsonb.CreateGinIndex=false

echo "PostgreSQL JSONB GIN"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark create --DatabaseType=PostgresJsonb --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_JSONB_GIN_TABLE --DropExisting=true
