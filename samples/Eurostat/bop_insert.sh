#!/bin/bash

. bop_variables.sh

DATA_SOURCE_CULTURE="en-GB"
WARMUP_QUERY_COUNT=100
QUERY_COUNT=215000
QUERY_PARALLELISM=100
BATCH_SIZE=1
TRACE_QUERIES=false
TRACE_RESULTS=false
INPUT_FILE="bop_euins6_m_linear.csv"
OUTPUT_FILE=_insert.csv

echo "MongoDB Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="MongoDB Composite Index" --DatabaseType=MongoDb --ConnectionString=$MONGODB_CONNECTION_STRING --TableFilePath=$TABLE_FILE --TableName=$MONGODB_COMPOSITEIDX_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE

echo "MongoDB Separate Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="MongoDB Separate Index" --DatabaseType=MongoDb --ConnectionString=$MONGODB_CONNECTION_STRING --TableFilePath=$TABLE_FILE --TableName=$MONGODB_SEPARATEIDX_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE 

echo "PostgreSQL Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="PostgreSQL Composite Index" --DatabaseType=Postgres --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_COMPOSITEIDX_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE 

echo "PostgreSQL Separate Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="PostgreSQL Separate Index" --DatabaseType=Postgres --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_SEPARATEIDX_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE 

echo "PostgreSQL JSONB Composite Index"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="PostgreSQL JSONB Composite Index" --DatabaseType=PostgresJsonb --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_JSONB_COMPOSITEIDX_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE 

echo "PostgreSQL JSONB GIN"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark insert --BenchmarkName="PostgreSQL JSONB GIN" --DatabaseType=PostgresJsonb --ConnectionString="$POSTGRES_CONNECTION_STRING" --TableFilePath=$TABLE_FILE --TableName=$POSTGRES_JSONB_GIN_TABLE --DataSourceType=Csv --DataSource.Csv.Culture=$DATA_SOURCE_CULTURE --DataSourceFilePath=$INPUT_FILE --MappingFilePath=$MAPPING_FILE --WarmupQueryCount=$WARMUP_QUERY_COUNT --QueryCount=$QUERY_COUNT --QueryParallelism=$QUERY_PARALLELISM --BatchSize=$BATCH_SIZE --TraceQueries=$TRACE_QUERIES --ReportFormatterType=Csv >> $OUTPUT_FILE 
