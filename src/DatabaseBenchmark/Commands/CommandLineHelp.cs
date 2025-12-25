using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Databases.ClickHouse;
using DatabaseBenchmark.Databases.CosmosDb;
using DatabaseBenchmark.Databases.MongoDb;
using DatabaseBenchmark.Databases.MySql;
using DatabaseBenchmark.Databases.PostgreSql;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Csv;
using DatabaseBenchmark.Reporting;
using System.Reflection;

namespace DatabaseBenchmark.Commands
{
    public static class CommandLineHelp
    {
        private static readonly CommandDescriptor[] _commands = new CommandDescriptor[]
        {
            new CommandDescriptor
            {
                Name = "create",
                OptionsContainerType = typeof(CreateCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(CreateCommandOptions.DatabaseType),
                        AllowedValuesProvider = new DatabaseFactory(null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "ClickHouse",
                                OptionsContainerType = typeof(ClickHouseTableOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "MySql",
                                OptionsContainerType = typeof(MySqlTableOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "PostgresJsonb",
                                OptionsContainerType = typeof(PostgreSqlJsonbTableOptions)
                            }
                        ]
                    }
                ]

            },
            new CommandDescriptor
            {
                Name = "import",
                OptionsContainerType = typeof(ImportCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(ImportCommandOptions.DatabaseType),
                        AllowedValuesProvider = new DatabaseFactory(null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "MongoDb",
                                OptionsContainerType = typeof(MongoDbInsertOptions)
                            }
                        ]
                    },
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(ImportCommandOptions.DataSourceType),
                        AllowedValuesProvider = new DataSourceFactory(null, null, null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "Csv",
                                OptionsContainerType = typeof(CsvDataSourceOptions)
                            }
                        ]
                    }
                ]
            },
            new CommandDescriptor
            {
                Name = "insert",
                OptionsContainerType = typeof(InsertCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(InsertCommandOptions.DatabaseType),
                        AllowedValuesProvider = new DatabaseFactory(null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "MongoDb",
                                OptionsContainerType = typeof(MongoDbInsertOptions)
                            }
                        ]
                    },
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(InsertCommandOptions.DataSourceType),
                        AllowedValuesProvider = new DataSourceFactory(null, null, null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "Csv",
                                OptionsContainerType = typeof(CsvDataSourceOptions)
                            }
                        ]
                    }
                ]
            },
            new CommandDescriptor
            {
                Name = "query",
                OptionsContainerType = typeof(QueryCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(QueryCommandOptions.DatabaseType),
                        AllowedValuesProvider = new DatabaseFactory(null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "CosmosDb",
                                OptionsContainerType = typeof(CosmosDbQueryOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "MongoDb",
                                OptionsContainerType = typeof(MongoDbQueryOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "Postgres",
                                OptionsContainerType = typeof(PostgreSqlQueryOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "PostgresJsonb",
                                OptionsContainerType = typeof(PostgreSqlJsonbQueryOptions)
                            }
                        ]
                    },
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(QueryCommandOptions.ReportFormatterType),
                        AllowedValuesProvider = new ReportFormatterFactory()
                    }
                ]
            },
            new CommandDescriptor
            {
                Name = "query-scenario",
                OptionsContainerType = typeof(QueryScenarioCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(RawQueryCommandOptions.ReportFormatterType),
                        AllowedValuesProvider = new ReportFormatterFactory()
                    }
                ]
            },
            new CommandDescriptor
            {
                Name = "raw-query",
                OptionsContainerType = typeof(RawQueryCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(RawQueryCommandOptions.DatabaseType),
                        AllowedValuesProvider = new DatabaseFactory(null, null),
                        ValueSpecificOptions =
                        [
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "CosmosDb",
                                OptionsContainerType = typeof(CosmosDbQueryOptions)
                            },
                            new ValueSpecificOptionsDescriptor
                            {
                                Value = "MongoDb",
                                OptionsContainerType = typeof(MongoDbQueryOptions)
                            }
                        ]
                    },
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(RawQueryCommandOptions.ReportFormatterType),
                        AllowedValuesProvider = new ReportFormatterFactory()
                    }
                ]
            },
            new CommandDescriptor
            {
                Name = "raw-query-scenario",
                OptionsContainerType = typeof(QueryScenarioCommandOptions),
                RestrictedValueOptions =
                [
                    new RestrictedValueOptionDescriptor
                    {
                        Name = nameof(RawQueryCommandOptions.ReportFormatterType),
                        AllowedValuesProvider = new ReportFormatterFactory()
                    }
                ]
            }
        };

        public static void Print()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.Write(new string(' ', 8));
            Console.WriteLine("DatabaseBenchmark <command> <[--CommandParameter1=value] ...>");
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine();

            foreach (var command in _commands)
            {
                Console.Write(new string(' ', 4));
                Console.WriteLine(command.Name);
                Console.WriteLine();

                PrintOptions(command.OptionsContainerType, command.RestrictedValueOptions);

                var valueSpecificOptions = command.RestrictedValueOptions
                    .Where(o => o.ValueSpecificOptions != null && o.ValueSpecificOptions.Any())
                    .SelectMany(o => o.ValueSpecificOptions
                        .Select(vo => new
                        {
                            o.Name,
                            vo.Value,
                            vo.OptionsContainerType
                        }));

                foreach (var option in valueSpecificOptions)
                {
                    Console.Write(new string(' ', 8));
                    Console.WriteLine($"Options specific to {option.Name}={option.Value}");
                    Console.WriteLine();

                    PrintOptions(option.OptionsContainerType, null);
                }
            }

            Console.WriteLine();
        }

        private static void PrintOptions(Type optionsContainerType, RestrictedValueOptionDescriptor[] restrictedValueOptions)
        {
            var prefix = optionsContainerType.GetCustomAttribute<OptionPrefixAttribute>()?.Prefix;

            var commandOptions = optionsContainerType.GetInterfaces().Concat([optionsContainerType])
                    .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                    .Select(p => new OptionDescriptor(p, p.GetCustomAttribute<OptionAttribute>()))
                    .Where(p => p.PropertyAttribute != null)
                    .OrderBy(p => p.Property.Name);

            var optionsInstance = Activator.CreateInstance(optionsContainerType);

            foreach (var option in commandOptions)
            {
                var propertyName = FormatProperty(prefix, option.Property.Name);

                Console.Write(new string(' ', 8));
                Console.Write($"--{propertyName}=");
                PrintParameterType(restrictedValueOptions, option);
                Console.WriteLine();
                Console.Write(new string(' ', 12));
                Console.Write(option.PropertyAttribute.Description);
                Console.Write(".");

                if (option.PropertyAttribute.IsRequired)
                {
                    Console.Write(" Required.");
                }

                object defaultValue = option.Property.GetValue(optionsInstance);
                if (defaultValue != null)
                {
                    Console.Write($" Default: {defaultValue}.");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static void PrintParameterType(RestrictedValueOptionDescriptor[] restrictedValueOptions, OptionDescriptor option)
        {
            var factoryOptionProperty = restrictedValueOptions?.FirstOrDefault(p => p.Name == option.Property.Name);
            Type optionType = Nullable.GetUnderlyingType(option.Property.PropertyType) ?? option.Property.PropertyType;

            var typeStrings = new Dictionary<Type, string>
            {
                [typeof(string)] = "string",
                [typeof(double)] = "double",
                [typeof(int)] = "int",
                [typeof(string[])] = "string[]",
                [typeof(double[])] = "double[]",
                [typeof(int[])] = "int[]",
            };

            if (factoryOptionProperty != null)
            {
                Console.Write($"{{ {string.Join(" | ", factoryOptionProperty.AllowedValuesProvider.Options)} }}");
            }
            else if (optionType == typeof(bool))
            {
                Console.Write("{ True | False }");
            }
            else if (typeStrings.TryGetValue(optionType, out var str))
            {
                Console.Write($"<{str}>");
            }
            else
            {
                throw new ArgumentException($"Unsupported option type \"{optionType}\"");
            }
        }

        private static string FormatProperty(string prefix, string propertyName) =>
           prefix != null ? string.Join(".", prefix, propertyName) : propertyName;

        private class CommandDescriptor
        {
            public string Name { get; set; }

            public Type OptionsContainerType { get; set; }

            public RestrictedValueOptionDescriptor[] RestrictedValueOptions { get; set; }
        }

        private class RestrictedValueOptionDescriptor
        {
            public string Name { get; set; }

            public IAllowedValuesProvider AllowedValuesProvider { get; set; }

            public ValueSpecificOptionsDescriptor[] ValueSpecificOptions { get; set; }
        }

        private class ValueSpecificOptionsDescriptor
        {
            public string Value { get; set; }

            public Type OptionsContainerType { get; set; }
        }
    }
}
