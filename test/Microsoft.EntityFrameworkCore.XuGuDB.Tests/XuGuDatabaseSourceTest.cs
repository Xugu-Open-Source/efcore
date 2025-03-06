// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDatabaseSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(XuGuDbConnection).GetTypeInfo().Assembly.GetName().Name,
                new XuGuDatabaseProviderServices(XuGuDbTestHelpers.Instance.CreateServiceProvider()).InvariantName);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie");

            Assert.True(new DatabaseProvider<XuGuDatabaseProviderServices, XuGuDbOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new DatabaseProvider<XuGuDatabaseProviderServices, XuGuDbOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }
    }
}
