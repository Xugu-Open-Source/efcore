// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu
{
    public class TestBase<TContext> : IDisposable, IAsyncLifetime
        where TContext : ContextBase, new()
    {
        public async Task InitializeAsync()
        {
            TestStore = await XGTestStore.CreateInitializedAsync(StoreName);
        }

        public Task DisposeAsync()
            => Task.CompletedTask;

        public virtual void Dispose() => TestStore.Dispose();

        public virtual string StoreName => GetType().Name;
        public virtual XGTestStore TestStore { get; private set; }
        public virtual List<string> SqlCommands { get; } = new List<string>();
        public virtual string Sql => string.Join("\n\n", SqlCommands);

        public virtual async Task<TContext> CreateContext(Action<XGDbContextOptionsBuilder> jetOptions = null,
            Action<IServiceProvider, DbContextOptionsBuilder> options = null,
            Action<ModelBuilder> model = null)
        {
            var context = new TContext();

            context.Initialize(
                TestStore.Name,
                command => SqlCommands.Add(command.CommandText),
                model: model,
                options: options,
                xgOptions: jetOptions);

            await TestStore.CleanAsync(context);

            return context;
        }
    }
}
