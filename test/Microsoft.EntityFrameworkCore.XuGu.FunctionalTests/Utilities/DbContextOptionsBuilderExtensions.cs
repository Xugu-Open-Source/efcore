// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static XGDbContextOptionsBuilder ApplyConfiguration(this XGDbContextOptionsBuilder optionsBuilder)
        {
            var maxBatch = TestEnvironment.GetInt(nameof(XGDbContextOptionsBuilder.MaxBatchSize));

            if (maxBatch.HasValue)
            {
                optionsBuilder.MaxBatchSize(maxBatch.Value);
            }

            var offsetSupport = TestEnvironment.GetFlag(nameof(XGCondition.SupportsOffset)) ?? true;

            if (!offsetSupport)
            {
                optionsBuilder.UseRowNumberForPaging();
            }

            return optionsBuilder;
        }
    }
}
