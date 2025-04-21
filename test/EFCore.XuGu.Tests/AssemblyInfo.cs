// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

//
// Optional: Control the test execution order.
//           This can be helpful for diffing etc.
//

#if FIXED_TEST_ORDER

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer("Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGTestCollectionOrderer", "Microsoft.EntityFrameworkCore.XuGu.FunctionalTests")]
[assembly: TestCaseOrderer("Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGTestCaseOrderer", "Microsoft.EntityFrameworkCore.XuGu.FunctionalTests")]

#endif

// Our custom XGXunitTestFrameworkDiscoverer class allows filtering whole classes like SupportedServerVersionConditionAttribute, instead
// of just the test cases. This is necessary, if a fixture is database server version dependent.
[assembly: TestFramework("Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGXunitTestFramework", "Microsoft.EntityFrameworkCore.XuGu.FunctionalTests")]
