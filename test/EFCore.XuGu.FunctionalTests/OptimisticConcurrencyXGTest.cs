// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class OptimisticConcurrencyXGTest : OptimisticConcurrencyRelationalTestBase<F1XGFixture, byte[]>
    {
        public OptimisticConcurrencyXGTest(F1XGFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        [ConditionalFact(Skip = "#588")]
        public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            return base.Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values();
        }

        [ConditionalFact(Skip = "#588")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values()
        {
            return base.Simple_concurrency_exception_can_be_resolved_with_store_values();
        }
    }
}
