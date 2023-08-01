#!/bin/bash

. bop_variables.sh

TRACE_QUERIES=false
TRACE_RESULTS=false

echo "Benchmarking Query 1 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters1_1.json --ReportFilePath=_query1_1.csv --ReportFormatterType=Csv

echo "Benchmarking Query 1 in 10 threads"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters1_10.json --ReportFilePath=_query1_10.csv --ReportFormatterType=Csv

echo "Benchmarking Query 2 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters2_1.json --ReportFilePath=_query2_1.csv --ReportFormatterType=Csv

echo "Benchmarking Query 2 in 10 threads"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters2_10.json --ReportFilePath=_query2_10.csv --ReportFormatterType=Csv

echo "Benchmarking Query 3 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters3_1_a.json --ReportFilePath=_query3_1_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,2,3,5,6

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters3_1_b.json --ReportFilePath=_query3_1_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=4

echo "Benchmarking Query 3 in 10 threads"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters3_10_a.json --ReportFilePath=_query3_10_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,2,3,5,6

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters3_10_b.json --ReportFilePath=_query3_10_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=4

echo "Benchmarking Query 4 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters4_1_a.json --ReportFilePath=_query4_1_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,3,5

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters4_1_b.json --ReportFilePath=_query4_1_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=2,4,6

echo "Benchmarking Query 4 in 10 threads"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters4_10_a.json --ReportFilePath=_query4_10_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,3,5

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters4_10_b.json --ReportFilePath=_query4_10_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=2,4,6

echo "Benchmarking Query 5 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters5_1_a.json --ReportFilePath=_query5_1_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,3,5,6

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters5_1_b.json --ReportFilePath=_query5_1_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=2,4

echo "Benchmarking Query 6 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark query-scenario --QueryScenarioFilePath=QueryScenario.json --QueryScenarioParametersFilePath=ScenarioParameters6_1.json --ReportFilePath=_query6_1.csv --ReportFormatterType=Csv

echo "Benchmarking Query 7 in 1 thread"

$BENCHMARK_TOOL_PATH/DatabaseBenchmark raw-query-scenario --QueryScenarioFilePath=QueryScenario7.json --QueryScenarioParametersFilePath=ScenarioParameters7_1_a.json --ReportFilePath=_query7_1_a.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=1,2,3,4

$BENCHMARK_TOOL_PATH/DatabaseBenchmark raw-query-scenario --QueryScenarioFilePath=QueryScenario7.json --QueryScenarioParametersFilePath=ScenarioParameters7_1_b.json --ReportFilePath=_query7_1_b.csv --ReportFormatterType=Csv --QueryScenarioStepIndexes=5,6