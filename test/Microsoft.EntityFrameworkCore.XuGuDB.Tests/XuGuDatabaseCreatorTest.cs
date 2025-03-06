using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Microsoft.EntityFrameworkCore.XuGuDB.Tests
{
    public class XuGuDatabaseCreatorTest
    {
        [Fact]
        public async Task CreateTest()
        {
            await Create(1, false);
        }
        private async Task Create(int errorNumber, bool async)
        {
            try
            {
                var customServices = new ServiceCollection()
                    .AddEntityFrameworkXuGuDb()
                    .AddScoped<IXuGuDbConnection, XuGuDbConnection>()
                    .AddScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddScoped<IRelationalDatabaseCreator, XuGuDbDatabaseCreator>();

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseXuGuDb(@"IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK");
                var contextServices = TestHelpers.Instance.CreateContextServices(customServices, optionsBuilder.Options);
                var connection = contextServices.GetRequiredService<IXuGuDbConnection>();
                var creator = contextServices.GetRequiredService<IRelationalDatabaseCreator>();

                if (async)
                {
                    await creator.CreateAsync();
                }
                else
                {
                    creator.Create();
                }
            }
            catch(Exception ex)
            {
                var test = ex.Message;
            }
        }

        public static IDbContextOptions CreateOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb(@"IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK");

            return optionsBuilder.Options;
        }
    }
}
