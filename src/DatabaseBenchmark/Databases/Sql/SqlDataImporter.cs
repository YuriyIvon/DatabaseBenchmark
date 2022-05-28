using DatabaseBenchmark.Core.Interfaces;
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
        private readonly SqlParametersBuilder _parametersBuilder;
        private readonly bool _useQueryParameters;
        private readonly int _batchSize;

        public SqlDataImporter(IExecutionEnvironment environment, 
            IProgressReporter progressReporter,
            bool useQueryParameters,
            int batchSize)
        {
            _environment = environment;
            _progressReporter = progressReporter;
            _parametersBuilder = new SqlParametersBuilder();
            _useQueryParameters = useQueryParameters;
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
            _parametersBuilder.Reset();

            var columns = table.Columns.Where(c => !c.DatabaseGenerated).Select(c => c.Name).ToArray();

            var commandText = new StringBuilder("INSERT INTO ");
            commandText.Append(table.Name);
            commandText.Append(" (");
            commandText.Append(string.Join(", ", columns));
            commandText.AppendLine(") VALUES ");

            int i = 0;
            var sections = new List<string>();

            while (i < _batchSize && source.Read())
            {
                var sectionValues = new List<string>();

                foreach (var column in columns)
                {
                    var value = source.GetValue(column);

                    if (value is double doubleValue && double.IsNaN(doubleValue))
                    {
                        value = null;
                    }

                    var valueRepresentation = _useQueryParameters ? _parametersBuilder.Append(value) : FormatValue(value);
                    sectionValues.Add(valueRepresentation);
                }

                sections.Add($"({string.Join(", ", sectionValues)})");

                i++;
            }

            if (sections.Any())
            {
                commandText.AppendLine(string.Join(", ", sections));

                var command = connection.CreateCommand();
                command.CommandText = commandText.ToString();

                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                if (_useQueryParameters)
                {
                    foreach (var parameterValue in _parametersBuilder.Values)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = parameterValue.Key;
                        parameter.Value = parameterValue.Value ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }
                }

                _environment.TraceCommand(command);

                command.ExecuteNonQuery();
            }

            return i;
        }

        private static string FormatValue(object value)
        {
            if (value != null)
            {
                var stringValue = value.ToString();

                return (value is bool || value is int || value is long || value is double)
                    ? stringValue : $"'{stringValue.Replace("'", "''")}'";
            }

            return "NULL";
        }
    }
}
