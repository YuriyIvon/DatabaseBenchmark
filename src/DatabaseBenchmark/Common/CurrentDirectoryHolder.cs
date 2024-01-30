namespace DatabaseBenchmark.Common
{
    public sealed class CurrentDirectoryHolder : IDisposable
    {
        private readonly string _currentDirectory;

        public CurrentDirectoryHolder() => _currentDirectory = Directory.GetCurrentDirectory();

        public void Dispose() => Directory.SetCurrentDirectory(_currentDirectory);
    }
}
