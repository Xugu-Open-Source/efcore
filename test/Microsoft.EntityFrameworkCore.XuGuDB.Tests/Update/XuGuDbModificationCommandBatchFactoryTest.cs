// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Update
{
    public class XuGuDbModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Uses_MaxBatchSize_specified_in_XuGuDbOptionsExtension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie", b => b.MaxBatchSize(1));

            var factory = new XuGuDbModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new XuGuDbTypeMapper()),
                new XuGuDbSqlGenerationHelper(),
                new XuGuDbUpdateSqlGenerator(new XuGuDbSqlGenerationHelper(), new XuGuDbTypeMapper()),
                new UntypedRelationalValueBufferFactoryFactory(),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.XuGuDb())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.XuGuDb())));
        }

        [Fact]
        public void MaxBatchSize_is_optional()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb("Database=Crunchie");

            var factory = new XuGuDbModificationCommandBatchFactory(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new XuGuDbTypeMapper()),
                new XuGuDbSqlGenerationHelper(),
                new XuGuDbUpdateSqlGenerator(new XuGuDbSqlGenerationHelper(), new XuGuDbTypeMapper()),
                new UntypedRelationalValueBufferFactoryFactory(),
                optionsBuilder.Options);

            var batch = factory.Create();

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.XuGuDb())));
            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.XuGuDb())));
        }
    }
}
