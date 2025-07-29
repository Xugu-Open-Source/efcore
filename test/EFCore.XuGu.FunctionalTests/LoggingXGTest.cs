using System;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class LoggingXGTest : LoggingRelationalTestBase<XGDbContextOptionsBuilder, XGOptionsExtension>
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<XGDbContextOptionsBuilder, XGOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseXG("Database=DummyDatabase", relationalAction);

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.XuGu";
    }
}
