using DatabaseBenchmark.Commands;
using DatabaseBenchmark.Common;

if (args.Length > 0)
{
    Environment.ExitCode = -1;

    try
    {
        var commandName = args.First();
        var optionsProvider = new CommandLineOptionsProvider(args.Skip(1).ToArray());
        var commandFactory = new CommandFactory(optionsProvider);
        var command = commandFactory.Create(commandName);

        command.Execute();

        Environment.ExitCode = 0;
    }
    catch (InputArgumentException ex)
    {
        Console.Error.WriteLine("Error:");
        Console.Error.WriteLine(ex.Message);
    }
    catch (AggregateException ex)
    {
        Console.Error.WriteLine("Error:");
        foreach (var iex in ex.InnerExceptions)
        {
            if (iex is InputArgumentException iaex)
            {
                Console.Error.WriteLine(iaex.Message);
            }
            else
            {
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
    }
}
else
{
    CommandLineHelp.Print();
}
