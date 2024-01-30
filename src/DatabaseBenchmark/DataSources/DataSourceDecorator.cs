using DatabaseBenchmark.Common;
using DatabaseBenchmark.DataSources.Decorators;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Globalization;

namespace DatabaseBenchmark.DataSources
{
    public class DataSourceDecorator
    {
        public IDataSource DataSource { get; }

        public DataSourceDecorator(IDataSource dataSource) => DataSource = dataSource;

        public DataSourceDecorator MaxRows(int maxRows) =>
            maxRows > 0
                ? new DataSourceDecorator(new DataSourceMaxRowsDecorator(DataSource, maxRows))
                : this;

        public DataSourceDecorator Mapping(string mappingFilePath) =>
            string.IsNullOrEmpty(mappingFilePath)
                ? this
                : new DataSourceDecorator(new DataSourceMappingDecorator(DataSource, JsonUtils.DeserializeFile<ColumnMappingCollection>(mappingFilePath)));

        public DataSourceDecorator TypedColumns(IEnumerable<Column> columns, string culture)
        {
            var formatter = string.IsNullOrEmpty(culture) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(culture);
            return new DataSourceDecorator(new DataSourceTypedColumnsDecorator(DataSource, columns, formatter));
        }
    }
}
