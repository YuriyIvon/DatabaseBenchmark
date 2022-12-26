using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbInsertBuilder : SqlInsertBuilder
    {
        public PostgreSqlJsonbInsertBuilder(Table table,
            IDataSource dataSource,
            ISqlParametersBuilder parametersBuilder)
            : base(table, dataSource, parametersBuilder)
        {
        }

        public override string Build()
        {
            throw new NotImplementedException();
        }
    }
}
