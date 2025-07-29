
using System;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu
{
    public class XGOptionsExtensionTest
    {
        [Fact]
        public void GetServiceProviderHashCode_returns_same_value()
        {
            Assert.Equal(new XGOptionsExtension().GetServiceProviderHashCode(), new XGOptionsExtension().GetServiceProviderHashCode());

            Assert.Equal(new XGOptionsExtension()
                    .WithAnsiCharSetInfo(new CharSetInfo(CharSet.Latin1))
                    .WithCharSetBehavior(CharSetBehavior.AppendToAllColumns)
                    .WithUnicodeCharSetInfo(new CharSetInfo(CharSet.Utf8mb4))
                    .WithServerVersion(new ServerVersion(new Version(1, 2, 3, 4), ServerType.XG))
                    .DisableBackslashEscaping()
                    .GetServiceProviderHashCode(),
                new XGOptionsExtension()
                    .WithAnsiCharSetInfo(new CharSetInfo(CharSet.Latin1))
                    .WithCharSetBehavior(CharSetBehavior.AppendToAllColumns)
                    .WithUnicodeCharSetInfo(new CharSetInfo(CharSet.Utf8mb4))
                    .WithServerVersion(new ServerVersion(new Version(1, 2, 3, 4), ServerType.XG))
                    .DisableBackslashEscaping()
                    .GetServiceProviderHashCode());
        }
    }
}
