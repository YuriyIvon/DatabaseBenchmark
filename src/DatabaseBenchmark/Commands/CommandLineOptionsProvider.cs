using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands
{
    public class CommandLineOptionsProvider : OptionsProvider
    {
        private readonly string[] _args;

        public CommandLineOptionsProvider(string[] args)
        {
            _args = args;
        }

        protected override void ParseOptions()
        {
            Options = new Dictionary<string, string>();

            foreach (var a in _args)
            {
                if (!a.StartsWith("--"))
                {
                    throw new InputArgumentException($"Command line argument \"{a}\" must be prefixed with \"--\"");
                }

                var parts = a.Split('=', 2);
                Options.Add(parts[0].TrimStart('-'), parts[1]);
            }
        }
    }
}
