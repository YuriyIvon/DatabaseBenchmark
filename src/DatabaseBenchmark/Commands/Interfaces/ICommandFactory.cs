namespace DatabaseBenchmark.Commands.Interfaces
{
    internal interface ICommandFactory
    {
        ICommand Create(string commandName);
    }
}
