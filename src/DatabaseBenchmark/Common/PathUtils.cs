namespace DatabaseBenchmark.Common
{
    public static class PathUtils
    {
        public static string CombinePaths(string current, string target) =>
            Path.IsPathRooted(target)
                ? target
                : Path.Combine(current, target);
    }
}
