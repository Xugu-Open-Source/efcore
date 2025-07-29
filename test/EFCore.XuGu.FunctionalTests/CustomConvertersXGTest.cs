using System;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class CustomConvertersXGTest : CustomConvertersTestBase<CustomConvertersXGTest.CustomConvertersXGFixture>
    {
        public CustomConvertersXGTest(CustomConvertersXGFixture fixture)
            : base(fixture)
        {
        }

        // Blocked by EF #11929
        public override void Can_query_using_any_data_type_nullable_shadow()
        {
        }

        public override void Can_perform_query_with_ansi_strings_test()
        {
        }

        public class CustomConvertersXGFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => true;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
