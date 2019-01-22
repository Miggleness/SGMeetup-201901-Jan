using System;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using AgileObjects.ReadableExpressions;
using System.Collections.Generic;
using System.Globalization;

namespace SgMeetup
{
    public class Demo2_LambdaFactory
    {
        private readonly ITestOutputHelper _output;

        public Demo2_LambdaFactory(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Should_be_same()
        {
            // GIVEN
            var props = new Dictionary<string, string>
            {
                {"EmployeeId", "1"},
                {"LeaveStart", "2019-01-23"},
                {"LeaveEnd", "2019-01-24"}
            };

            var expected = ALotOfTyping.ObjectFactoryMethod(props);

            // WHEN
            var lambdaExpr = ObjectFactoryBuilder.Create<LeaveApprovalData>();
            _output.WriteLine(lambdaExpr.ToReadableString());
            var func = lambdaExpr.Compile();
            var actual = func(props) as LeaveApprovalData;


            // THEN
            actual.ShouldSatisfyAllConditions(
                () => actual.ShouldNotBeNull(),
                () => actual.EmployeeId.ShouldBe(expected.EmployeeId),
                () => actual.LeaveStart.ShouldBe(expected.LeaveStart),
                () => actual.LeaveEnd.ShouldBe(expected.LeaveEnd));
        }

    }


    public class LeaveApprovalData
    {
        public int EmployeeId { get; set; }
        public DateTime LeaveStart { get; set; }
        public DateTime LeaveEnd { get; set; }
    }

    public class ALotOfTyping
    {
        public static LeaveApprovalData ObjectFactoryMethod(
            Dictionary<string, string> props)
        {
            var data = new LeaveApprovalData();

            if(props.ContainsKey("EmployeeId"))
            {
                data.EmployeeId = int.Parse(props["EmployeeId"]);
            }
            if (props.ContainsKey("LeaveStart"))
            {
                data.LeaveStart = DateTime.Parse(props["LeaveStart"],
                            DateTimeFormatInfo.InvariantInfo,
                            DateTimeStyles.None);
            }
            if (props.ContainsKey("LeaveEnd"))
            {
                data.LeaveEnd = DateTime.Parse(props["LeaveEnd"],
                            DateTimeFormatInfo.InvariantInfo,
                            DateTimeStyles.None);
            }

            return data;
        }
    }

}
