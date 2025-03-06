// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.TestModels
{
    public class XuGuDbNorthwindContext : NorthwindContext
    {
        public static readonly string DatabaseName = StoreName;
        public static readonly string ConnectionString = XuGuDbTestStore.CreateConnectionString(DatabaseName);

        public XuGuDbNorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        public static XuGuDbTestStore GetSharedStore()
        {
            return XuGuDbTestStore.GetOrCreateShared(
                DatabaseName,
                () => XuGuDbTestStore.CreateDatabase(DatabaseName, scriptPath: @"Northwind.sql"));
        }
    }
}
