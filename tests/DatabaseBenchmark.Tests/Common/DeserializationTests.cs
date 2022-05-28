using DatabaseBenchmark.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace DatabaseBenchmark.Tests.Common
{
    public class DeserializationTests
    {
        [Fact]
        public void DeserializeObject()
        {
            var json = @"{
  ""BooleanProperty"": true,
  ""IntegerProperty"": 10,
  ""DoubleProperty"": 10.1,
  ""DateProperty"": ""2007-10-10"",
  ""DateTimeProperty"": ""2007-10-10T12:12:12Z"",
  ""StringProperty"": ""string"",
  ""ArrayProperty"": [""A"", ""B""]
}";
            var deserialized = JsonSerializer.Deserialize<DeserializedPayload>(json);

            Assert.Equal(typeof(bool), deserialized.BooleanProperty.GetType());
            Assert.Equal(typeof(long), deserialized.IntegerProperty.GetType());
            Assert.Equal(typeof(double), deserialized.DoubleProperty.GetType());
            Assert.Equal(typeof(DateTime), deserialized.DateProperty.GetType());
            Assert.Equal(typeof(DateTime), deserialized.DateTimeProperty.GetType());
            Assert.Equal(typeof(string), deserialized.StringProperty.GetType());
            Assert.Equal(typeof(string), deserialized.ArrayProperty[0].GetType());
        }

        [Fact]
        public void DeserializeCondition()
        {
            string json = @"{
  ""Condition"": {
    ""Operator"": ""And"",
    ""Conditions"": [
      {
        ""ColumnName"": ""Price"",
        ""Operator"": ""Greater"",
        ""Value"": 10
      },
      {
        ""ColumnName"": ""Price"",
        ""Operator"": ""Lower"",
        ""Value"": 100
      }
    ]
  }
}";
            var deserialized = JsonSerializer.Deserialize<DeserializedCondition>(json);

            Assert.Equal(typeof(QueryGroupCondition), deserialized.Condition.GetType());
            var conditions = ((QueryGroupCondition)deserialized.Condition).Conditions;
            Assert.NotNull(conditions);
            Assert.Equal(2, conditions.Length);
            Assert.Equal(typeof(QueryPrimitiveCondition), conditions[0].GetType());
            Assert.Equal(typeof(QueryPrimitiveCondition), conditions[1].GetType());

        }

        private class DeserializedPayload
        {
            [JsonConverter(typeof(JsonObjectConverter))]
            public object BooleanProperty { get; set; }

            [JsonConverter(typeof(JsonObjectConverter))]
            public object IntegerProperty { get; set; }

            [JsonConverter(typeof(JsonObjectConverter))]
            public object DoubleProperty { get; set; }

            [JsonConverter(typeof(JsonObjectConverter))]
            public object DateProperty { get; set; }

            [JsonConverter(typeof(JsonObjectConverter))]
            public object DateTimeProperty { get; set; }

            [JsonConverter(typeof(JsonObjectConverter))]
            public object StringProperty { get; set; }

            [JsonConverter(typeof(JsonObjectArrayConverter))]
            public object[] ArrayProperty { get; set; }
        }

        private class DeserializedCondition
        {
            [JsonConverter(typeof(JsonQueryConditionConverter))]
            public IQueryCondition Condition { get; set; }
        }
    }
}
