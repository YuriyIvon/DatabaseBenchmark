using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface ITargetOptions
    {
        [Option("Database type", true)]
        string DatabaseType { get; set; }

        [Option("Connection string", true)]
        string ConnectionString { get; set; }

        [Option("Target physical table name")]
        string TableName { get; set; }
    }
}
