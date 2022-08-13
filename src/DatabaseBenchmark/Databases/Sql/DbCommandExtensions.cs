using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public static class DbCommandExtensions
    {
        public static T[] ReadAsArray<T>(this IDbCommand command)
        {
            using var reader = command.ExecuteReader();

            List<T> values = new();
            while (reader.Read())
            {
                values.Add((T)reader.GetValue(0));
            }

            return values.ToArray();
        }
    }
}
