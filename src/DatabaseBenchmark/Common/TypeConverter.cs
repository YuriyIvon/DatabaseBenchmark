using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DatabaseBenchmark.Common
{
    public static class TypeConverter
    {
        // Cache the compiled delegates so "pay" for compilation once per type-pair
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _converterCache = new();

        public static object ChangeType(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            Type sourceType = value.GetType();

            if (targetType.IsAssignableFrom(sourceType))
            {
                return value;
            }

            if (targetType.IsArray && sourceType.IsArray)
            {
                var converter = _converterCache.GetOrAdd((sourceType, targetType), _ => CompileArrayConverter(sourceType, targetType));
                return converter.DynamicInvoke(value);
            }

            return Convert.ChangeType(value, targetType);
        }

        private static Delegate CompileArrayConverter(Type sourceType, Type targetType)
        {
            var sourceElement = sourceType.GetElementType();
            var targetElement = targetType.GetElementType();

            var inputParam = Expression.Parameter(sourceType, "input");
            var lengthVar = Expression.Property(inputParam, "Length");
            var indexVar = Expression.Variable(typeof(int), "i");
            var resultVar = Expression.Variable(targetType, "result");

            // Loop Label for jumping
            var breakLabel = Expression.Label("LoopBreak");

            // The logic: result[i] = (TargetElement)input[i]
            var loopBody = Expression.Block(
                [indexVar, resultVar],
                Expression.Assign(resultVar, Expression.NewArrayBounds(targetElement, lengthVar)),
                Expression.Assign(indexVar, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(indexVar, lengthVar),
                        Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(resultVar, indexVar),
                                Expression.Convert(Expression.ArrayAccess(inputParam, indexVar), targetElement)
                            ),
                            Expression.PostIncrementAssign(indexVar)
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                resultVar
            );

            return Expression.Lambda(loopBody, inputParam).Compile();
        }
    }
}
