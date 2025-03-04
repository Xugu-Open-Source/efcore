using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Storage;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public partial class NorthwindMiscellaneousQueryXGTest
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        [SupportedServerVersionLessThanCondition(nameof(ServerVersionSupport.WindowFunctions))]
        public virtual Task RowNumberOverPartitionBy_not_supported_throws(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_Joined_Take(async));
        }
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task DateTime_parse_is_parameterized_when_from_closure(bool async)
        {
            var date = "1/1/1998 12:00:00 PM";

            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderDate > DateTime.Parse(date)),
                entryCount: 267);
        }
    }
}
