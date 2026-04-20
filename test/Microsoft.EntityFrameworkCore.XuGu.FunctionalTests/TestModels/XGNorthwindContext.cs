// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests.TestModels
{
    public class XGNorthwindContext : NorthwindContext
    {
        public static readonly string DatabaseName = StoreName;
        public static readonly string ConnectionString = XGTestStore.CreateConnectionString(DatabaseName);

        public XGNorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        public static XGTestStore GetSharedStore()
        {
            return XGTestStore.GetOrCreateShared(
                DatabaseName,
                () => XGTestStore.CreateDatabase(DatabaseName, scriptPath: @"Northwind.sql"));
        }
    }
}
