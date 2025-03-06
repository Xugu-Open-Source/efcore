// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbValueGeneratorCache : ValueGeneratorCache, IXuGuDbValueGeneratorCache
    {
        private readonly ConcurrentDictionary<string, XuGuDbSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ConcurrentDictionary<string, XuGuDbSequenceValueGeneratorState>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual XuGuDbSequenceValueGeneratorState GetOrAddSequenceState(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var sequence = property.XuGuDb().FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence),
                sequenceName => new XuGuDbSequenceValueGeneratorState(sequence));
        }

        private static string GetSequenceName(ISequence sequence)
            => (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
    }
}
