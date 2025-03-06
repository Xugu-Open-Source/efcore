// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Relational.Tests.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Storage
{
    public class XuGuDbTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new XGCommand();

        protected override DbType DefaultParameterType
            => DbType.Object;

        [Fact]
        public void Can_create_simple_parameter()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int))
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int))
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }
    }
}
