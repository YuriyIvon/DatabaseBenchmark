using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseBenchmark.Tests.Utils
{
    internal class SqlQueryParameterEqualityComparer : IEqualityComparer<SqlQueryParameter>
    {
        public bool Equals(SqlQueryParameter x, SqlQueryParameter y) => 
            x.Prefix == y.Prefix
                && x.Name == y.Name
                && x.Type == y.Type
                && StructuralComparisons.StructuralEqualityComparer.Equals(x.Value, y.Value)
                && x.Array == y.Array;

        public int GetHashCode([DisallowNull] SqlQueryParameter obj) => obj.GetHashCode();
    }
}
