// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public static class XGDbContextOptionsBuilderExtensions
    {
        public static XGDbContextOptionsBuilder ApplyConfiguration(this XGDbContextOptionsBuilder optionsBuilder)
        {
            var maxBatch = TestEnvironment.GetInt(nameof(XGDbContextOptionsBuilder.MaxBatchSize));
            if (maxBatch.HasValue)
            {
                optionsBuilder.MaxBatchSize(maxBatch.Value);
            }

            optionsBuilder.ExecutionStrategy(d => new TestXGRetryingExecutionStrategy(d));

            optionsBuilder.CommandTimeout(XGTestStore.DefaultCommandTimeout);

            return optionsBuilder;
        }
    }
}
