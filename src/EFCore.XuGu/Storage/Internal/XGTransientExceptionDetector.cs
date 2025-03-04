// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using JetBrains.Annotations;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     Detects the exceptions caused by XG transient failures.
    /// </summary>
    public static class XGTransientExceptionDetector
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool ShouldRetryOn([NotNull] Exception ex)
        {
            if (ex is Exception XGException)
            {
                //switch (XGException.ErrorCode)
                //{
                //    // Thrown if timer queue couldn't be cleared while reading sockets
                //    case XGErrorCode.CommandTimeoutExpired:
                //    // Unable to open connection
                //    case XGErrorCode.UnableToConnectToHost:
                //    // Too many connections
                //    case XGErrorCode.ConnectionCountError:
                //    // Lock wait timeout exceeded; try restarting transaction
                //    case XGErrorCode.LockWaitTimeout:
                //    // Deadlock found when trying to get lock; try restarting transaction
                //    case XGErrorCode.LockDeadlock:
                //    // Transaction branch was rolled back: deadlock was detected
                //    case XGErrorCode.XARBDeadlock:
                //        // Retry in all cases above
                //        return true;
                //}

                switch (XGException.Message)
                {
                    // Thrown if timer queue couldn't be cleared while reading sockets
                    case "CommandTimeoutExpired":
                    // Unable to open connection
                    case "UnableToConnectToHost":
                    // Too many connections
                    case "ConnectionCountError":
                    // Lock wait timeout exceeded; try restarting transaction
                    case "LockWaitTimeout":
                    // Deadlock found when trying to get lock; try restarting transaction
                    case "LockDeadlock":
                    // Transaction branch was rolled back: deadlock was detected
                    case "XARBDeadlock":
                        // Retry in all cases above
                        return true;
                }

                // Otherwise don't retry
                return false;
            }

            return ex is TimeoutException;
        }
    }
}
