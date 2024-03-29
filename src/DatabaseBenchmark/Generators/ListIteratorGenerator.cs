﻿using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using System.Collections;

namespace DatabaseBenchmark.Generators
{
    public class ListIteratorGenerator : IGenerator
    {
        private readonly ListIteratorGeneratorOptions _options;
        private readonly IEnumerator _enumerator;

        public object Current => _enumerator.Current;

        public bool IsBounded => true;

        public ListIteratorGenerator(ListIteratorGeneratorOptions options)
        {
            _options = options;
            _enumerator = _options.Items.GetEnumerator();
        }

        public bool Next() => _enumerator.MoveNext();
    }
}
