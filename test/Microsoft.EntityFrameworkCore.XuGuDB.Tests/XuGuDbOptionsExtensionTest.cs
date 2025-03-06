using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using XuguClient;
using Xunit;
using Assert = Xunit.Assert;

namespace Microsoft.EntityFrameworkCore.XuGuDB.Tests
{
    public class XuGuDbOptionsExtensionTest
    {
        [Fact]
        public void ApplyServices_adds_XuGuDb_services()
        {
            var services = new ServiceCollection();

            new XuGuDbOptionsExtension().ApplyServices(services);

            Assert.True(services.Any(sd => sd.ServiceType == typeof(RelationalDatabase)));
        }
    }
}
