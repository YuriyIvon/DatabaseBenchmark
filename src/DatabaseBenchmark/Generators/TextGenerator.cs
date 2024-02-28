using Bogus.DataSets;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.TextGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class TextGenerator : IGenerator
    {
        private readonly Lorem _textFaker;
        private readonly TextGeneratorOptions _options;

        public object Current { get; private set; }

        public TextGenerator(TextGeneratorOptions options)
        {
            _options = options;
            _textFaker = string.IsNullOrEmpty(options.Locale) ? new Lorem() : new Lorem(locale: options.Locale);
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.Word => _textFaker.Word(),
                GeneratorKind.Sentence => _textFaker.Sentence(),
                GeneratorKind.Paragraph => _textFaker.Paragraph(),
                GeneratorKind.Text => _textFaker.Text(),
                _ => throw new InputArgumentException($"Unknown text generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
