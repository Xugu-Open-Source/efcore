// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities
{
    public class XuGuDbConditionTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var XuGuDbCondition = (traitAttribute as IReflectionAttributeInfo)?.Attribute as XuGuDbConditionAttribute;
            if (XuGuDbCondition == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return Enum.GetValues(typeof(XuGuDbCondition)).Cast<XuGuDbCondition>()
                .Where(c => XuGuDbCondition.Conditions.HasFlag(c))
                .Select(c => new KeyValuePair<string, string>(nameof(XuGuDbCondition), c.ToString()));
        }
    }
}
