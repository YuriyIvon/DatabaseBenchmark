using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public static class DbConnectionExtensions
    {
        public static void DropTableIfExists(this IDbConnection connection, string tableName)
        {
            try
            {
                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = $"DROP TABLE {tableName}";
                dropCommand.ExecuteNonQuery();
            }
            catch
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
            }
        }
    }
}
