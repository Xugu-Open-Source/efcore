using System;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.XuGu.Tests
{
    public class XGOptionsExtensionTest
    {
        [Fact]
        public void GetServiceProviderHashCode_returns_same_value()
        {
            Assert.Equal(
                new XGOptionsExtension().Info.GetServiceProviderHashCode(),
                new XGOptionsExtension().Info.GetServiceProviderHashCode());

            Assert.Equal(
                new XGOptionsExtension()
                    .WithServerVersion(new XGServerVersion(new Version(1, 2, 3, 4)))
                    .WithDisabledBackslashEscaping()
                    .Info
                    .GetServiceProviderHashCode(),
                new XGOptionsExtension()
                    .WithServerVersion(new XGServerVersion(new Version(1, 2, 3, 4)))
                    .WithDisabledBackslashEscaping()
                    .Info
                    .GetServiceProviderHashCode());
        }
    }
}
