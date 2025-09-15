// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class TPTTableSplittingXGTest : TPTTableSplittingTestBase
    {
        public TPTTableSplittingXGTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        public override Task Can_insert_dependent_with_just_one_parent()
        {
            // This scenario is not valid for TPT
            return Task.CompletedTask;
        }

        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;
    }
}
