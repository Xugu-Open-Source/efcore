// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [TraitDiscoverer("Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities.XuGuDbConditionTraitDiscoverer", "Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests")]
    public class XuGuDbConditionAttribute : Attribute, ITestCondition, ITraitAttribute
    {
        public XuGuDbCondition Conditions { get; set; }

        public XuGuDbConditionAttribute(XuGuDbCondition conditions)
        {
            Conditions = conditions;
        }

        public bool IsMet
        {
            get
            {
                var isMet = true;
                if (Conditions.HasFlag(XuGuDbCondition.SupportsSequences))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(XuGuDbCondition.SupportsSequences)) ?? true;
                }
                if (Conditions.HasFlag(XuGuDbCondition.SupportsOffset))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(XuGuDbCondition.SupportsOffset)) ?? true;
                }
                if (Conditions.HasFlag(XuGuDbCondition.IsSqlAzure))
                {
                    isMet &= TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                if (Conditions.HasFlag(XuGuDbCondition.IsNotSqlAzure))
                {
                    isMet &= !TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                return isMet;
            }
        }

        public string SkipReason =>
            // ReSharper disable once UseStringInterpolation
            string.Format("The test XuGuDb does not meet these conditions: '{0}'",
                string.Join(", ", Enum.GetValues(typeof(XuGuDbCondition))
                    .Cast<Enum>()
                    .Where(f => Conditions.HasFlag(f))
                    .Select(f => Enum.GetName(typeof(XuGuDbCondition), f))));
    }

    [Flags]
    public enum XuGuDbCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1,
        IsSqlAzure = 1 << 2,
        IsNotSqlAzure = 1 << 3
    }
}
