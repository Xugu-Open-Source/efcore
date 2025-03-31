using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu
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
