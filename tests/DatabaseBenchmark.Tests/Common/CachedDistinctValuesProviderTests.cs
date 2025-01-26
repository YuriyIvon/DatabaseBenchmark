using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class CachedDistinctValuesProviderTests
    {
        [Fact]
        public void GetDistinctValuesCached()
        {
            var mockDistinctValuesProvider = Substitute.For<IDistinctValuesProvider>();
            mockDistinctValuesProvider.GetDistinctValues(default, default, false).ReturnsForAnyArgs(Array.Empty<object>());
            var cache = new MemoryCache();
            var distinctValuesProvider = new CachedDistinctValuesProvider(mockDistinctValuesProvider, cache);

            Parallel.For(0, 20, _ => distinctValuesProvider.GetDistinctValues("table", new Column { Name = "column" }, false));

            mockDistinctValuesProvider.ReceivedWithAnyArgs(1).GetDistinctValues(default, default, default);
        }
    }
}
