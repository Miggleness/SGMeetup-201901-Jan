using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace SgMeetup
{
    public static class ObjectFactoryBuilder
    {
        public static Expression<Func<IReadOnlyDictionary<string,string>, object>> Create<T>()
            where T: class
        {
            // Gets get the type we'll generate a factory for
            var ofType = typeof(T);

            // get all properties of type
            var propInfos = from prop in ofType.GetProperties()
                        select prop;

            // This is our factory's input parameter (dictionary of string)
            var dictionaryParam = Expression.Parameter(
                                        type: typeof(IReadOnlyDictionary<string, string>), 
                                        name: "props");

            // This is where we store all expressions into
            var expressionBodies = new List<Expression>();


            // We have our output variable here and we assign it with its class' constructor
            var returnVariable = Expression.Variable(type: ofType, name: "result");

            var constructor = Expression.New(type: ofType);

            expressionBodies.Add(
                Expression.Assign(
                    left: returnVariable, 
                    right: constructor)
            );


            // Let's go thru each property
            foreach(var propInfo in propInfos)
            {
                var propNameConst = Expression.Constant(value: propInfo.Name);

                var returnVariableMember = Expression.Property(
                                expression: returnVariable, 
                                property: propInfo);

                var containsKeyTest = DictionaryContainsKeyExpression(
                                        dictionaryExpr: dictionaryParam, 
                                        keyExpr: propNameConst);

                switch(propInfo.PropertyType)
                {
                    case Type t when t == typeof(int):
                        expressionBodies.Add(
                            Expression.IfThen(
                                test: containsKeyTest,
                                ifTrue: Expression.Assign(
                                    left: returnVariableMember,
                                    right: GetValueFromDictionaryAsInt(dictionaryParam, propNameConst)
                            )));
                        break;
                    case Type t when t == typeof(DateTime):
                        expressionBodies.Add(
                            Expression.IfThen(test: containsKeyTest,
                                ifTrue: Expression.Assign(
                                        left: returnVariableMember,
                                        right: GetValueFromDictionaryAsDateTime(dictionaryParam, propNameConst)
                            )));
                        break;
                }
            }

            // IMPORTANT! We add our variable at the end of the expression body to indicate 
            // it will be our return value
            expressionBodies.Add(returnVariable);

            // Pour all expressions into an expression block
            var body = Expression.Block(new[] { returnVariable }, expressionBodies);

            // Create lambda expression 
            return Expression.Lambda<Func<IReadOnlyDictionary<string, string>, object>>(body, dictionaryParam);

        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <returns>The contains key expression.</returns>
        /// <param name="dictionaryExpr">Dictionary expr.</param>
        /// <param name="keyExpr">Key expr.</param>
        public static Expression DictionaryContainsKeyExpression(Expression dictionaryExpr, Expression keyExpr)
        {
            return Expression.Call(dictionaryExpr, "ContainsKey", null, keyExpr);
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <returns>The value from dictionary as int.</returns>
        /// <param name="dictionaryExpr">Dictionary expr.</param>
        /// <param name="keyExpr">Key expr.</param>
        public static Expression GetValueFromDictionaryAsInt(Expression dictionaryExpr, Expression keyExpr)
        {
            return GetValueFromDictionaryAs<int>(dictionaryExpr, keyExpr);
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <returns>The value from dictionary as date time.</returns>
        /// <param name="dictionaryExpr">Dictionary expr.</param>
        /// <param name="keyExpr">Key expr.</param>
        public static Expression GetValueFromDictionaryAsDateTime(Expression dictionaryExpr, Expression keyExpr)
        {
            // create variables
            var resultVar = Expression.Variable(typeof(DateTime), "resultDateTime");
            var dictionaryLookupResult = Expression.Variable(typeof(string), "dictionaryLookupResult");

            // assign result from dictionary lookup result
            var assignDictionaryLookupExpr = Expression.Assign(dictionaryLookupResult, Expression.Property(dictionaryExpr, "Item", keyExpr));
            // assign result of DateTime.Parse(string, IFormatProvider,  against our value
            var assignParseResultExpr = Expression.Assign(
                left: resultVar,
                right: Expression.Call(
                    method: typeof(DateTime).GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider), typeof(DateTimeStyles) }),
                    arguments: new Expression[] { dictionaryLookupResult, Expression.Constant(DateTimeFormatInfo.InvariantInfo), Expression.Constant(DateTimeStyles.None) }));

            return Expression.Block(new[] { resultVar, dictionaryLookupResult },
                assignDictionaryLookupExpr,
                assignParseResultExpr,
                resultVar);
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <returns>The value from dictionary as.</returns>
        /// <param name="dictionaryExpr">Dictionary expr.</param>
        /// <param name="keyExpr">Key expr.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private static Expression GetValueFromDictionaryAs<T>(Expression dictionaryExpr, Expression keyExpr)
        {
            var type = typeof(T);

            // create variables
            var resultVar = Expression.Variable(type, "parseResult");
            var dictionaryLookupResult = Expression.Variable(typeof(string), "dictionaryLookupResult");

            // assign result from dictionary lookup result
            var assignDictionaryLookupExpr = Expression.Assign(dictionaryLookupResult, Expression.Property(dictionaryExpr, "Item", keyExpr));

            // assign result of double.Parse against our value
            var assignParseResultExpr = Expression.Assign(
                left: resultVar,
                right: Expression.Call(
                    method: type.GetMethod("Parse", new[] { typeof(string) }),
                    arguments: new[] { dictionaryLookupResult }));

            return Expression.Block(new[] { resultVar, dictionaryLookupResult },
                assignDictionaryLookupExpr,
                assignParseResultExpr,
                resultVar);
        }
    }
}
