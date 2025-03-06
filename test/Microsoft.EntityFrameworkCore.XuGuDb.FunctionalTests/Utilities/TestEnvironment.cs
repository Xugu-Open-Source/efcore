// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build()
                .GetSection("Test:XuGuDb");
        }

        private const string DefaultConnectionString = "IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";

        public static string DefaultConnection => Config["DefaultConnection"] ?? DefaultConnectionString;

        public static bool? GetFlag(string key)
        {
            bool flag;
            return bool.TryParse(Config[key], out flag) ? flag : (bool?)null;
        }

        public static int? GetInt(string key)
        {
            int value;
            return int.TryParse(Config[key], out value) ? value : (int?)null;
        }
    }
}
