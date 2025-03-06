// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_max_batch_size()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie", b => b.MaxBatchSize(123));

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }

        [Fact]
        public void Can_add_extension_with_command_timeout()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie", b => b.CommandTimeout(30));

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Equal(30, extension.CommandTimeout);
        }

        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie");

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Equal("Database=Crunchie", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseXuGuDb("Database=Whisper");

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Equal("Database=Whisper", extension.ConnectionString);
            Assert.Null(extension.Connection);
        }

        [Fact]
        public void Can_add_extension_with_connection()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var connection = new SqlConnection();

            optionsBuilder.UseXuGuDb(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_connection_using_generic_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            var connection = new SqlConnection();

            optionsBuilder.UseXuGuDb(connection);

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.Same(connection, extension.Connection);
            Assert.Null(extension.ConnectionString);
        }

        [Fact]
        public void Can_add_extension_with_legacy_paging()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

            optionsBuilder.UseXuGuDb("Database=Kilimanjaro", b => b.UseRowNumberForPaging());

            var extension = optionsBuilder.Options.Extensions.OfType<XuGuDbOptionsExtension>().Single();

            Assert.True(extension.RowNumberPaging.HasValue);
            Assert.True(extension.RowNumberPaging.Value);
        }
    }
}
