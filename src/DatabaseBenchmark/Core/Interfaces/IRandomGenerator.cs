namespace DatabaseBenchmark.Core.Interfaces
{
    public interface IRandomGenerator
    {
        bool GetRandomBoolean();

        int GetRandomInteger(int minValue, int maxValue);

        double GetRandomDouble(double minValue, double maxValue);

        DateTime GetRandomDateTime(DateTime minValue, DateTime maxValue);

        string GetRandomString(int minLength, int maxLength, string allowedCharacters);
    }
}
