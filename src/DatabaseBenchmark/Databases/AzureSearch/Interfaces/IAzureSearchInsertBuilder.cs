using System.Collections.Generic;

namespace DatabaseBenchmark.Databases.AzureSearch.Interfaces
{
    public interface IAzureSearchInsertBuilder
    {
        int BatchSize { get; }
        IEnumerable<object> Build();
    }
}