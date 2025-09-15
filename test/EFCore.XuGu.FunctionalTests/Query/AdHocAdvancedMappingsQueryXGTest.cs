// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class AdHocAdvancedMappingsQueryXGTest : AdHocAdvancedMappingsQueryRelationalTestBase
{
    [SkippableTheory]
    public override async Task Query_generates_correct_datetime2_parameter_definition(int? fractionalSeconds, string postfix)
    {
        Skip.If(fractionalSeconds > 6, "XuGu has a max. DateTime precision of 6.");

        await base.Query_generates_correct_datetime2_parameter_definition(fractionalSeconds, postfix);
    }

    [SkippableTheory]
    public override async Task Query_generates_correct_datetimeoffset_parameter_definition(int? fractionalSeconds, string postfix)
    {
        Skip.If(fractionalSeconds > 6, "XuGu has a max. DateTimeOffset precision of 6.");

        await base.Query_generates_correct_datetimeoffset_parameter_definition(fractionalSeconds, postfix);
    }

    [SkippableTheory]
    public override async Task Query_generates_correct_timespan_parameter_definition(int? fractionalSeconds, string postfix)
    {
        Skip.If(fractionalSeconds > 6, "XuGu has a max. TimeSpan precision of 6.");

        await base.Query_generates_correct_timespan_parameter_definition(fractionalSeconds, postfix);
    }

    protected override ITestStoreFactory TestStoreFactory
        => XGTestStoreFactory.Instance;
}
