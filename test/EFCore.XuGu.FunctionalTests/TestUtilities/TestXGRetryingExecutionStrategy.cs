// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class TestXGRetryingExecutionStrategy : XGRetryingExecutionStrategy
    {
        private const bool ErrorNumberDebugMode = false;

        // TODO: Check for correct XG error codes.
        private static readonly int[] _additionalErrorNumbers =
        {
            -1, // Physical connection is not usable
            -2, // Timeout
            1807, // Could not obtain exclusive lock on database 'model'
            42008, // Mirroring (Only when a database is deleted and another one is created in fast succession)
            42019 // CREATE DATABASE operation failed
        };

        public TestXGRetryingExecutionStrategy()
            : base(
                new DbContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseXG(TestEnvironment.DefaultConnection, AppConfig.ServerVersion).Options),
                DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestXGRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestXGRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
        {
        }

        public TestXGRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

//#pragma warning disable 162
//            if (ErrorNumberDebugMode &&
//                exception is XGException xgException)
//            {
//                var message = $"Didn't retry on {xgException.ErrorCode} ({(int)xgException.ErrorCode}/0x{(int)xgException.ErrorCode:X8})";
//                throw new InvalidOperationException(message, exception);
//            }
//#pragma warning restore 162

            return exception is InvalidOperationException invalidOperationException
                && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6.";
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }
    }
}
