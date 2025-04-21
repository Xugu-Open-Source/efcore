// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations
{
    public class ServerVersionTest
    {
        [Theory]
        [InlineData("5.7.18", ServerType.XG, "5.7.18", true)]
        [InlineData("version5.0", ServerType.XG, "5.0", false)]
        [InlineData("5.1", ServerType.XG, "5.1", false)]
        public void TestValidVersion(string input, ServerType serverType, string actualVersion, bool supportsRenameIndex)
        {
            var serverVersion = ServerVersion.Parse(input);
            Assert.Equal(serverVersion.Type, serverType);
            Assert.Equal(serverVersion.Version, new Version(actualVersion));
            Assert.Equal(serverVersion.Supports.RenameIndex, supportsRenameIndex);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("8")]
        [InlineData("8-xg")]
        public void TestInvalidVersion(string input)
        {
            Assert.Throws<InvalidOperationException>(() => ServerVersion.Parse("unknown"));
        }
    }
}
