using DatabaseBenchmark.DataSources.Interfaces;
using System;
using System.Collections.Generic;

namespace DatabaseBenchmark.Tests.Utils
{
    public sealed class SampleDataSource : IDataSource
    {
        private static readonly List<Dictionary<string, object>> _rows = new()
        {
            new() 
            {
                ["Id"] = 1,
                ["Category"] = "Category",
                ["SubCategory"] = "SubCategory",
                ["Name"] = "One",
                ["CreatedAt"] = new DateTime(2022, 10, 10),
                ["Rating"] = 5,
                ["Price"] = 23.5,
                ["Count"] = 50
            },
            new()
            {
                ["Id"] = 2,
                ["Category"] = "Category",
                ["SubCategory"] = "SubCategory",
                ["Name"] = "Two",
                ["CreatedAt"] = new DateTime(2022, 10, 14),
                ["Rating"] = 4.2,
                ["Price"] = 57.1,
                ["Count"] = 230
            },
            new()
            {
                ["Id"] = 3,
                ["Category"] = "Category",
                ["SubCategory"] = "SubCategory",
                ["Name"] = "Three",
                ["CreatedAt"] = new DateTime(2022, 11, 3),
                ["Rating"] = 3.8,
                ["Price"] = 45.2,
                ["Count"] = 10
            }
        };

        private readonly IEnumerator<Dictionary<string, object>> _rowIterator = _rows.GetEnumerator();

        public object GetValue(string name) => _rowIterator.Current[name];

        public bool Read() => _rowIterator.MoveNext();

        public void Dispose() => _rowIterator.Dispose();
    }
}
