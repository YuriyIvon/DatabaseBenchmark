﻿using System.Text.Json;

namespace DatabaseBenchmark.Model
{
    public class ValueRandomizationRule
    {
        public bool UseExistingValues { get; set; } = true;

        public JsonElement GeneratorOptions { get; set; }

        public int MinCollectionLength { get; set; } = 1;

        public int MaxCollectionLength { get; set; } = 10;
    }
}
