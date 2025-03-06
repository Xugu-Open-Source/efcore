// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static XuGuDbContextOptionsBuilder ApplyConfiguration(this XuGuDbContextOptionsBuilder optionsBuilder)
        {
            var maxBatch = TestEnvironment.GetInt(nameof(XuGuDbContextOptionsBuilder.MaxBatchSize));

            if (maxBatch.HasValue)
            {
                optionsBuilder.MaxBatchSize(maxBatch.Value);
            }

            var offsetSupport = TestEnvironment.GetFlag(nameof(XuGuDbCondition.SupportsOffset)) ?? true;

            if (!offsetSupport)
            {
                optionsBuilder.UseRowNumberForPaging();
            }

            return optionsBuilder;
        }
    }
}
