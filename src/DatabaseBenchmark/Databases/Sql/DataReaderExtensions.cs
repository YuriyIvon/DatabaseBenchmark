using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public static class DataReaderExtensions
    {
        public static DataTable ToDataTable(this IDataReader reader)
        {
            var table = new DataTable();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                table.Columns.Add(new DataColumn(reader.GetName(i)));
            }

            while (reader.Read())
            {
                DataRow row = table.NewRow();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
