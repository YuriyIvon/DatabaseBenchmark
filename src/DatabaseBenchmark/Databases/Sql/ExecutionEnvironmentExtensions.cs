using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public static class ExecutionEnvironmentExtensions
    {
        public static void TraceCommand(this IExecutionEnvironment environment, string commandText)
        {
            if (environment.TraceQueries)
            {
                var traceBuilder = new StringBuilder();

                traceBuilder.AppendLine("Query:");
                traceBuilder.AppendLine(commandText);

                environment.WriteLine(traceBuilder.ToString());
            }
        }

        public static void TraceCommand(this IExecutionEnvironment environment, string commandText, IEnumerable<SqlQueryParameter> parameters)
        {
            environment.TraceCommand(commandText);

            if (environment.TraceQueries)
            {
                var traceBuilder = new StringBuilder();

                if (parameters.Count() > 0)
                {
                    traceBuilder.AppendLine("Parameters:");

                    foreach (var parameter in parameters)
                    {
                        PrintParameter(traceBuilder, parameter, environment.ValueFormatter);
                    }
                }

                environment.WriteLine(traceBuilder.ToString());
            }
        }

        private static void PrintParameter(StringBuilder traceBuilder, SqlQueryParameter parameter, IValueFormatter valueFormatter)
        {
            traceBuilder.Append($"{parameter.Prefix}{parameter.Name}={valueFormatter.Format(parameter.Value)}");

            if (parameter.Value != null)
            {
                traceBuilder.AppendLine($" ({parameter.Value.GetType()})");
            }
            else
            {
                traceBuilder.AppendLine();
            }
        }
    }
}
