// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class XuGuDbFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public XuGuDbFixture()
        {
            ServiceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .BuildServiceProvider();
        }
    }
}
