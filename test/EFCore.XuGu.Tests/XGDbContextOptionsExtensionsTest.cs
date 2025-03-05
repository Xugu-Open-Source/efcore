using System;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.XuGu.Tests.Tests;
using Xunit;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Internal;

namespace EntityFrameworkCore.XuGu.Tests
{
    public class XGDbContextOptionsBuilderExtensionsTest
    {

        [Fact]
        public void UseXG_without_connection_string()
        {
            var builder = new DbContextOptionsBuilder();
            var serverVersion = ServerVersion.AutoDetect(AppConfig.ConnectionString);

            builder.UseXG(serverVersion);

            var xgOptions = new XGOptions();
            xgOptions.Initialize(builder.Options);
        }

        [Fact]
        public void UseXG_without_connection_explicit_DefaultDataTypeMappings_is_applied()
        {
            var builder = new DbContextOptionsBuilder();

            builder.UseXG(
                AppConfig.ServerVersion,
                b => b.DefaultDataTypeMappings(m => m.WithClrBoolean(XGBooleanType.Bit1)));

            var xgOptions = new XGOptions();
            xgOptions.Initialize(builder.Options);

            Assert.Equal(XGBooleanType.Bit1, xgOptions.DefaultDataTypeMappings.ClrBoolean);
        }
    }
}
