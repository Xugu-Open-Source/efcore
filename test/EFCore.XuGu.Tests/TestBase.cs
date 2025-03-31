using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu
{
    public class TestBase<TContext> : IDisposable
        where TContext : ContextBase, new()
    {
        public TestBase()
        {
            TestStore = XGTestStore.CreateInitialized(StoreName);
        }

        public virtual void Dispose() => TestStore.Dispose();

        public virtual string StoreName => GetType().Name;
        public virtual XGTestStore TestStore { get; }
        public virtual List<string> SqlCommands { get; } = new List<string>();
        public virtual string Sql => string.Join("\n\n", SqlCommands);

        public virtual TContext CreateContext(
            Action<XGDbContextOptionsBuilder> jetOptions = null,
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

            TestStore.Clean(context);

            return context;
        }
    }
}
