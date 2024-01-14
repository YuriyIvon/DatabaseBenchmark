using Bogus;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using GeneratorKind = DatabaseBenchmark.Generators.Options.TextGeneratorOptions.GeneratorKind;

namespace DatabaseBenchmark.Generators
{
    public class TextGenerator : IGenerator
    {
        private readonly Faker _faker;
        private readonly TextGeneratorOptions _options;

        public object Current { get; private set; }

        public TextGenerator(Faker faker, TextGeneratorOptions options)
        {
            _faker = faker;
            _options = options;
        }

        public bool Next()
        {
            Current = _options.Kind switch
            {
                GeneratorKind.Word => _faker.Lorem.Word(),
                GeneratorKind.Sentence => _faker.Lorem.Sentence(),
                GeneratorKind.Paragraph => _faker.Lorem.Paragraph(),
                GeneratorKind.Text => _faker.Lorem.Text(),
                _ => throw new InputArgumentException($"Unknown text generator kind \"{_options.Kind}\"")
            };

            return true;
        }
    }
}
