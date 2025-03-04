using Xunit;

//
// Optional: Control the test execution order.
//           This can be helpful for diffing etc.
//

#if FIXED_TEST_ORDER

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer("EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGTestCollectionOrderer", "EntityFrameworkCore.XuGu.FunctionalTests")]
[assembly: TestCaseOrderer("EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGTestCaseOrderer", "EntityFrameworkCore.XuGu.FunctionalTests")]

#endif

// Our custom XGXunitTestFrameworkDiscoverer class allows filtering whole classes like SupportedServerVersionConditionAttribute, instead
// of just the test cases. This is necessary, if a fixture is database server version dependent.
[assembly: TestFramework("EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit.XGXunitTestFramework", "EntityFrameworkCore.XuGu.FunctionalTests")]
