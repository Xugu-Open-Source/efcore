// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
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
    }
}
