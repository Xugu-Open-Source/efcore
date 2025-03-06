// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbTestHelpers : RelationalTestHelpers
    {
        protected XuGuDbTestHelpers()
        {
        }

        public new static XuGuDbTestHelpers Instance { get; } = new XuGuDbTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkXuGuDb();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseXuGuDb(new XGConnection("Database=DummyDatabase"));
    }
}
