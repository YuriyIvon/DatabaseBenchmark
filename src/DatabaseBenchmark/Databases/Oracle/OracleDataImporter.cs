using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases.Oracle
{
    public class OracleDataImporter : SqlDataImporter
    {
        private static readonly string Spacing = new(' ', 4);

        public OracleDataImporter(IExecutionEnvironment environment,
            IProgressReporter progressReporter,
            SqlParametersBuilder parametersBuilder,
            int batchSize) : base(environment, progressReporter, parametersBuilder, batchSize)
        {
        }

        protected override string BuildCommandText(
            string tableName, 
            IEnumerable<string> columns, 
            IEnumerable<IEnumerable<string>> rows)
        {
            var commandText = new StringBuilder("INSERT ALL");

            foreach (var values in rows)
            {
                var columnsText = string.Join(", ", columns);
                var valuesText = string.Join(", ", values);
                commandText.AppendLine($"{Spacing}INTO {tableName}({columnsText}) VALUES ({valuesText})");
            }

            commandText.AppendLine("SELECT 1 FROM DUAL");

            return commandText.ToString();
        }
    }
}
