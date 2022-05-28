using DatabaseBenchmark.Core.Interfaces;
using System.Data;
using System.Text;

namespace DatabaseBenchmark.Databases.Sql
{
    public static class ExecutionEnvironmentExtensions
    {
        public static void TraceCommand(this IExecutionEnvironment environment, IDbCommand command)
        {
            if (environment.TraceQueries)
            {
                var traceBuilder = new StringBuilder();

                traceBuilder.AppendLine("Query:");
                traceBuilder.AppendLine(command.CommandText);

                if (command.Parameters.Count > 0)
                {
                    traceBuilder.AppendLine("Parameters:");

                    //To make this common method compatible with ClickHouse driver
                    if (command.Parameters is IEnumerable<IDataParameter> collection)
                    {
                        foreach (var parameter in collection)
                        {
                            traceBuilder.AppendLine($"{parameter.ParameterName}={parameter.Value}");
                        }
                    }
                    else
                    {
                        foreach (IDataParameter parameter in command.Parameters)
                        {
                            traceBuilder.AppendLine($"{parameter.ParameterName}={parameter.Value}");
                        }
                    }
                }

                environment.WriteLine(traceBuilder.ToString());
            }
        }
    }
}
