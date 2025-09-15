// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests;

public class GraphUpdatesXGClientCascadeTest : GraphUpdatesXGTestBase<GraphUpdatesXGClientCascadeTest.XGFixture>
{
    public GraphUpdatesXGClientCascadeTest(XGFixture fixture)
        : base(fixture)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class XGFixture : GraphUpdatesXGFixtureBase
    {
        public override bool NoStoreCascades
            => true;

        protected override string StoreName { get; } = "GraphClientCascadeUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            foreach (var foreignKey in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(e => e.GetDeclaredForeignKeys())
                         .Where(e => e.DeleteBehavior == DeleteBehavior.Cascade))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.ClientCascade;
            }
        }
    }
}
