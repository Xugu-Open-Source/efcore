// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
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
