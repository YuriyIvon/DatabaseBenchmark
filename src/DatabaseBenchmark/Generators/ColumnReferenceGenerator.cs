using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;

namespace DatabaseBenchmark.Generators
{
    public class ColumnReferenceGenerator : IGenerator
    {
        private readonly ColumnReferenceGeneratorOptions _options;
        private readonly IGeneratedValuesContext _context;

        public object Current { get; private set; }

        public bool IsBounded => false;

        public ColumnReferenceGenerator(ColumnReferenceGeneratorOptions options, IGeneratedValuesContext context)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool Next()
        {
            Current = _context.GetValue(_options.ColumnName);
            return true;
        }
    }
}
