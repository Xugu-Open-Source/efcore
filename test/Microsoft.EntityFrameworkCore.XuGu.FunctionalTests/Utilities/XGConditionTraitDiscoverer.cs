// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities
{
    public class XGConditionTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var XGCondition = (traitAttribute as IReflectionAttributeInfo)?.Attribute as XGConditionAttribute;
            if (XGCondition == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return Enum.GetValues(typeof(XGCondition)).Cast<XGCondition>()
                .Where(c => XGCondition.Conditions.HasFlag(c))
                .Select(c => new KeyValuePair<string, string>(nameof(XGCondition), c.ToString()));
        }
    }
}
