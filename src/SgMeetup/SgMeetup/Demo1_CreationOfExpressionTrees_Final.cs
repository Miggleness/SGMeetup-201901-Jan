using System;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace SgMeetup
{
    // dotnet add package AgileObjects.ReadableExpressions
    // dotnet add package Shouldly

    public class Demo1
    {
        ITestOutputHelper _output;
        public Demo1(ITestOutputHelper output)
        {
            _output = output;

        }

        //[Fact]
        public void UsingLambdaExpression()
        {
            // 1. Create Lambda Expression
            Expression<Func<int, int>> expr = x => x * x;

            // 2. Compile into delegate
            var square = expr.Compile();

            // 3. Run delegate and validate
            square(2).ShouldBe(4);

            // Display code
            _output.WriteLine("----------- Code -------------");
            _output.WriteLine(expr.ToReadableString());
        }

        //[Fact]
        public void UsingExpressionApi()
        {
            // Create our parameter
            var param = Expression.Parameter(typeof(int), "x");

            // Create multiply expression
            var multiply = Expression.Multiply(param, param);

            // Create a lambda
            var lambda = Expression.Lambda<Func<int, int>>(multiply, param);

            // Compile into delegate and run
            var square = lambda.Compile();
            square(2).ShouldBe(4);

            // Display code
            _output.WriteLine("----------- Code -------------");
            _output.WriteLine(lambda.ToReadableString());
        }

    }
}
