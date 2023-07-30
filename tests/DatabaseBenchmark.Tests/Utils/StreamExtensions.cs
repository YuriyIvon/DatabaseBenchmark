using System.IO;

namespace DatabaseBenchmark.Tests.Utils
{
    public static class StreamExtensions
    {
        public static string ReadAsString(this Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
