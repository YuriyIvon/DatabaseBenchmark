﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataImporter
    {
        private readonly IExecutionEnvironment _environment;
        private readonly IProgressReporter _progressReporter;
        private readonly SqlQueryParametersBuilder _parametersBuilder;
        private readonly ISqlParameterAdapter _parameterAdapter;
        private readonly int _batchSize;

        public SqlDataImporter(IExecutionEnvironment environment, 
            IProgressReporter progressReporter,
            int batchSize,
            SqlQueryParametersBuilder parametersBuilder = null,
            ISqlParameterAdapter parameterAdapter = null)
        {
            _environment = environment;
            _progressReporter = progressReporter;
            _parametersBuilder = parametersBuilder;
            _parameterAdapter = parameterAdapter;
            _batchSize = batchSize;
        }

        public void Import(IDataSource source, Table table, IDbConnection connection, IDbTransaction transaction)
        {
            int imported;
            do
            {
                imported = ImportBatch(source, table, connection, transaction);

                _progressReporter.Increment(imported);
            }
            while (imported == _batchSize);
        }

        private int ImportBatch(IDataSource source, Table table, IDbConnection connection, IDbTransaction transaction)
        {
            if (_parametersBuilder != null)
            {
                _parametersBuilder.Reset();
            }

            var columns = table.Columns.Where(c => !c.DatabaseGenerated).ToArray();
            var columnNames = columns.Select(c => c.Name).ToArray();

            int i = 0;
            var rows = new List<List<string>>();

            while (i < _batchSize && source.Read())
            {
                var values = new List<string>();

                foreach (var column in columns)
                {
                    var value = source.GetValue(column.GetNativeType(), column.Name);

                    if (value is double doubleValue && double.IsNaN(doubleValue))
                    {
                        value = null;
                    }

                    var valueRepresentation = _parametersBuilder != null ? _parametersBuilder.Append(value, column.Type) : FormatValue(value);
                    values.Add(valueRepresentation);
                }

                rows.Add(values);

                i++;
            }

            if (rows.Any())
            {
                var command = connection.CreateCommand();
                command.CommandText = BuildCommandText(table.Name, columnNames, rows);

                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                if (_parametersBuilder != null)
                {
                    foreach (var parameter in _parametersBuilder.Parameters)
                    {
                        var dbParameter = command.CreateParameter();
                        _parameterAdapter.Populate(parameter, dbParameter);
                        command.Parameters.Add(dbParameter);
                    }
                }

                _environment.TraceCommand(command);

                command.ExecuteNonQuery();
            }

            return i;
        }

        protected virtual string BuildCommandText(
            string tableName,
            IEnumerable<string> columns,
            IEnumerable<IEnumerable<string>> rows)
        {
            var commandText = new StringBuilder("INSERT INTO ");
            commandText.Append(tableName);
            commandText.Append(" (");
            commandText.Append(string.Join(", ", columns));
            commandText.AppendLine(") VALUES ");

            var rowsText = string.Join(",\n",
                rows.Select(values => $"({string.Join(", ", values)})"));

            commandText.AppendLine(rowsText);

            return commandText.ToString();
        }

        //TODO: improve constant formatting and make it pluggable
        private static string FormatValue(object value)
        {
            if (value != null)
            {
                var stringValue = value is DateTime dateTimeValue ? dateTimeValue.ToString("o") : value.ToString();

                return (value is bool || value is int || value is long || value is double)
                    ? stringValue : $"'{stringValue.Replace("'", "''")}'";
            }

            return "NULL";
        }
    }
}
