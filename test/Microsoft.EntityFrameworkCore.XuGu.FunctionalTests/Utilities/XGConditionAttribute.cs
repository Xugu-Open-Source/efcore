// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [TraitDiscoverer("Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities.XGConditionTraitDiscoverer", "Microsoft.EntityFrameworkCore.XG.FunctionalTests")]
    public class XGConditionAttribute : Attribute, ITestCondition, ITraitAttribute
    {
        public XGCondition Conditions { get; set; }

        public XGConditionAttribute(XGCondition conditions)
        {
            Conditions = conditions;
        }

        public bool IsMet
        {
            get
            {
                var isMet = true;
                if (Conditions.HasFlag(XGCondition.SupportsSequences))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(XGCondition.SupportsSequences)) ?? true;
                }
                if (Conditions.HasFlag(XGCondition.SupportsOffset))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(XGCondition.SupportsOffset)) ?? true;
                }
                if (Conditions.HasFlag(XGCondition.IsSqlAzure))
                {
                    isMet &= TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                if (Conditions.HasFlag(XGCondition.IsNotSqlAzure))
                {
                    isMet &= !TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                return isMet;
            }
        }

        public string SkipReason =>
            // ReSharper disable once UseStringInterpolation
            string.Format("The test XG does not meet these conditions: '{0}'",
                string.Join(", ", Enum.GetValues(typeof(XGCondition))
                    .Cast<Enum>()
                    .Where(f => Conditions.HasFlag(f))
                    .Select(f => Enum.GetName(typeof(XGCondition), f))));
    }

    [Flags]
    public enum XGCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1,
        IsSqlAzure = 1 << 2,
        IsNotSqlAzure = 1 << 3
    }
}
