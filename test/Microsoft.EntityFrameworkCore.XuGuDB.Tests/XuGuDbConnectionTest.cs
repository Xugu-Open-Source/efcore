using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using XuguClient;
using Xunit;
using Assert = Xunit.Assert;

namespace Microsoft.EntityFrameworkCore.XuGuDB.Tests
{
    public class XuGuDbConnectionTest
    {
        [Fact]
        public void Creates_XuGuDb_connection_string()
        {
            using (var connection = new XuGuDbConnection(CreateOptions(), new Logger<XuGuDbConnection>(new LoggerFactory())))
            {
                Assert.IsType<XGConnection>(connection.DbConnection);
            }
        }

        [Fact]
        public void Can_create_system_connection()
        {
            using (var connection = new XuGuDbConnection(CreateOptions(), new Logger<XuGuDbConnection>(new LoggerFactory())))
            {
                using (var system = connection.CreateSystemConnection())
                {
                    Assert.Equal(@"IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK", system.ConnectionString);
                }
            }
        }

        [Fact]
        public void System_connection_string_none_default_command_timeout()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXuGuDb(@"IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK",
                b => b.CommandTimeout(55));

            using (var connection = new XuGuDbConnection(optionsBuilder.Options, new Logger<XuGuDbConnection>(new LoggerFactory())))
            {
                using (var system = connection.CreateSystemConnection())
                {
                    Assert.Equal(55, system.CommandTimeout);
                }
            }
        }

        [Fact]
        public void Can_create_connection()
        {
            using (var connection = new XuGuDbConnection(CreateOptions(), new Logger<XuGuDbConnection>(new LoggerFactory())))
            {
                using (var system = connection.CreateSystemConnection())
                {
                    system.Open();
                    system.Close();
                }
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
